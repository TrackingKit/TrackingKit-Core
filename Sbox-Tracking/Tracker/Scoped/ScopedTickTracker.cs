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
        // Ref
        private TrackerData Data { get; set; }

        private ScopeSettings Settings { get; set; }

        internal ScopedTickTracker(TrackerData data, ScopeSettings settings)
        {
            Data = data;
            Settings = settings;
        }

        public T Get<T>(string propertyName)
        {
            // ... function to get the value
            return default;
        }

        // TODO: Should Get have a option for version? and so should ScopeTracker etc.!!!
        // public T Get<T>(string propertyName, int version); 

        public T GetOrDefault<T>(string propertyName, T defaultValue)
        {
            // ... function to get the value or return default if none exists
            return default;
        }



        public IEnumerable<T> GetDetailed<T>(string propertyName)
        {
            // ... function to get all values
            return default;
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName)
        {
            // ... function to get all values or return default if none exists
            return default;
        }




        public void Dispose()
        {
            Data = null;
            Settings = null;
        }
    }



}
