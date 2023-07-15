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

        public bool Exists(string propertyName)
        {
            var query = Data.Get(propertyName, Settings);

            return query.Any();
        }





        public T Get<T>(string propertyName)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == Settings.SpecificTick);

            if( !query.Any() )
            {
                Log.Error("No values found of that type");
                return default;
            }

            // Highest version.
            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();

            return (T)itemToSelect.Value;
        }

        public T GetOrDefault<T>(string propertyName, T defaultValue)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == Settings.SpecificTick);

            if (!query.Any())
            {
                return defaultValue;
            }

            // Highest version.
            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();

            return (T)itemToSelect.Value;
        }

        public IEnumerable<T> GetDetailed<T>(string propertyName)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == Settings.SpecificTick);



            return query.Select(pair => (T)pair.Value);
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, IEnumerable<T> defaultValue)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == Settings.SpecificTick);

            if (!query.Any())
            {
                return defaultValue;
            }

            return query.Select(pair => (T)pair.Value);
        }

        public void Dispose()
        {
            // TODO: data cached in this?
            Data = null;
            Settings = null;
        }



    }



}
