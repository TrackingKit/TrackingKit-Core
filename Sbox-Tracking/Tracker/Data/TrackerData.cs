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

        public IEnumerable<(TrackerKey Key, object Value)> Query(
            string propertyName = null,
            (int minTick, int maxTick)? tickRange = null,
            params string[] tags)
        {
            var dataInRange = tickRange.HasValue
                ? indexedData.GetValuesBetweenTicks(tickRange.Value.minTick, tickRange.Value.maxTick)
                : indexedData.GetValuesBetweenTicks(int.MinValue, int.MaxValue);

            if (!string.IsNullOrEmpty(propertyName))
            {
                dataInRange = dataInRange.Where(data => data.PropertyName == propertyName);
            }

            if (tags != null && tags.Length > 0)
            {
                dataInRange = dataInRange.Where(data => tags.All(tag => data.TaggedData.Tags.Contains(tag)));
            }

            return dataInRange
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

        public int QueryCount(
            string propertyName = null,
            (int minTick, int maxTick)? tickRange = null,
            params string[] tags)
        {
            int count = 0;
            foreach (var item in indexedData.GetValuesBetweenTicks(tickRange?.minTick ?? int.MinValue, tickRange?.maxTick ?? int.MaxValue))
            {
                if (propertyName != null && item.PropertyName != propertyName)
                {
                    continue;
                }

                if (tags.Length > 0 && !tags.All(tag => item.TaggedData.Tags.Contains(tag)))
                {
                    continue;
                }

                count++;
            }

            return count;
        }


    }


}
