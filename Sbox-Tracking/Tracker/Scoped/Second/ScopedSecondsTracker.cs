using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public partial class ScopedSecondsTracker : IDisposable
    {
        private TrackerData Data { get; }
        private ScopedSecondSettings ScopedSettings { get; }

        internal ScopedSecondsTracker(TrackerData data, ScopedSecondSettings scopedSettings)
        {
            Data = data;
            ScopedSettings = scopedSettings;
        }

        public int Count()
            => Data.QueryCount(tickRange: (TimeUtility.SecondToTick(ScopedSettings.MinSecond), TimeUtility.SecondToTick(ScopedSettings.MaxSecond)));

        public bool Exists(string propertyName)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(ScopedSettings.MinSecond), TimeUtility.SecondToTick(ScopedSettings.MaxSecond)), ScopedSettings.Tags);
            return query.Any();
        }

        public bool ExistsAtOrBefore(string propertyName, float second)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(ScopedSettings.MinSecond), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            return query.Any();
        }

        public bool ExistsInRange(string propertyName, float minSecond, float maxSecond)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(minSecond), TimeUtility.SecondToTick(maxSecond)), ScopedSettings.Tags);
            return query.Any();
        }

        public T Get<T>(string propertyName, float second)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error($"No valid found for {propertyName}, {second}");
                return default;
            }

            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();
            return (T)itemToSelect.Value;
        }

        public T GetOrDefault<T>(string propertyName, float second, T defaultValue)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();
            return (T)itemToSelect.Value;
        }

        public T GetOrPrevious<T>(string propertyName, float second)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(ScopedSettings.MinSecond), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
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

        public T GetOrPreviousOrDefault<T>(string propertyName, float second, T defaultValue)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(ScopedSettings.MinSecond), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderByDescending(pair => pair.Key.Tick)
                .ThenByDescending(pair => pair.Key.Version);

            var itemToSelect = query.First();
            return (T)itemToSelect.Value;
        }

        public IEnumerable<T> GetDetailed<T>(string propertyName, float second)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error("Failed to find any values");
                return default;
            }

            query = query.OrderByDescending(x => x.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, float second, IEnumerable<T> defaultValue)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderByDescending(x => x.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public IEnumerable<T> GetDetailedOrPrevious<T>(string propertyName, float second)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(ScopedSettings.MinSecond), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            if (!query.Any())
            {
                Log.Error("No values found");
                return default;
            }

            query = query.OrderByDescending(pair => pair.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public IEnumerable<T> GetDetailedOrPreviousOrDefault<T>(string propertyName, float second, IEnumerable<T> defaultValue)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(ScopedSettings.MinSecond), TimeUtility.SecondToTick(second)), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderByDescending(pair => pair.Key.Version);
            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public T GetOrNext<T>(string propertyName, float second)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(ScopedSettings.MaxSecond)), ScopedSettings.Tags);
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

        public T GetOrNextOrDefault<T>(string propertyName, float second, T defaultValue)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(ScopedSettings.MaxSecond)), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderBy(pair => pair.Key.Tick)
                .ThenBy(pair => pair.Key.Version);

            var itemToSelect = query.First();
            return (T)itemToSelect.Value;
        }

        public IEnumerable<T> GetDetailedOrNext<T>(string propertyName, float second)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(ScopedSettings.MaxSecond)), ScopedSettings.Tags);
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

        public IEnumerable<T> GetDetailedOrNextOrDefault<T>(string propertyName, float second, IEnumerable<T> defaultValue)
        {
            var query = Data.Query(propertyName, (TimeUtility.SecondToTick(second), TimeUtility.SecondToTick(ScopedSettings.MaxSecond)), ScopedSettings.Tags);
            if (!query.Any()) return defaultValue;

            query = query.OrderBy(pair => pair.Key.Tick)
                         .ThenBy(pair => pair.Key.Version);

            var itemsToSelect = query.Select(x => x.Value);
            return (IEnumerable<T>)itemsToSelect;
        }

        public void Dispose()
        {
            // Implementation of dispose if necessary
        }
    }
}
