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
            var detailed = GetDetailed<T>(propertyName, tick);
            return detailed.Any() ? detailed.Last() : default;
        }

        public T GetOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var detailed = GetDetailed<T>(propertyName, tick);
            return detailed.Any() ? detailed.Last() : defaultValue;
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

        public T GetOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
        {

            // ...
            return default;
        }





        public IEnumerable<T> GetDetailed<T>(string propertyName, int tick)
        {
            // ... 
            return default;
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var detailed = GetDetailed<T>(propertyName, tick);
            return detailed.Any() ? detailed : new List<T> { defaultValue };
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

        public IEnumerable<T> GetDetailedOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            return default;
        }



        public void Dispose()
        {
            Data = null;
            Settings = null;
        }
    }



}
