using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    public partial class ScopedTickTracker : IDisposable
    {
        private TrackerData Data { get; }

        private ScopeSettings Settings { get; }

        internal ScopedTickTracker(TrackerData data, ScopeSettings settings)
        {
            Data = data;
            Settings = settings;
        }

        // in scope

        public IEnumerable<T> GetDetailed<T>(string propertyName)
        {
            // ... function to get all values
            return default;
        }

        public T Get<T>(string propertyName)
        {
            // ... function to get the value
            return default;
        }

        public T GetOrPrevious<T>(string propertyName)
        {
            // ... function to get the value or the previous one
            return default;
        }

        public IEnumerable<T> GetDetailedOrPrevious<T>(string propertyName)
        {
            // ... function to get all values or the previous ones
            return default;
        }

        public T GetOrDefault<T>(string propertyName, T defaultValue)
        {
            // ... function to get the value or return default if none exists
            return default;
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName)
        {
            // ... function to get all values or return default if none exists
            return default;
        }

        public T GetOrLastOrDefault<T>(string propertyName, T defaultValue)
        {
            var detailed = GetDetailed<T>(propertyName);
            if (detailed.Any())
            {
                // The value exists at the current tick, so return it.
                return Get<T>(propertyName);
            }
            else
            {
                // Try to get the last value set before this tick.
                var last = GetOrPrevious<T>(propertyName);
                if (last != null)
                {
                    // A previous value exists, so return it.
                    return last;
                }
                else
                {
                    // No previous value exists, so return default.
                    return default;
                }
            }
        }

        public T GetDetailedOrLastOrDefault<T>(string propertyName, T defaultValue)
        {
            return default;
        }

    }



}
