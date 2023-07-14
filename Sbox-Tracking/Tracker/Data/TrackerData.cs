using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public class TrackerData : ITrackerDataReadOnly
    {
        private readonly SortedDictionary<TrackerKey, object> Values = new SortedDictionary<TrackerKey, object>();

        private readonly SortedDictionary<(string PropertyName, int Tick), int> LatestVersions = new SortedDictionary<(string, int), int>();

        public int Count => Values.Count;

        public bool GetKeyExists(string propertyName)
            => Values.Keys.Where(x => x.PropertyName == propertyName ).Any();

        public IEnumerable<string> Keys
                => LatestVersions.Keys.Select(k => k.PropertyName).Distinct();


        private int minTick = int.MaxValue;
        private int maxTick = int.MinValue;

        public (int minTick, int maxTick) RecordedRange => (minTick, maxTick);



        public int GetLatestVersion(string propertyName, int tick)
        {
            var key = (PropertyName: propertyName, Tick: tick);
            LatestVersions.TryGetValue(key, out int latestRecordedVersion);
            return latestRecordedVersion;
        }


        public void AddValue(TrackerKey key, object value)
        {
            Values[key] = value;

            var keyLatestVersion = (PropertyName: key.PropertyName, Tick: key.Tick);
            LatestVersions[keyLatestVersion] = key.Version;

            // Update the min and max ticks
            minTick = Math.Min(minTick, key.Tick);
            maxTick = Math.Max(maxTick, key.Tick);

        }

        public void RemoveValue(TrackerKey key)
        {
            Values.Remove(key);

            // Remove version associated with this key
            var versionKey = (PropertyName: key.PropertyName, Tick: key.Tick);

            if (LatestVersions.ContainsKey(versionKey) && LatestVersions[versionKey] == key.Version)
            {
                LatestVersions.Remove(versionKey);
            }

            // If we removed the last value with the min or max tick, compute them again
            if (!LatestVersions.Keys.Any(k => k.Tick == minTick) || !LatestVersions.Keys.Any(k => k.Tick == maxTick))
            {
                minTick = LatestVersions.Count > 0 ? LatestVersions.Keys.Min(k => k.Tick) : int.MaxValue;
                maxTick = LatestVersions.Count > 0 ? LatestVersions.Keys.Max(k => k.Tick) : int.MinValue;
            }
        }

        public IEnumerable<KeyValuePair<TrackerKey, object>> Get(string propertyName, ScopeSettings settings)
        {
            return GetValues()
                .Where(pair => 
                    pair.Key.PropertyName == propertyName
                    && pair.Key.Tick >= settings.MinTick
                    && pair.Key.Tick <= settings.MaxTick
                    && (settings.Tags == null || settings.Tags.All(tag => pair.Key.Tags.Contains(tag)))
                    );
        }

        public IEnumerable<KeyValuePair<TrackerKey, object>> GetValues() => Values;
    }

}