using System.Text.Json.Serialization;
using Tracking;

namespace TrackingKit_Core
{
    [JsonConverter(typeof(JsonConverterTrackerStorage<SQLTrackerStorage>))]
    public partial class SQLTrackerStorage : ITrackerStorage
    {
        protected ISQLTrackerStorage SQL { get; }

        public IEnumerable<string> Properties => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public SQLTrackerStorage()
        {
            SQL = InstanceFactory.GetOrCreateDatabase().Invoke("idk");
        }

        public bool TryGetTicks(string propertyName, out IEnumerable<int> ticks)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue<T>(string propertyName, int tick, int version, out TaggedData<T> value) where T : notnull
        {
            throw new NotImplementedException();
        }

        public bool TryGetVersions(string propertyName, int tick, out IEnumerable<int> versions)
        {
            throw new NotImplementedException();
        }

        public bool TryGetLatestVersion(string propertyName, int tick, out int latestVersion)
        {
            throw new NotImplementedException();
        }

        public bool AddValue<T>(string propertyName, int tick, T value) where T : notnull
        {
            throw new NotImplementedException();
        }

        public bool AddValue<T>(string propertyName, int tick, int version, T value) where T : notnull
        {
            throw new NotImplementedException();
        }

        public bool AddValueWithTags<T>(string propertyName, int tick, T value, IReadOnlyCollection<string> tags) where T : notnull
        {
            throw new NotImplementedException();
        }

        public bool AddValueWithTags<T>(string propertyName, int tick, int version, T value, IReadOnlyCollection<string> tags) where T : notnull
        {
            throw new NotImplementedException();
        }

        public bool UpdateValue<T>(string propertyName, int tick, int version, T value) where T : notnull
        {
            throw new NotImplementedException();
        }

        public bool UpdateTags(string propertyName, int tick, int version, IReadOnlyCollection<string> tags)
        {
            throw new NotImplementedException();
        }

        public bool RemoveValue(string propertyName, int tick, int version)
        {
            throw new NotImplementedException();
        }

        public bool RemoveValuesForTick(string propertyName, int tick)
        {
            throw new NotImplementedException();
        }
    }
}
