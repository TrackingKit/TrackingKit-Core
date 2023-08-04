using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tracking
{
    public class TrackerMetaData
    {
        public object Data { get; }
        public IReadOnlyCollection<string> Tags { get; }

        internal TrackerMetaData(object data, IEnumerable<string> tags)
        {
            Data = data;
            Tags = new HashSet<string>(tags);
        }
    }

    internal class TrackerStorage : ICloneable
    {


        internal TrackerStorage()
        {
            Data = new();
        }

        private readonly Dictionary<string, SortedDictionary<int, SortedDictionary<int, TrackerMetaData>>> Data;

        public IEnumerable<string> Keys => Data.Keys;

        public void SetValue(string propertyName, int tick, int version, object value, IEnumerable<string> tags = default)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

            if (!Data.ContainsKey(propertyName))
            {
                Data[propertyName] = new SortedDictionary<int, SortedDictionary<int, TrackerMetaData>>();
            }

            var tickDict = Data[propertyName];

            if (!tickDict.ContainsKey(tick))
            {
                tickDict[tick] = new SortedDictionary<int, TrackerMetaData>();
            }

            var versionDict = tickDict[tick];

            versionDict[version] = new TrackerMetaData(value, tags);
        }


        public bool RemoveValues(string propertyName, int tick)
        {
            if (Data.TryGetValue(propertyName, out var tickDict))
            {
                return tickDict.Remove(tick);
            }

            return false; // nothing was removed
        }


        public bool RemoveSpecificValue(string propertyName, int tick, int version)
        {
            if (Data.TryGetValue(propertyName, out var tickDict) &&
                tickDict.TryGetValue(tick, out var versionDict))
            {
                return versionDict.Remove(version);
            }

            return false; // nothing was removed
        }

        public object Clone()
        {
            var cloned = new TrackerStorage();
            foreach (var property in Data)
            {
                foreach (var tick in property.Value)
                {
                    foreach (var version in tick.Value)
                    {
                        cloned.SetValue(property.Key, tick.Key, version.Key, version.Value.Data, version.Value.Tags);
                    }
                }
            }
            return cloned;
        }



        #region Common Internal Function

        /// <summary>
        /// Process tracking data for a property given the property data, tick range, and conditions for inclusion and stopping.
        /// Within the tick range, the processing order depends on the value of the "descending" parameter. If "descending" is false,
        /// ticks are processed from the lowest to highest, and within each tick, versions are processed from the lowest to highest.
        /// If "descending" is true, ticks are processed from the highest to lowest, and within each tick, versions are processed from
        /// the highest to lowest.
        /// </summary>
        /// <param name="propertyData">The dictionary of the property's tracking data</param>
        /// <param name="minTick">The minimum tick for the range</param>
        /// <param name="maxTick">The maximum tick for the range</param>
        /// <param name="ascending">Indicates if the range should be processed in descending order. If true, ticks and versions within ticks are processed from highest to lowest. If false, ticks and versions within ticks are processed from lowest to highest.</param>
        /// <param name="shouldInclude">The condition for including a version in the result</param>
        /// <param name="stopCondition">The condition for stopping the processing</param>
        /// <param name="result">The result dictionary to be filled with included versions</param>
        /// <param name="accumulatedSoFar">The count of versions accumulated so far</param>
        /// <param name="useIndexing">Indicates if indexing should be used to get the versions and data</param>
        private void ProcessTrackingData(
            SortedDictionary<int, SortedDictionary<int, TrackerMetaData>> propertyData,
            int minTick, int maxTick, bool ascending,
            Func<TrackerInformation, bool> shouldInclude,
            Func<StopConditionInformation, bool> stopCondition,
            SortedDictionary<int, SortedDictionary<int, TrackerMetaData>> result,
            ref int accumulatedSoFar,
            bool useIndexing = false
        )
        {
            // Generate the tick range, either in ascending or descending order
            IEnumerable<int> tickRange = ascending
                ? Enumerable.Range(minTick, maxTick - minTick + 1)
                : Enumerable.Range(minTick, maxTick - minTick + 1).Reverse();

            // Iterate over each tick in the range
            foreach (int tick in tickRange)
            {
                SortedDictionary<int, TrackerMetaData> versionsAndData;

                // Get the versions and data for the current tick, using indexing or TryGetValue
                if (useIndexing)
                {
                    versionsAndData = propertyData[tick];
                }
                else if (!propertyData.TryGetValue(tick, out versionsAndData))
                {
                    continue;
                }

                // If descending, reverse the versions and data
                var orderedVersionsAndData = ascending
                    ? new Dictionary<int, TrackerMetaData>(versionsAndData.OrderBy(kvp => kvp.Key))
                    : new Dictionary<int, TrackerMetaData>(versionsAndData.OrderByDescending(kvp => kvp.Key));


                // Iterate over each version and its data (in the required order)
                foreach (var versionAndData in orderedVersionsAndData)
                {
                    // Create a tracking data object for the current version
                    var trackingData = new TrackerInformation(tick, versionAndData.Key, versionAndData.Value);

                    // Include the version in the result if it satisfies the include condition
                    if (shouldInclude?.Invoke(trackingData) ?? true)
                    {
                        // Add a new entry for the current tick if it does not already exist in the result
                        if (!result.ContainsKey(tick))
                            result[tick] = new SortedDictionary<int, TrackerMetaData>();

                        // Add the current version and its data to the result
                        result[tick][versionAndData.Key] = versionAndData.Value;
                        accumulatedSoFar++;

                        // Stop the processing if the stop condition is satisfied
                        if (stopCondition?.Invoke(new StopConditionInformation(tick, versionAndData.Key, versionAndData.Value, accumulatedSoFar)) ?? false)
                        {
                            return;
                        }
                    }
                }
            }
        }


        #endregion

        #region Get Property Filtered

        private SortedDictionary<int, SortedDictionary<int, TrackerMetaData>> GetPropertyValueInternal(
            SortedDictionary<int, SortedDictionary<int, TrackerMetaData>> propertyData,
            int minTick, int maxTick, bool ascending,
            Func<TrackerInformation, bool> shouldInclude,
            Func<StopConditionInformation, bool> stopCondition,
            bool useIndexing = false
            )
        {
            var result = new SortedDictionary<int, SortedDictionary<int, TrackerMetaData>>();
            int accumulatedSoFar = 0;

            ProcessTrackingData(propertyData, minTick, maxTick, ascending, shouldInclude, stopCondition, result, ref accumulatedSoFar, useIndexing);

            return result;
        }

        /// <summary>
        /// Retrieves the tracking data for a specific property, filtered by a given tick range and conditions for inclusion and stopping.
        /// The processing order is controlled by the "descending" parameter. If false, ticks are processed from lowest to highest, 
        /// and within each tick, versions are processed from lowest to highest. If true, the order is reversed.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve tracking data for</param>
        /// <param name="minTick">The minimum tick for the range (default is int.MinValue)</param>
        /// <param name="maxTick">The maximum tick for the range (default is int.MaxValue)</param>
        /// <param name="ascending">Indicates if the range should be processed in ascending order (default is true)</param>
        /// <param name="shouldInclude">The condition for including a version in the result (default is null)</param>
        /// <param name="stopCondition">The condition for stopping the processing (default is null)</param>
        /// <returns>A read-only dictionary of tracking data for the specified property</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the property is not found</exception>
        public IReadOnlyDictionary<int, IReadOnlyDictionary<int, TrackerMetaData>> GetPropertyValue(
            string propertyName, int minTick = int.MinValue, int maxTick = int.MaxValue, bool ascending = true,
            Func<TrackerInformation, bool> shouldInclude = null,
            Func<StopConditionInformation, bool> stopCondition = null)
        {
            // Check if the property exists in the data, and throw an exception if it does not
            if (!Data.TryGetValue(propertyName, out var propertyData))
            {
                throw new KeyNotFoundException($"Property '{propertyName}' not found.");
            }

            // Get the property's tracking data filtered according to the specified conditions
            var sortedData = GetPropertyValueInternal(propertyData, minTick, maxTick, ascending, shouldInclude, stopCondition, useIndexing: true);
            return sortedData.ToDictionary(k => k.Key, v => (IReadOnlyDictionary<int, TrackerMetaData>)new ReadOnlyDictionary<int, TrackerMetaData>(v.Value));
        }


        /// <summary>
        /// Attempts to retrieve the tracking data for a specific property, filtered by a given tick range and conditions for inclusion and stopping.
        /// The processing order is controlled by the "descending" parameter. If false, ticks are processed from lowest to highest, 
        /// and within each tick, versions are processed from lowest to highest. If true, the order is reversed.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve tracking data for</param>
        /// <param name="result">Output parameter containing the result if the property is found</param>
        /// <param name="minTick">The minimum tick for the range (default is int.MinValue)</param>
        /// <param name="maxTick">The maximum tick for the range (default is int.MaxValue)</param>
        /// <param name="ascending">Indicates if the range should be processed in ascending order (default is true)</param>
        /// <param name="shouldInclude">The condition for including a version in the result (default is null)</param>
        /// <param name="stopCondition">The condition for stopping the processing (default is null)</param>
        /// <returns>True if the property is found, otherwise false</returns>
        public bool TryGetPropertyValue(
            string propertyName,
            out IReadOnlyDictionary<int, IReadOnlyDictionary<int, TrackerMetaData>> result,
            int minTick = int.MinValue,
            int maxTick = int.MaxValue,
            bool ascending = true,
            Func<TrackerInformation, bool> shouldInclude = null,
            Func<StopConditionInformation, bool> stopCondition = null)
        {
            result = null;

            if (Data.TryGetValue(propertyName, out var propertyData))
            {
                var sortedData = GetPropertyValueInternal(propertyData, minTick, maxTick, ascending, shouldInclude, stopCondition);
                if (sortedData != null && sortedData.Any())
                {
                    result = sortedData.ToDictionary(k => k.Key, v => (IReadOnlyDictionary<int, TrackerMetaData>)new ReadOnlyDictionary<int, TrackerMetaData>(v.Value));
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Get Filtered Across All Properties

        /// <summary>
        /// Retrieves tracking data for all properties, filtered by a given tick range and conditions for inclusion and stopping.
        /// The processing order is controlled by the "descending" parameter. If false, ticks are processed from lowest to highest,
        /// and within each tick, versions are processed from lowest to highest. If true, the order is reversed.
        /// </summary>
        /// <param name="minTick">The minimum tick for the range (default is int.MinValue)</param>
        /// <param name="maxTick">The maximum tick for the range (default is int.MaxValue)</param>
        /// <param name="ascending">Indicates if the range should be processed in ascending order (default is true)</param>
        /// <param name="shouldInclude">The condition for including a version in the result (default is null)</param>
        /// <param name="stopCondition">The condition for stopping the processing (default is null)</param>
        /// <param name="useIndexing">Indicates if indexing should be used to get the versions and data (default is false)</param>
        /// <returns>A read-only dictionary containing tracking data for all properties</returns>
        private IReadOnlyDictionary<string, IReadOnlyDictionary<int, IReadOnlyDictionary<int, TrackerMetaData>>> GetValueInternal
        (
            int minTick = int.MinValue, int maxTick = int.MaxValue, bool ascending = true,
            Func<TrackingPropertyInformation, bool> shouldInclude = null,
            Func<StopPropertyConditionInformation, bool> stopCondition = null,
            bool useIndexing = false)
        {
            var result = new Dictionary<string, SortedDictionary<int, SortedDictionary<int, TrackerMetaData>>>();
            int accumulatedSoFar = 0;

            foreach (var propertyName in Data.Keys)
            {
                var propertyData = useIndexing ? Data[propertyName] : Data.GetValueOrDefault(propertyName);

                result[propertyName] = new SortedDictionary<int, SortedDictionary<int, TrackerMetaData>>();

                ProcessTrackingData(propertyData, minTick, maxTick, ascending,
                    trackingData => shouldInclude?.Invoke(new TrackingPropertyInformation(propertyName, trackingData.Tick, trackingData.Version, trackingData.Data, trackingData.Tags)) ?? true,
                    stopInfo => stopCondition?.Invoke(new StopPropertyConditionInformation(propertyName, stopInfo.Tick, stopInfo.Version, stopInfo.Data, stopInfo.Tags, accumulatedSoFar)) ?? false,
                    result[propertyName], ref accumulatedSoFar, useIndexing);
            }

            return result.ToDictionary(k => k.Key, v => (IReadOnlyDictionary<int, IReadOnlyDictionary<int, TrackerMetaData>>)new ReadOnlyDictionary<int, IReadOnlyDictionary<int, TrackerMetaData>>(v.Value.ToDictionary(k1 => k1.Key, v1 => (IReadOnlyDictionary<int, TrackerMetaData>)new ReadOnlyDictionary<int, TrackerMetaData>(v1.Value))));
        }

        /// <summary>
        /// Retrieves tracking data for all properties, filtered by a given tick range and conditions for inclusion and stopping.
        /// The processing order is controlled by the "descending" parameter. If false, ticks are processed from lowest to highest,
        /// and within each tick, versions are processed from lowest to highest. If true, the order is reversed.
        /// </summary>
        /// <param name="minTick">The minimum tick for the range (default is int.MinValue)</param>
        /// <param name="maxTick">The maximum tick for the range (default is int.MaxValue)</param>
        /// <param name="ascending">Indicates if the range should be processed in ascending order (default is true)</param>
        /// <param name="shouldInclude">The condition for including a version in the result (default is null)</param>
        /// <param name="stopCondition">The condition for stopping the processing (default is null)</param>
        /// <returns>A read-only dictionary containing tracking data for all properties</returns>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<int, IReadOnlyDictionary<int, TrackerMetaData>>> GetValue(
            int minTick = int.MinValue, int maxTick = int.MaxValue, bool ascending = true,
            Func<TrackingPropertyInformation, bool> shouldInclude = null,
            Func<StopPropertyConditionInformation, bool> stopCondition = null)
        {
            return GetValueInternal(minTick, maxTick, ascending, shouldInclude, stopCondition, useIndexing: true);
        }


        /// <summary>
        /// Attempts to retrieve tracking data for all properties, filtered by a given tick range and conditions for inclusion and stopping.
        /// The processing order is controlled by the "descending" parameter. If false, ticks are processed from lowest to highest,
        /// and within each tick, versions are processed from lowest to highest. If true, the order is reversed.
        /// </summary>
        /// <param name="result">Output parameter containing the result</param>
        /// <param name="minTick">The minimum tick for the range (default is int.MinValue)</param>
        /// <param name="maxTick">The maximum tick for the range (default is int.MaxValue)</param>
        /// <param name="ascending">Indicates if the range should be processed in ascending order (default is true)</param>
        /// <param name="shouldInclude">The condition for including a version in the result (default is null)</param>
        /// <param name="stopCondition">The condition for stopping the processing (default is null)</param>
        /// <returns>True if there are any results, otherwise false</returns>
        public bool TryGetValue(
            out IReadOnlyDictionary<string, IReadOnlyDictionary<int, IReadOnlyDictionary<int, TrackerMetaData>>> result, int minTick = int.MinValue, int maxTick = int.MaxValue,
            bool ascending = true, Func<TrackingPropertyInformation, bool> shouldInclude = null,
            Func<StopPropertyConditionInformation, bool> stopCondition = null)
        {
            result = GetValueInternal(minTick, maxTick, ascending, shouldInclude, stopCondition);
            return result.Count > 0;
        }

        #endregion









    }


}
