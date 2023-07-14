using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public partial class ScopedTracker : IDisposable
    {
        private ITrackerDataReadOnly Data { get; set; }
        private ScopeSettings Settings { get; set; }

        internal ScopedTracker(TrackerData data, ScopeSettings settings)
        {
            Data = data;
            Settings = settings;
        }

        public T Get<T>(string propertyName, int tick)
        {

            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == tick) // Get in tick position.
                .Select(pair => pair.Value);

            if (!result.Any())
            {
                Log.Error($"No valid found for {propertyName}, {tick}");
                return default;
            }

            return (T)result.First();
        }

        public T GetOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == tick)
                .Select(pair => pair.Value)
                .DefaultIfEmpty(defaultValue)
                .First();

            return (T)result;
        }

        public T GetOrPrevious<T>(string propertyName, int tick)
        {
            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick)
                .OrderByDescending(pair => pair.Key.Tick)
                .Select(pair => pair.Value)
                .DefaultIfEmpty(default(T))
                .First();

            return (T)result;
        }

        public T GetOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick)
                .OrderByDescending(pair => pair.Key.Tick)
                .Select(pair => pair.Value)
                .DefaultIfEmpty(defaultValue)
                .First();

            return (T)result;
        }





        public IEnumerable<T> GetDetailed<T>(string propertyName, int tick)
        {
            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == tick)
                .Select(pair => (T)pair.Value);

            return result.Any() ? result : throw new Exception("No values found at the given tick");
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == tick)
                .Select(pair => (T)pair.Value);

            return result.Any() ? result : Enumerable.Repeat(defaultValue, 1);
        }

        public IEnumerable<T> GetDetailedOrPrevious<T>(string propertyName, int tick)
        {
            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick)
                .OrderByDescending(pair => pair.Key.Tick)
                .Select(pair => (T)pair.Value);

            return result.Any() ? result : throw new Exception("No values found at or before the given tick");
        }

        public IEnumerable<T> GetDetailedOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var result = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick)
                .OrderByDescending(pair => pair.Key.Tick)
                .Select(pair => (T)pair.Value);

            return result.Any() ? result : Enumerable.Repeat(defaultValue, 1);
        }



        public void Dispose()
        {
            Data = null;
            Settings = null;
        }
    }



}
