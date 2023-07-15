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
            indexedData.Remove(propertyName, tick, version);
        }

        public IEnumerable<(TrackerKey Key, object Value)> Get(string propertyName, ScopeSettings settings)
        {
            var dataInRange = indexedData.GetValuesForPropertyBetweenTicks(propertyName, settings.MinTick, settings.MaxTick);

            return dataInRange
                .Where(data => settings.Tags == null || settings.Tags.All(tag => data.taggedData.Tags.Contains(tag)))
                .Select(data => (new TrackerKey { PropertyName = propertyName, Tick = data.tick, Version = data.version, Tags = data.taggedData.Tags }, data.taggedData.Data));
        }
    }


}
