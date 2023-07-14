using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public partial class ScopedTracker
    {
        private TrackerData Data { get; }
        private ScopeSettings Settings { get; }

        internal ScopedTracker(TrackerData data, ScopeSettings settings)
        {
            Data = data;
            Settings = settings;
        }

        public IEnumerable<T> GetDetailed<T>(string propertyName, int tick)
        {
            // ... 
            return default;
        }

        public T Get<T>(string propertyName, int tick)
        {
            var detailed = GetDetailed<T>(propertyName, tick);
            return detailed.Any() ? detailed.Last() : default;
        }

        public T GetOrPrevious<T>(string propertyName, int tick)
        {
            var detailed = GetDetailed<T>(propertyName, tick);
            if (detailed.Any())
            {
                // The value exists at the current tick, so return it.
                return detailed.Last();
            }
            else
            {
                // Try to get the last value set before this tick.
                var last = GetDetailed<T>(propertyName, tick - 1);
                return last.Any() ? last.Last() : default;
            }
        }

        public IEnumerable<T> GetDetailedOrPrevious<T>(string propertyName, int tick)
        {
            var detailed = GetDetailed<T>(propertyName, tick);
            if (detailed.Any())
            {
                // The values exist at the current tick, so return them.
                return detailed;
            }
            else
            {
                // Try to get the last values set before this tick.
                return GetDetailed<T>(propertyName, tick - 1);
            }
        }

        public T GetOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var detailed = GetDetailed<T>(propertyName, tick);
            return detailed.Any() ? detailed.Last() : defaultValue;
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var detailed = GetDetailed<T>(propertyName, tick);
            return detailed.Any() ? detailed : new List<T> { defaultValue };
        }

        public T GetOrLastOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            // ...
            return default;
        }
    }



}
