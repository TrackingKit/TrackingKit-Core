using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
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



    public class TrackerData
    {
        private readonly Dictionary<string, SortedDictionary<int, SortedDictionary<int, TaggedData>>> data = new();

        // TODO: Should track count inside.
        public int AllCount { get; protected set; }

        private static bool MatchTags(TaggedData data, TagFilter tagFilter)
        {
            if (tagFilter == null || !tagFilter.Tags.Any())
            {
                return true;
            }

            if (data.Tags == null)
            {
                return false;
            }

            // Match only if all tags in the data should be included according to the tagFilter
            return data.Tags.All(tag => tagFilter.ShouldInclude(tag));
        }


        public IEnumerable<string> DistinctKeys 
            => data.Keys;

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




        public void SetValue(string propertyName, int tick, int version, object value, string[] tags)
        {
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

        // TODO: Specific version, tick?

        #region Latest

        public bool TryGetLatestValue(TrackerQuery query, out TrackerQueryResult result)
        {
            result = new TrackerQueryResult();

            if (data.TryGetValue(query.PropertyName, out var tickDict) && tickDict.TryGetValue(query.Tick, out var versionDict))
            {
                var matchingVersions = versionDict
                    .Where(kv => MatchTags(kv.Value, query.Filter))  // Filter on tags
                    .OrderByDescending(kv => kv.Key);  // Highest version first

                // If any values with matching tags are found, return the one with the highest version
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
