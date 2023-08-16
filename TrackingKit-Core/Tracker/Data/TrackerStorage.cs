using NestedDictionary2;

namespace Tracking
{
    public class TaggedData
    {
        public object Object { get; }

        public IReadOnlyCollection<string> Tags { get; }

        public TaggedData(object @object)
        {
            Object = @object;
            Tags = new HashSet<string>();
        }

        public TaggedData(object @object, IReadOnlyCollection<string> tags)
        {
            Object = @object;
            Tags = new HashSet<string>(tags);
        }
    }


    internal sealed class TrackerStorage : ICloneable
    {
        public int Count => _data.Count;

        private readonly ConcurrentNestedDictionary<string, int, int, TaggedData> _data = new();
        private readonly ConcurrentNestedDictionary<string, int, int> _latestVersions = new();

        public bool TryGetValue(string propertyName, int tick, int version, out TaggedData value)
        {
            if (_data.ContainsPrimary(propertyName) && _data.ContainsSecondary(propertyName, tick) && _data.ContainsThirdKey(propertyName, tick, version))
            {
                value = _data[propertyName, tick, version];
                return true;
            }
            value = null;
            return false;
        }


        public TaggedData this[string propertyName, int tick, int version]
        {
            get => _data[propertyName, tick, version];
            set => _data[propertyName, tick, version] = value;
        }


        public bool TryGetLatestVersion(string propertyName, int tick, out int outputVersion)
        {
            return _latestVersions.TryGetValue(propertyName, tick, out outputVersion);
        }

        public bool TryGetVersions(string propertyName, int tick, out IEnumerable<int> outputVersion)
        {
            outputVersion = _data.GetThirdKeys(propertyName, tick).OrderBy(v => v);
            return outputVersion.Any();
        }

        public void SetValue<T>(string propertyName, int tick, int version, T value)
            where T : notnull
        {
            _data[propertyName, tick, version] = new TaggedData(value);
            UpdateLatestVersion(propertyName, tick, version);
        }

        public void SetValue<T>(string propertyName, int tick, int version, T value, IReadOnlyCollection<string> tags)
            where T : notnull
        {
            _data[propertyName, tick, version] = new TaggedData(value, tags);
            UpdateLatestVersion(propertyName, tick, version);
        }

        private void UpdateLatestVersion(string propertyName, int tick, int version)
        {
            if (!_latestVersions.ContainsPrimary(propertyName) || !_latestVersions.ContainsSecondary(propertyName, tick) || _latestVersions[propertyName, tick] < version)
            {
                _latestVersions[propertyName, tick] = version;
            }
        }

        public bool RemoveValues(string propertyName, int tick)
        {
            var versions = _data.GetThirdKeys(propertyName, tick).ToList();
            foreach (var version in versions)
            {
                _data.RemoveThirdKey(propertyName, tick, version);
            }

            if (_latestVersions.ContainsPrimary(propertyName) && _latestVersions.ContainsSecondary(propertyName, tick))
            {
                var maxVersion = versions.Any() ? versions.Max() : -1;
                _latestVersions[propertyName, tick] = maxVersion;
            }

            return versions.Count > 0;
        }

        public bool RemoveSpecificValue(string propertyName, int tick, int version)
        {
            var removed = _data.RemoveThirdKey(propertyName, tick, version);
            if (removed && _latestVersions.ContainsPrimary(propertyName) && _latestVersions.ContainsSecondary(propertyName, tick) && _latestVersions[propertyName, tick] == version)
            {
                var versions = _data.GetThirdKeys(propertyName, tick).ToList();
                var maxVersion = versions.Any() ? versions.Max() : -1;
                _latestVersions[propertyName, tick] = maxVersion;
            }

            return removed;
        }

        public IEnumerable<string> GetPropertyNamesKeys()
        {
            return _data.GetPrimaryKeys();
        }

        public IEnumerable<int> GetTickKeys(string propertyName)
        {
            return _data.GetSecondaryKeys(propertyName);
        }

        public IEnumerable<int> GetVersionKeys(string propertyName, int tick)
        {
            return _data.GetThirdKeys(propertyName, tick);
        }

        public bool ContainsProperty(string propertyName)
        {
            return _data.ContainsPrimary(propertyName);
        }

        public bool ContainsTick(string propertyName, int tick)
        {
            return _data.ContainsSecondary(propertyName, tick);
        }

        public bool ContainsVersion(string propertyName, int tick, int version)
        {
            return _data.ContainsThirdKey(propertyName, tick, version);
        }

        public object Clone()
        {
            var cloned = new TrackerStorage();

            foreach (var propertyName in _data.GetPrimaryKeys())
            {
                foreach (var tick in _data.GetSecondaryKeys(propertyName))
                {
                    foreach (var version in _data.GetThirdKeys(propertyName, tick))
                    {
                        cloned.SetValue(propertyName, tick, version, _data[propertyName, tick, version].Object, _data[propertyName, tick, version].Tags.ToList());
                    }
                }
            }

            foreach (var propertyName in _latestVersions.GetPrimaryKeys())
            {
                foreach (var tick in _latestVersions.GetSecondaryKeys(propertyName))
                {
                    cloned._latestVersions[propertyName, tick] = _latestVersions[propertyName, tick];
                }
            }

            return cloned;
        }
    }




}
