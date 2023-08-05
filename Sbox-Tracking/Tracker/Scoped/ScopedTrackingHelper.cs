using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    /// <summary>
    /// The ScopedTrackingHelper class provides functionalities for tracking specific properties within a scoped context.
    /// </summary>
    /// <remarks>
    /// <para title="Raw Methods">
    /// Raw methods perform tracking without considering the scoped context settings. They deal directly with the raw data,
    /// ignoring any filters, modifications, or customizations defined by the ScopedTickSettings. This enables direct and
    /// unmodified access to the tracking data, providing a low-level view of the information.
    /// </para>
    /// <para title="Normal Methods">
    /// In contrast, normal methods consider the ScopedTickSettings in their operations. They take into account the current
    /// context, applying any rules, filters, or transformations defined within the settings. This leads to a more controlled
    /// and customized view of the tracking data, allowing for tailored interactions within the specific scope.
    /// </para>
    /// <para title="Conclusion">
    /// Together, these two types of methods provide a flexible framework for interacting with tracking data, accommodating
    /// both unfiltered access and customized, context-aware operations.
    /// </para>
    /// </remarks>
    internal class ScopedTrackingHelper : IValid
    {
        public bool IsValid => Storage == null;


        protected readonly TrackerStorage Storage;



        internal ScopedTrackingHelper(TrackerStorage storageData)
        {
            Storage = storageData;
        }








        // TODO: Some count cache mechanism?


        protected int RawCount(int minTick, int maxTick)
        {
            
            if(Storage.TryGetValue(out var result, minTick, maxTick))
            {
                // Assuming you want to count the number of TrackerMetaData objects
                int sum = 0;
                foreach (var property in result)
                {
                    foreach (var tick in property.Value)
                    {
                        sum += tick.Value.Count; // Add the count of TrackerMetaData for this tick
                    }
                }

                return sum;
            }

            return 0;
        }

        


        #region Exists

        protected bool RawPropertyExists(string propertyName, int minTick, int maxTick)
        {
            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0;

            return Storage.TryGetPropertyValue(propertyName, out var result, minTick: minTick, maxTick: maxTick, stopCondition: stopCondition);
        }

        protected bool RawExists(int minTick, int maxTick)
        {
            Func<StopPropertyConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0;

            return Storage.TryGetValue(out var result, minTick: minTick, maxTick: maxTick, stopCondition: stopCondition);
        }




        #endregion





        #region Raw TryGet Latest


        // TODO Count one? check hash if need to do again etc

        protected bool TryGetRawLatestValueAtTick(string propertyName, int tick, out (int Version, TrackerMetaData Data)? output, IReadOnlyTagFilter filter)
        {
            if (Storage.TryGetPropertyValue(
                propertyName, out var value, minTick: tick, maxTick: tick,
                shouldInclude: x => (filter?.ShouldIncludes(x.Tags) ?? true)))
            {
                if (value.TryGetValue(tick, out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    // Get the highest key value directly
                    var highestKey = trackerMetaData.Keys.Max();
                    var correspondingValue = trackerMetaData[highestKey];

                    // Assuming you want to use highestKey as Version
                    output = (highestKey, correspondingValue);
                    return true;
                }
            }

            output = null;
            return false;
        }




        protected bool TryGetRawLatestValueAtOrNextTick(string propertyName, out int outputTick, out (int Version, TrackerMetaData Data)? output, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            outputTick = 0; // Initialize outputTick

            int? lastTick = null;

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = filter?.ShouldIncludes(x.Tags) ?? true;
                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition))
            {
                if (value.Keys.Count() == 1 && value.TryGetValue(value.Keys.First(), out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    var highestKey = trackerMetaData.Keys.Max();
                    var correspondingValue = trackerMetaData[highestKey];

                    output = (highestKey, correspondingValue);
                    outputTick = value.Keys.First(); // Set the outputTick value
                    return true;
                }
            }

            output = null;
            return false;
        }


        protected bool TryGetRawLatestValueAtOrPreviousTick(string propertyName, out int outputTick, out (int Version, TrackerMetaData Data)? output, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            outputTick = 0; // Initialize outputTick
            int? lastTick = null;

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = filter?.ShouldIncludes(x.Tags) ?? true;
                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition, ascending: false))
            {
                if (value.Keys.Count() == 1 && value.TryGetValue(value.Keys.First(), out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    var highestKey = trackerMetaData.Keys.Max();
                    var correspondingValue = trackerMetaData[highestKey];

                    output = (highestKey, correspondingValue);
                    outputTick = value.Keys.First(); // Set the outputTick value
                    return true;
                }
            }

            output = null;
            return false;
        }





        #endregion

        #region Raw TryGet Detailed

        protected bool TryGetRawDetailedValues(string propertyName, int minTick, int maxTick, out int outputTick, out IEnumerable<(int Version, TrackerMetaData Data)> output, IReadOnlyTagFilter filter, TickSearchMode searchMode)
        {
            outputTick = 0; // Initialize outputTick
            List<(int Version, TrackerMetaData Data)> results = new List<(int Version, TrackerMetaData Data)>();
            int? lastTick = null;

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = filter?.ShouldIncludes(x.Tags) ?? true;

                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            bool ascending = searchMode != TickSearchMode.AtOrPreviousTick;

            if (searchMode == TickSearchMode.AtTick)
            {
                minTick = maxTick;
            }

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition, ascending: ascending))
            {
                var firstTickData = value.FirstOrDefault();
                if (firstTickData.Value != null)
                {
                    results.AddRange(firstTickData.Value.Select(kvp => (Version: kvp.Key, Data: kvp.Value)));
                    outputTick = firstTickData.Key; // Set the outputTick value
                }
            }

            if (results.Any())
            {
                output = results;
                return true;
            }

            output = null;
            return false;
        }

        public enum TickSearchMode
        {
            AtTick,
            AtOrNextTick,
            AtOrPreviousTick
        }


        [Obsolete("Too specific")]
        protected bool TryGetRawDetailedValueAtTick(string propertyName, int tick, out IEnumerable<(int Version, TrackerMetaData Data)> output, IReadOnlyTagFilter filter)
        {
            if (Storage.TryGetPropertyValue(
                propertyName, out var value, minTick: tick, maxTick: tick,
                shouldInclude: x => (filter?.ShouldIncludes(x.Tags) ?? true)))
            {
                if (value.TryGetValue(tick, out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    output = trackerMetaData.Select(kvp => (Version: kvp.Key, Data: kvp.Value)).ToList();
                    return true;
                }
            }

            output = null;
            return false;
        }

        [Obsolete("Too specific")]
        protected bool TryGetRawDetailedValuesAtOrNextTick(string propertyName, out int outputTick, out IEnumerable<(int Version, TrackerMetaData Data)> output, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            outputTick = 0; // Initialize outputTick
            int? lastTick = null;
            List<(int Version, TrackerMetaData Data)> results = new List<(int Version, TrackerMetaData Data)>();

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = filter?.ShouldIncludes(x.Tags) ?? true;

                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition))
            {
                var firstTickData = value.FirstOrDefault();
                if (firstTickData.Value != null)
                {
                    results.AddRange(firstTickData.Value.Select(kvp => (Version: kvp.Key, Data: kvp.Value)));
                    outputTick = firstTickData.Key; // Set the outputTick value
                }
            }

            if (results.Any())
            {
                output = results;
                return true;
            }

            output = null;
            return false;
        }

        [Obsolete("Too specific")]
        protected bool TryGetRawDetailedValuesAtOrPreviousTick(string propertyName, out int outputTick, out IEnumerable<(int Version, TrackerMetaData Data)> output, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            outputTick = 0; // Initialize outputTick
            int? lastTick = null;
            List<(int Version, TrackerMetaData Data)> results = new List<(int Version, TrackerMetaData Data)>();

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = filter?.ShouldIncludes(x.Tags) ?? true;
                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition, ascending: false))
            {
                var firstTickData = value.FirstOrDefault();
                if (firstTickData.Value != null)
                {
                    results.AddRange(firstTickData.Value.Select(kvp => (Version: kvp.Key, Data: kvp.Value)));
                    outputTick = firstTickData.Key; // Set the outputTick value
                }
            }

            if (results.Any())
            {
                output = results;
                return true;
            }

            output = null;
            return false;
        }



        #endregion


        


    }
}
