using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public class TrackerData
    {
        private readonly TrackerIndexedData indexedData = new TrackerIndexedData();

        public int Count => indexedData.Count;

        public int GetLatestVersion(string propertyName, int tick)
            => indexedData.GetLatestVersion(propertyName, tick);

        public void AddValue(string propertyName, int tick, int version, object value, params string[] tags)
        {
            indexedData.SetValue(propertyName, tick, version, value, tags);
        }

        public void RemoveValue(string propertyName, int tick, int version)
        {
            indexedData.Remove(tick, propertyName, version);
        }

        public IEnumerable<(TrackerKey Key, object Value)> GenerateScopeTicks(int minTick, int maxTick, params string[] tags)
        {
            var dataInRange = indexedData.GetValuesBetweenTicks(minTick, maxTick);

            return dataInRange
                .Where(data => tags == null || tags.All(tag => data.TaggedData.Tags.Contains(tag)))
                .Select(data => (
                    new TrackerKey
                    {
                        PropertyName = data.PropertyName,
                        Tick = data.Tick,
                        Version = data.Version,
                        Tags = data.TaggedData.Tags
                    },
                    data.TaggedData.Data
                ));
        }

    }


}
