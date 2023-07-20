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

        private static bool MatchTags(TaggedData data, string[] tags)
        {
            if (tags == null || !tags.Any())
            {
                return true;
            }

            if (data.Tags == null)
            {
                return false;
            }

            return tags.All(tag => data.Tags.Contains(tag));
        }

        public IEnumerable<string> DistinctKeys 
            => data.Keys;

        public bool Exists(string propertyName, int minTick = int.MinValue, int maxTick = int.MaxValue, params string[] tags)
        {
            // Check if the property exists
            if (data.TryGetValue(propertyName, out var tickDict))
            {
                // Check if any of the ticks for this property falls within the given range
                foreach (var tickKv in tickDict)
                {
                    if (tickKv.Key >= minTick && tickKv.Key <= maxTick)
                    {
                        // Check for any matching tags
                        var matchingVersions = tickKv.Value
                            .Where(versionKv => MatchTags(versionKv.Value, tags))
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

        public bool TryGetLatestValue(string propertyName, int tick, out KeyValuePair<TrackerKey, object> value, params string[] tags)
        {
            if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
            {
                var matchingVersions = versionDict
                    .Where(kv => MatchTags(kv.Value, tags))  // Filter on tags
                    .OrderByDescending(kv => kv.Key);  // Highest version first

                // If any values with matching tags are found, return the one with the highest version
                if (matchingVersions.Any())
                {
                    var highestVersion = matchingVersions.First();
                    value = new KeyValuePair<TrackerKey, object>(new TrackerKey
                    {
                        PropertyName = propertyName,
                        Tick = tick,
                        Version = highestVersion.Key,
                        Tags = highestVersion.Value.Tags
                    }, highestVersion.Value.Data);
                    return true;
                }
            }

            // If no values with matching tags are found, return false
            value = default;
            return false;
        }

        public bool TryGetLatestValueAtNextAvailableTick(string propertyName, int tick, out KeyValuePair<TrackerKey, object> value, int stopIfTick = int.MaxValue, params string[] tags)
        {
            if (data.TryGetValue(propertyName, out var tickDict))
            {
                var nextTicks = tickDict.Keys.Where(t => t > tick && t <= stopIfTick).OrderBy(t => t); // Ensure ticks are in ascending order and within bounds
                foreach (var nextTick in nextTicks)
                {
                    if (tickDict.TryGetValue(nextTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, tags))
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            var highestVersion = matchingVersions.First();
                            value = new KeyValuePair<TrackerKey, object>(new TrackerKey
                            {
                                PropertyName = propertyName,
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
            value = default;
            return false;
        }

        public bool TryGetLatestValueAtPreviousAvailableTick(string propertyName, int tick, out KeyValuePair<TrackerKey, object> value, int stopIfTick = int.MinValue, params string[] tags)
        {
            if (data.TryGetValue(propertyName, out var tickDict))
            {
                var previousTicks = tickDict.Keys.Where(t => t < tick && t >= stopIfTick).OrderByDescending(t => t); // Ensure ticks are in descending order and within bounds
                foreach (var previousTick in previousTicks)
                {
                    if (tickDict.TryGetValue(previousTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, tags))
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            var highestVersion = matchingVersions.First();
                            value = new KeyValuePair<TrackerKey, object>(new TrackerKey
                            {
                                PropertyName = propertyName,
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
            value = default;
            return false;
        }

        #endregion

        #region Detailed

        public bool TryGetDetailedValue(string propertyName, int tick, out IEnumerable<KeyValuePair<TrackerKey, object>> values, params string[] tags)
        {
            if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
            {
                var matchingVersions = versionDict
                    .Where(kv => MatchTags(kv.Value, tags))  // Filter on tags
                    .OrderByDescending(kv => kv.Key);  // Highest version first

                // If any values with matching tags are found, return them
                if (matchingVersions.Any())
                {
                    values = matchingVersions.Select(kv => new KeyValuePair<TrackerKey, object>(
                        new TrackerKey { PropertyName = propertyName, Tick = tick, Version = kv.Key, Tags = kv.Value.Tags },
                        kv.Value.Data
                    ));
                    return true;
                }
            }

            // If no values with matching tags are found, return false
            values = null;
            return false;
        }

        public bool TryGetDetailedValuesAtNextAvailableTick(string propertyName, int tick, out IEnumerable<KeyValuePair<TrackerKey, object>> values, int stopIfTick = int.MaxValue, params string[] tags)
        {
            if (data.TryGetValue(propertyName, out var tickDict))
            {
                var nextTicks = tickDict.Keys.Where(t => t > tick && t <= stopIfTick).OrderBy(t => t); // Ensure ticks are in ascending order and within bounds
                foreach (var nextTick in nextTicks)
                {
                    if (tickDict.TryGetValue(nextTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, tags))  // Filter on tags
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            values = matchingVersions.Select(kv => new KeyValuePair<TrackerKey, object>(
                                new TrackerKey { PropertyName = propertyName, Tick = nextTick, Version = kv.Key, Tags = kv.Value.Tags },
                                kv.Value.Data
                            ));
                            return true;
                        }
                    }
                }
            }

            // If no values with matching tags are found after examining all ticks, return false
            values = null;
            return false;
        }

        public bool TryGetDetailedValuesAtPreviousAvailableTick(string propertyName, int tick, out IEnumerable<KeyValuePair<TrackerKey, object>> values, int stopIfTick = int.MinValue, params string[] tags)
        {
            if (data.TryGetValue(propertyName, out var tickDict))
            {
                var previousTicks = tickDict.Keys.Where(t => t < tick && t >= stopIfTick).OrderByDescending(t => t); // Ensure ticks are in descending order and within bounds
                foreach (var previousTick in previousTicks)
                {
                    if (tickDict.TryGetValue(previousTick, out var versionDict))
                    {
                        var matchingVersions = versionDict
                            .Where(kv => MatchTags(kv.Value, tags))  // Filter on tags
                            .OrderByDescending(kv => kv.Key);  // Highest version first

                        // If any values with matching tags are found, return them
                        if (matchingVersions.Any())
                        {
                            values = matchingVersions.Select(kv => new KeyValuePair<TrackerKey, object>(
                                new TrackerKey { PropertyName = propertyName, Tick = previousTick, Version = kv.Key, Tags = kv.Value.Tags },
                                kv.Value.Data
                            ));
                            return true;
                        }
                    }
                }
            }

            // If no values with matching tags are found after examining all ticks, return false
            values = null;
            return false;
        }

        #endregion


    }


}
