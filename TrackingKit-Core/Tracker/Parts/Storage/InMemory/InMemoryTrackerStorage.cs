using System;
using System.Collections.Generic;
using System.Linq;
using CompositeDictionary;
using Newtonsoft.Json;
using TrackingKit_Core;

namespace Tracking
{
    // TOOD : FIx
    //[JsonConverter(typeof(JsonConverterTrackerStorage<InMemoryTrackerStorage<>>))]
    internal sealed class InMemoryTrackerStorage<TKey> : ICloneable, ITrackerStorage<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ICompositeDictionary<TKey, int, int, TaggedData<object>> _data;
        private readonly ICompositeDictionary<TKey, int, int> _latestVersions;

        // TODO: Fix this. We need to make like a free _count mechanism like .NET.
        public int Count => _data.Count;

        public IEnumerable<TKey> Properties => _data.GetPrimaryKeys();

        public InMemoryTrackerStorage() : this(true) { }

        public InMemoryTrackerStorage(bool isConcurrent)
        {
            if (isConcurrent)
            {
                _data = new ConcurrentCompositeDictionary<TKey, int, int, TaggedData<object>>();
                _latestVersions = new ConcurrentCompositeDictionary<TKey, int, int>();
            }
            else
            {
                _data = new CompositeDictionary<TKey, int, int, TaggedData<object>>();
                _latestVersions = new CompositeDictionary<TKey, int, int>();
            }
        }

        public bool TryGetTicks(TKey propertyName, out IEnumerable<int> ticks)
        {
            if(_data.ContainsPrimary(propertyName))
            {
                ticks = new List<int>( _data.GetSecondaryKeys(propertyName) );
                return true;
            }

            ticks = new List<int>();
            return false;
        }

        public bool TryGetValue<T>(TKey propertyName, int tick, int version, out TaggedData<T> value)
            where T : notnull
        {
            if (_data.ContainsThirdKey(propertyName, tick, version))
            {
                value = _data[propertyName, tick, version].CastObject<T>();
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetVersions(TKey propertyName, int tick, out IEnumerable<int> versions)
        {
            versions = _data.GetThirdKeys(propertyName, tick).OrderBy(v => v);
            return versions.Any();
        }

        public bool TryGetLatestVersion(TKey propertyName, int tick, out int latestVersion)
        {
            return _latestVersions.TryGetValue(propertyName, tick, out latestVersion);
        }

        public bool AddValue<T>(TKey propertyName, int tick, T value)
            where T : notnull
        {
            int version = ComputeNextVersion(propertyName, tick);
            _data[propertyName, tick, version] = new TaggedData<object>(value);
            UpdateLatestVersion(propertyName, tick, version);
            return true; // Assuming always successful, adjust if needed
        }

        public bool AddValueWithTags<T>(TKey propertyName, int tick, T value, IReadOnlyCollection<string> tags)
            where T : notnull
        {
            int version = ComputeNextVersion(propertyName, tick);
            _data[propertyName, tick, version] = new TaggedData<object>(value, tags);
            UpdateLatestVersion(propertyName, tick, version);
            return true; // Assuming always successful, adjust if needed
        }

        public bool UpdateValue<T>(TKey propertyName, int tick, int version, T value)
            where T : notnull
        {
            if (!_data.ContainsThirdKey(propertyName, tick, version))
                return false;

            var existingTags = _data[propertyName, tick, version].Tags;
            _data[propertyName, tick, version] = new TaggedData<object>(value, existingTags.ToList());
            return true;
        }

        public bool UpdateTags(TKey propertyName, int tick, int version, IReadOnlyCollection<string> tags)
        {
            if (!_data.ContainsThirdKey(propertyName, tick, version))
                return false;

            var existingValue = _data[propertyName, tick, version];
            _data[propertyName, tick, version] = new TaggedData<object>(existingValue.Object, tags);
            return true;
        }

        public bool RemoveValue(TKey propertyName, int tick, int version)
        {
            return _data.RemoveThirdKey(propertyName, tick, version);
        }

        public bool RemoveValuesForTick(TKey propertyName, int tick)
        {
            var versions = _data.GetThirdKeys(propertyName, tick).ToList();
            foreach (var version in versions)
            {
                _data.RemoveThirdKey(propertyName, tick, version);
            }
            return versions.Count > 0;
        }

        private int ComputeNextVersion(TKey propertyName, int tick)
        {
            if (TryGetLatestVersion(propertyName, tick, out var latestVersion))
                return latestVersion + 1;
            return 1;
        }

        private void UpdateLatestVersion(TKey propertyName, int tick, int version)
        {
            if (!_latestVersions.ContainsSecondary(propertyName, tick) || _latestVersions[propertyName, tick] < version)
            {
                _latestVersions[propertyName, tick] = version;
            }
        }

        public object Clone()
        {
            var cloned = new InMemoryTrackerStorage<TKey>();

            foreach (var propertyName in _data.GetPrimaryKeys())
            {
                foreach (var tick in _data.GetSecondaryKeys(propertyName))
                {
                    foreach (var version in _data.GetThirdKeys(propertyName, tick))
                    {
                        cloned._data[propertyName, tick, version] = _data[propertyName, tick, version];
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

        public bool AddValue<T>(TKey propertyName, int tick, int version, T value) where T : notnull
        {
            throw new NotImplementedException();
        }

        public bool AddValueWithTags<T>(TKey propertyName, int tick, int version, T value, IReadOnlyCollection<string> tags) where T : notnull
        {
            throw new NotImplementedException();
        }
    }
}
