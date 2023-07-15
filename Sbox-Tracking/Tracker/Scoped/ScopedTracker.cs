using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public partial class ScopedTracker : IDisposable
    {
        private TrackerData Data { get; set; }
        private ScopeSettings Settings { get; set; }

        internal ScopedTracker(TrackerData data, ScopeSettings settings)
        {
            Data = data;
            Settings = settings;
        }

        public bool Exists(string propertyName)
        {
            var query = Data.Get(propertyName, Settings);
            return query.Any();
        }

        public bool ExistsAtOrBefore(string propertyName, int tick)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick); // Tick or less value.
            return query.Any();
        }

        public bool ExistsInRange(string propertyName, int minTick, int maxTick)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(x => x.Key.Tick >= minTick && x.Key.Tick <= maxTick); // Within range.
            return query.Any();
        }







        public T Get<T>(string propertyName, int tick)
        {

            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == tick); // Get in tick position.

            if (!query.Any())
            {
                Log.Error($"No valid found for {propertyName}, {tick}");
                return default;
            }

            // Highest version
            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();

            return (T)itemToSelect.Value;
        }

        public T GetOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var query = Data.Get(propertyName, Settings)
               .Where(pair => pair.Key.Tick == tick); // Get in tick position.

            if (!query.Any())
            {
                return defaultValue;
            }

            // Highest version
            var itemToSelect = query.OrderByDescending(pair => pair.Key.Version).First();

            return (T)itemToSelect.Value;
        }

        public T GetOrPrevious<T>(string propertyName, int tick)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick); // Get below or equal to tick amount
            
            if (!query.Any())
            {
                Log.Error("No values found of that type");
                return default;
            }

            query = query.OrderByDescending(pair => pair.Key.Tick) // Order by descending tick rate
                .ThenByDescending(pair => pair.Key.Version); // Then by descending version

            // The first item will have the highest tick and version.
            var itemToSelect = query.First();

            return (T)itemToSelect.Value;
        }

        public T GetOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
        {
            var query = Data.Get(propertyName, Settings)
               .Where(pair => pair.Key.Tick <= tick); // Get below or equal to tick amount

            if (!query.Any())
            {
                return defaultValue;
            }

            query = query.OrderByDescending(pair => pair.Key.Tick) // Order by descending tick rate
                .ThenByDescending(pair => pair.Key.Version); // Then by descending version

            // The first item will have the highest tick and version.
            var itemToSelect = query.First();

            return (T)itemToSelect.Value;
        }




        // TODO: test.
        public IEnumerable<T> GetDetailed<T>(string propertyName, int tick)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == tick);

            if(!query.Any())
            {
                Log.Error("Failed to find any values");
                return default;
            }


            query = query.OrderByDescending(x => x.Key.Version); // Order by highest version.

            var itemsToSelect = query.Select( x => x.Value );

            return (IEnumerable<T>)itemsToSelect;
        }


        // TODO: Below is pretty much probably not done right, I need to go over this.



        // TODO: test.
        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick == tick);

            if (!query.Any())
            {
                return defaultValue;
            }

            query = query.OrderByDescending(x => x.Key.Version); // Order by highest version.


            var itemsToSelect = query.Select(x => x.Value);


            return (IEnumerable<T>)itemsToSelect;
        }

        // TODO: Test.
        public IEnumerable<T> GetDetailedOrPrevious<T>(string propertyName, int tick)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick); // Tick or less value.

            if(!query.Any())
            {
                Log.Error("No values found");
                return default;
            }

            query = query.OrderByDescending(pair => pair.Key.Version);

            var itemsToSelect = query.Select(x => x.Value);


            return (IEnumerable<T>)itemsToSelect;
        }

        // TODO: Test.
        public IEnumerable<T> GetDetailedOrPreviousOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
        {
            var query = Data.Get(propertyName, Settings)
                .Where(pair => pair.Key.Tick <= tick); // Tick or less value.

            if (!query.Any())
            {
                return defaultValue;
            }

            query = query.OrderByDescending(pair => pair.Key.Version);

            var itemsToSelect = query.Select(x => x.Value);


            return default;
        }



        public void Dispose()
        {
            Data = null;
            Settings = null;
        }
    }



}
