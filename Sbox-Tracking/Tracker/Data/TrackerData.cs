using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;


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

            clone.AllCount = clone.data.Sum(propEntry => propEntry.Value.Sum(tickEntry => tickEntry.Value.Count));

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


        // The main data storage, organized first by property name, then by tick, then by version.
        internal Dictionary<string, SortedDictionary<int, SortedDictionary<int, TaggedData>>> data { get; set; } = new();

        // TODO: Should track count inside.


        public int Count(TagFilter filter = default)
        {
            int count = 0;

            foreach (var propEntry in data)
            {
                foreach (var tickEntry in propEntry.Value)
                {
                    foreach (var versionEntry in tickEntry.Value)
                    {
                        if (MatchTags(versionEntry.Value, filter))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }



        /// <summary> Count of all tracked elements across all properties, ticks, and versions. </summary>
        public int AllCount { get; private set; }

        private static bool MatchTags(TaggedData data, TagFilter tagFilter)
        {

            if (tagFilter == null)
            {
                return true;
            }

            // Match only if all tags in the data should be included according to the tagFilter
            return data.Tags.All(tag => tagFilter.ShouldInclude(tag));
        }


        /// <summary> Gets all distinct property keys present in the data. </summary>
        public IEnumerable<string> AllDistinctKeys 
            => data.Keys;


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

            if (!versionDict.ContainsKey(version)) // If the version does not exist yet, increment count
            {
                AllCount++;
            }

            versionDict[version] = new TaggedData(value, tags);
        }

        /// <summary>
        /// Removes all versions for a given property and tick.
        /// </summary>
        public void RemoveValue(string propertyName, int tick)
        {
            if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
            {
                // Decrease the overall count by the number of versions for this tick
                AllCount -= versionDict.Count;

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
                AllCount--;

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
                // Decrease the overall count by the total number of versions across all ticks for this property
                foreach (var versionDict in tickDict.Values)
                {
                    AllCount -= versionDict.Count;
                }

                data.Remove(propertyName);
            }
        }


        // TODO: Specific version, tick?

        #region Latest

        /// <summary> Attempts to get the latest value for a given property and tick. </summary>
        public bool TryGetLatestValue(TrackerQuery query, out TrackerQueryResult result)
        {
            result = new TrackerQueryResult();

            if (data.TryGetValue(query.PropertyName, out var tickDict) && tickDict.TryGetValue(query.Tick, out var versionDict))
            {
                // This will get all versions that match the provided tag filter, ordered in descending order (highest version number first)
                var matchingVersions = versionDict
                    .Where(kv => MatchTags(kv.Value, query.Filter))  // Filter on tags
                    .OrderByDescending(kv => kv.Key);  // Highest version first

                // If any matching versions were found
                if (matchingVersions.Any())
                {
                    var highestVersion = matchingVersions.First();
                    result.Value = new KeyValuePair<TrackerKey, object>(new TrackerKey
                    {
                        PropertyName = query.PropertyName,
                        Tick = query.Tick,
                        Version = highestVersion.Key,
                        Tags = highestVersion.Value.Tags
                    }, highestVersion.Value.Data);
                    return true;
                }
            }

            // If no values with matching tags are found, return false
            return false;
        }

        public bool TryGetLatestValueAtNextAvailableTick(TrackerRangeQuery query, out TrackerQueryResult result)
        {
            result = new TrackerQueryResult();

            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var nextTicks = tickDict.Keys.Where(t => t > query.MinTick && t <= query.MaxTick).OrderBy(t => t); // Ensure ticks are in ascending order and within bounds
                foreach (var nextTick in nextTicks)
                {
                    if (tickDict.TryGetValue(nextTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, query.Filter))
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            var highestVersion = matchingVersions.First();


                            result.Value = new KeyValuePair<TrackerKey, object>(new TrackerKey
                            {
                                PropertyName = query.PropertyName,
                                Tick = nextTick,
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

        public bool TryGetLatestValueAtPreviousAvailableTick(TrackerRangeQuery query, out TrackerQueryResult result)
        {
            result = new TrackerQueryResult();

            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var previousTicks = tickDict.Keys.Where(t => t < query.MaxTick && t >= query.MinTick).OrderByDescending(t => t); // Ensure ticks are in descending order and within bounds
                foreach (var previousTick in previousTicks)
                {
                    if (tickDict.TryGetValue(previousTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, query.Filter))
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            var highestVersion = matchingVersions.First();
                            result.Value = new KeyValuePair<TrackerKey, object>(new TrackerKey
                            {
                                PropertyName = query.PropertyName,
                                Tick = previousTick,
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

        #region Detailed

        public bool TryGetDetailedValue(TrackerQuery query, out TrackerDetailedQueryResult result)
        {
            result = new TrackerDetailedQueryResult();

            if (data.TryGetValue(query.PropertyName, out var tickDict) && tickDict.TryGetValue(query.Tick, out var versionDict))
            {
                var matchingVersions = versionDict
                    .Where(kv => MatchTags(kv.Value, query.Filter))  // Filter on tags
                    .OrderByDescending(kv => kv.Key);  // Highest version first

                // If any values with matching tags are found, return them
                if (matchingVersions.Any())
                {
                    result.Values = matchingVersions.Select(kv => new KeyValuePair<TrackerKey, object>(
                        new TrackerKey { PropertyName = query.PropertyName, Tick = query.Tick, Version = kv.Key, Tags = kv.Value.Tags },
                        kv.Value.Data
                    ));
                    return true;
                }
            }

            // If no values with matching tags are found, return false
            return false;
        }

        public bool TryGetDetailedValuesAtNextAvailableTick(TrackerRangeQuery query, out TrackerDetailedQueryResult result)
        {
            result = new TrackerDetailedQueryResult();

            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var nextTicks = tickDict.Keys.Where(t => t > query.MinTick && t <= query.MaxTick).OrderBy(t => t); // Ensure ticks are in ascending order and within bounds
                foreach (var nextTick in nextTicks)
                {
                    if (tickDict.TryGetValue(nextTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, query.Filter))  // Filter on tags
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            result.Values = matchingVersions.Select(kv => new KeyValuePair<TrackerKey, object>(
                                new TrackerKey { PropertyName = query.PropertyName, Tick = nextTick, Version = kv.Key, Tags = kv.Value.Tags },
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

        public bool TryGetDetailedValuesAtPreviousAvailableTick(TrackerRangeQuery query, out TrackerDetailedQueryResult result)
        {
            result = new TrackerDetailedQueryResult();

            if (data.TryGetValue(query.PropertyName, out var tickDict))
            {
                var previousTicks = tickDict.Keys.Where(t => t < query.MaxTick && t >= query.MinTick).OrderByDescending(t => t); // Ensure ticks are in descending order and within bounds
                foreach (var previousTick in previousTicks)
                {
                    if (tickDict.TryGetValue(previousTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, query.Filter))  // Filter on tags
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            result.Values = matchingVersions.Select(kv => new KeyValuePair<TrackerKey, object>(
                                new TrackerKey { PropertyName = query.PropertyName, Tick = previousTick, Version = kv.Key, Tags = kv.Value.Tags },
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
