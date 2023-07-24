using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Tracking
{
    /// <summary>
    /// This class is used to track data with specific properties. 
    /// Each property can have multiple "ticks", and each tick can have multiple versions. 
    /// The data is also tagged for additional organization and filtering capabilities.
    /// </summary>
    [JsonConverter(typeof(TrackerDataConverter))]
    public class TrackerData : ICloneable
    {
        public class TaggedData
        {
            public object Data { get; }
            public string[] Tags { get; }

            public TaggedData(object data, string[] tags)
            {
                Data = data;
                Tags = tags;
            }
        }

        // The main data storage, organized first by property name, then by tick, then by version.
        private Dictionary<string, SortedDictionary<int, SortedDictionary<int, TaggedData>>> data { get; set; } = new();



        private static bool MatchTags(TaggedData data, TagFilter tagFilter)
        {

            if (tagFilter == null)
            {
                return true;
            }

            // Match only if all tags in the data should be included according to the tagFilter
            return data.Tags.All(tag => tagFilter.ShouldInclude(tag));
        }


        #region IClonable / Clone

        public object Clone()
            => Clone(default);

        public TrackerData Clone(TagFilter filter = default)
        {
            var clone = new TrackerData();


            foreach (var propEntry in data)
            {
                var tickDict = new SortedDictionary<int, SortedDictionary<int, TaggedData>>();

                foreach (var tickEntry in propEntry.Value)
                {
                    var versionDict = new SortedDictionary<int, TaggedData>();

                    foreach (var versionEntry in tickEntry.Value)
                    {
                        if (MatchTags(versionEntry.Value, filter))
                        {
                            versionDict[versionEntry.Key] = new TaggedData(versionEntry.Value.Data, versionEntry.Value.Tags.ToArray());
                        }
                    }

                    if (versionDict.Count > 0) // If any versions pass the filter for this tick
                    {
                        tickDict[tickEntry.Key] = versionDict;
                    }
                }

                if (tickDict.Count > 0) // If any ticks pass the filter for this property
                {
                    clone.data[propEntry.Key] = tickDict;
                }
            }


            return clone;
        }


        public void Merge(TrackerData other)
        {
            if (other == null) return;

            foreach (var property in other.data)
            {
                foreach (var tick in property.Value)
                {
                    foreach (var version in tick.Value)
                    {
                        // Using SetValue to ensure consistent storage and handling of AllCount
                        SetValue(property.Key, tick.Key, version.Key, version.Value.Data, version.Value.Tags);
                    }
                }
            }
        }

        #endregion

        #region Count

        // TODO: Should we make param in rangerquery not have propertyname
        // as we make it optional here, but this seems confusing. But I might be overthikning this.
        public int Count(TrackerRangeQuery trackerRangeQuery)
        {
            int count = 0;

            // If PropertyName is an empty string, count all matching elements
            if (string.IsNullOrEmpty(trackerRangeQuery.PropertyName))
            {
                foreach (var propEntry in data)
                {
                    count += CountFromPropertyEntry(propEntry.Value, trackerRangeQuery);
                }
            }
            else
            {
                // If PropertyName is not an empty string, count matching elements only for that property
                if (data.TryGetValue(trackerRangeQuery.PropertyName, out var tickDict))
                {
                    count = CountFromPropertyEntry(tickDict, trackerRangeQuery);
                }
            }

            return count;
        }

        private int CountFromPropertyEntry(SortedDictionary<int, SortedDictionary<int, TaggedData>> tickDict, TrackerRangeQuery trackerRangeQuery)
        {
            int count = 0;

            foreach (var tickEntry in tickDict)
            {
                if (tickEntry.Key < trackerRangeQuery.MinTick || tickEntry.Key > trackerRangeQuery.MaxTick)
                {
                    continue;
                }

                foreach (var versionEntry in tickEntry.Value)
                {
                    if (MatchTags(versionEntry.Value, trackerRangeQuery.Filter))  // I assume TaggedData has a property called Tags
                    {
                        count++;
                    }
                }
            }

            return count;
        }


        #endregion



        /// <summary> Checks if any data exists that matches the given query. </summary>
        public bool Exists(TrackerRangeQuery trackerRangeQuery)
        {
            // Check if the property exists
            if (data.TryGetValue(trackerRangeQuery.PropertyName, out var tickDict))
            {
                // Check if any of the ticks for this property falls within the given range
                foreach (var tickKv in tickDict)
                {
                    if (tickKv.Key >= trackerRangeQuery.MinTick && tickKv.Key <= trackerRangeQuery.MaxTick)
                    {
                        // Check for any matching tags
                        var matchingVersions = tickKv.Value
                            .Where(versionKv => MatchTags(versionKv.Value, trackerRangeQuery.Filter))
                            .OrderByDescending(versionKv => versionKv.Key);  // Highest version first

                        if (matchingVersions.Any())
                        {
                            return true;
                        }
                    }
                }
            }

            // The property does not exist
            return false;
        }



        /// <summary> Sets the value for a given property, tick, and version. </summary>
        public void SetValue(string propertyName, int tick, int version, object value, string[] tags)
        {
            // Ensure the dictionaries are properly initialized for the given property and tick.
            if (!data.ContainsKey(propertyName))
            {
                data[propertyName] = new SortedDictionary<int, SortedDictionary<int, TaggedData>>();
            }

            var tickDict = data[propertyName];

            if (!tickDict.ContainsKey(tick))
            {
                tickDict[tick] = new SortedDictionary<int, TaggedData>();
            }

            var versionDict = tickDict[tick];

            versionDict[version] = new TaggedData(value, tags);
        }


        #region Get

        public IEnumerable<string> GetProperties(TagFilter filter = default)
        {
            return data
                .Where(kv => kv.Value.Any(tickKv => tickKv.Value.Any(versionKv => MatchTags(versionKv.Value, filter))))
                .Select(kv => kv.Key);

        }

        public IEnumerable<int> GetTicks(string propertyName, TagFilter filter = default)
        {
            if (data.TryGetValue(propertyName, out var tickDict))
            {
                return tickDict
                    .Where(tickKv => tickKv.Value.Any(versionKv => MatchTags(versionKv.Value, filter)))
                    .Select(tickKv => tickKv.Key);
            }

            return Enumerable.Empty<int>(); // return an empty sequence if the property does not exist
        }

        public IEnumerable<int> GetVersions(string propertyName, int tick, TagFilter filter = default)
        {
            if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
            {
                return versionDict
                    .Where(versionKv => MatchTags(versionKv.Value, filter))
                    .Select(versionKv => versionKv.Key);
            }

            return Enumerable.Empty<int>(); // return an empty sequence if the tick does not exist
        }

        public TaggedData GetTaggedData(string propertyName, int tick, int version, TagFilter filter = default)
        {
            if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
            {
                if (versionDict.TryGetValue(version, out var taggedData) && MatchTags(taggedData, filter))
                {
                    return taggedData;
                }
            }

            // If no matching TaggedData is found, return null
            return null;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes all versions for a given property and tick.
        /// </summary>
        public void RemoveValue(string propertyName, int tick)
        {
            if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
            {
                tickDict.Remove(tick);

                // If there are no more ticks left for this property, remove the property as well
                if (tickDict.Count == 0)
                {
                    data.Remove(propertyName);
                }
            }
        }



        /// <summary> 
        /// Removes the value for a given property, tick, and version. 
        /// </summary>
        public void RemoveSpecificValue(string propertyName, int tick, int version)
        {
            if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict) && versionDict.ContainsKey(version))
            {
                versionDict.Remove(version);

                // If there are no more versions left for this tick, remove the tick as well
                if (versionDict.Count == 0)
                {
                    tickDict.Remove(tick);
                }

                // If there are no more ticks left for this property, remove the property as well
                if (tickDict.Count == 0)
                {
                    data.Remove(propertyName);
                }
            }
        }

        /// <summary>
        /// Removes all ticks and versions for a given property.
        /// </summary>
        public void RemoveAllPropertyValues(string propertyName)
        {
            if (data.TryGetValue(propertyName, out var tickDict))
            {
                data.Remove(propertyName);
            }
        }

        #endregion


        #region TryGet Latest

        public bool TryGetLatestValue(TrackerQuery query, out TrackerQueryResult result)
            => TryGetLatestValueHelper(query.PropertyName, query.Filter, new[] { query.Tick }, out result);

        public bool TryGetLatestValueAtNextAvailableTick(TrackerRangeQuery query, out TrackerQueryResult result)
        {
            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var nextTicks = tickDict.Keys.Where(t => t > query.MinTick && t <= query.MaxTick).OrderBy(t => t); // Ensure ticks are in ascending order and within bounds
                return TryGetLatestValueHelper(query.PropertyName, query.Filter, nextTicks, out result);
            }

            result = null;
            return false;
        }

        public bool TryGetLatestValueAtPreviousAvailableTick(TrackerRangeQuery query, out TrackerQueryResult result)
        {
            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var previousTicks = tickDict.Keys.Where(t => t < query.MaxTick && t >= query.MinTick).OrderByDescending(t => t); // Ensure ticks are in descending order and within bounds
                return TryGetLatestValueHelper(query.PropertyName, query.Filter, previousTicks, out result);
            }

            result = null;
            return false;
        }

        private bool TryGetLatestValueHelper(string propertyName, TagFilter filter, IEnumerable<int> ticks, out TrackerQueryResult result)
        {
            result = new TrackerQueryResult();

            if (data.TryGetValue(propertyName, out var tickDict))
            {
                foreach (var tick in ticks)
                {
                    if (tickDict.TryGetValue(tick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, filter))  // Filter on tags
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any matching versions were found
                        if (matchingVersions.Any())
                        {
                            var highestVersion = matchingVersions.First();
                            result.Value = new KeyValuePair<TrackerKey, object>(new TrackerKey
                            {
                                PropertyName = propertyName,
                                Tick = tick,
                                Version = highestVersion.Key,
                                Tags = highestVersion.Value.Tags
                            }, highestVersion.Value.Data);

                            return true;
                        }
                    }
                }
            }

            // If no values with matching tags are found after examining all ticks, return false
            return false;
        }


        #endregion

        #region TryGet Detailed

        public bool TryGetDetailedValue(TrackerQuery query, out TrackerDetailedQueryResult result)
            => TryGetDetailedValues(query.PropertyName, query.Filter, new[] { query.Tick }, out result);

        public bool TryGetDetailedValuesAtNextAvailableTick(TrackerRangeQuery query, out TrackerDetailedQueryResult result)
        {
            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var nextTicks = tickDict.Keys.Where(t => t > query.MinTick && t <= query.MaxTick).OrderBy(t => t); // Ensure ticks are in ascending order and within bounds
                return TryGetDetailedValues(query.PropertyName, query.Filter, nextTicks, out result);
            }

            result = null;
            return false;
        }

        public bool TryGetDetailedValuesAtPreviousAvailableTick(TrackerRangeQuery query, out TrackerDetailedQueryResult result)
        {
            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var previousTicks = tickDict.Keys.Where(t => t < query.MaxTick && t >= query.MinTick).OrderByDescending(t => t); // Ensure ticks are in descending order and within bounds
                return TryGetDetailedValues(query.PropertyName, query.Filter, previousTicks, out result);
            }

            result = null;
            return false;
        }

        private bool TryGetDetailedValues(string propertyName, TagFilter filter, IEnumerable<int> ticks, out TrackerDetailedQueryResult result)
        {
            result = new TrackerDetailedQueryResult();

            if (data.TryGetValue(propertyName, out var tickDict))
            {
                foreach (var tick in ticks)
                {
                    if (tickDict.TryGetValue(tick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, filter))  // Filter on tags
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            result.Values = matchingVersions.Select(kv => new KeyValuePair<TrackerKey, object>(
                                new TrackerKey { PropertyName = propertyName, Tick = tick, Version = kv.Key, Tags = kv.Value.Tags },
                                kv.Value.Data
                            ));
                            return true;
                        }
                    }
                }
            }

            // If no values with matching tags are found after examining all ticks, return false
            return false;
        }




        #endregion


    }


}
