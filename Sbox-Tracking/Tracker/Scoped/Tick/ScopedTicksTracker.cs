using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public partial class ScopedTicksTracker : IDisposable
    {
        private TrackerData Data { get; }
        private ScopedTickSettings ScopedSettings { get; }

        internal ScopedTicksTracker(TrackerData data, ScopedTickSettings scopedSettings)
        {
            Data = data;
            ScopedSettings = scopedSettings;
        }

        #region Count, DistinctKeys and Exists methods
        public int Count() 
            => Data.QueryCount(tickRange: (ScopedSettings.MinTick, ScopedSettings.MaxTick));

        public IEnumerable<string> GetDistinctKeys()
        {
            return Data
                .Query(tickRange: (ScopedSettings.MinTick, ScopedSettings.MaxTick))
                .Select(item => item.Key.PropertyName)
                .Distinct();
        }


        public bool Exists(string propertyName)
        {
            var query = Data.Query(propertyName, (ScopedSettings.MinTick, ScopedSettings.MaxTick), ScopedSettings.Tags);
            return query.Any();
        }

        public bool ExistsAtOrBefore(string propertyName, int tick)
        {
            var query = Data.Query(propertyName, (ScopedSettings.MinTick, tick), ScopedSettings.Tags);
            return query.Any();
        }

        public bool ExistsInRange(string propertyName, int minTick, int maxTick)
        {
            var query = Data.Query(propertyName, (minTick, maxTick), ScopedSettings.Tags);
            return query.Any();
        }
        #endregion

        #region Get methods
        public T Get<T>(string propertyName, int tick)
        {
            var query = Data.Query(propertyName, (tick, tick), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error($"No valid found for {propertyName}, {tick}");
                return default;
            }

            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();
            return (T)itemToSelect.Value;
        }

        public T GetOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var query = Data.Query(propertyName, (tick, tick), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();
            return (T)itemToSelect.Value;
        }
        #endregion


        #region Previous methods
        public T GetOrPrevious<T>(string propertyName, int tick)
        {
            var query = Data.Query(propertyName, (ScopedSettings.MinTick, tick), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error("No values found of that type");
                return default;
            }

            query = query.OrderByDescending(pair => pair.Key.Tick)
                .ThenByDescending(pair => pair.Key.Version);

            var itemToSelect = query.First();
            return (T)itemToSelect.Value;
        }

        public T GetOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var query = Data.Query(propertyName, (ScopedSettings.MinTick, tick), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderByDescending(pair => pair.Key.Tick)
                .ThenByDescending(pair => pair.Key.Version);

            var itemToSelect = query.First();
            return (T)itemToSelect.Value;
        }
        #endregion

        #region Detailed previous methods
        public IEnumerable<T> GetDetailed<T>(string propertyName, int tick)
        {
            var query = Data.Query(propertyName, (tick, tick), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error("Failed to find any values");
                return default;
            }

            query = query.OrderByDescending(x => x.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
        {
            var query = Data.Query(propertyName, (tick, tick), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderByDescending(x => x.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public IEnumerable<T> GetDetailedOrPrevious<T>(string propertyName, int tick)
        {
            var query = Data.Query(propertyName, (ScopedSettings.MinTick, tick), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error("No values found");
                return default;
            }

            query = query.OrderByDescending(pair => pair.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public IEnumerable<T> GetDetailedOrPreviousOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
        {
            var query = Data.Query(propertyName, (ScopedSettings.MinTick, tick), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderByDescending(pair => pair.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }
        #endregion


        #region Next methods
        public T GetOrNext<T>(string propertyName, int tick)
        {
            var query = Data.Query(propertyName, (tick, ScopedSettings.MaxTick), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error("No values found of that type");
                return default;
            }

            query = query.OrderBy(pair => pair.Key.Tick)
                .ThenBy(pair => pair.Key.Version);

            var itemToSelect = query.First();
            return (T)itemToSelect.Value;
        }

        public T GetOrNextOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var query = Data.Query(propertyName, (tick, ScopedSettings.MaxTick), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderBy(pair => pair.Key.Tick)
                .ThenBy(pair => pair.Key.Version);

            var itemToSelect = query.First();
            return (T)itemToSelect.Value;
        }
        #endregion

        #region Detailed Next methods
        public IEnumerable<T> GetDetailedOrNext<T>(string propertyName, int tick)
        {
            var query = Data.Query(propertyName, (tick, ScopedSettings.MaxTick), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error("No values found");
                return default;
            }

            query = query.OrderBy(pair => pair.Key.Tick)
                         .ThenBy(pair => pair.Key.Version);

            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public IEnumerable<T> GetDetailedOrNextOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
        {
            var query = Data.Query(propertyName, (tick, ScopedSettings.MaxTick), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderBy(pair => pair.Key.Tick)
                         .ThenBy(pair => pair.Key.Version);

            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }
        #endregion

        public void Dispose()
        {

        }
    }
}
