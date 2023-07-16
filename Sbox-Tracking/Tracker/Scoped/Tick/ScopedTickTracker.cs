using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    public sealed class ScopedTickTracker : IDisposable
    {
        private IEnumerable<(TrackerKey Key, object Value)> Data { get; }


        internal ScopedTickTracker(IEnumerable<(TrackerKey Key, object Value)> data)
        {
            Data = data;
        }

        public bool Exists(string propertyName)
        {
            var query = Data.Where(x => x.Key.PropertyName == propertyName);

            return query.Any();
        }





        public T Get<T>(string propertyName)
        {
            var query = Data.Where(x => x.Key.PropertyName == propertyName);

            if ( !query.Any() )
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
            var query = Data.Where(x => x.Key.PropertyName == propertyName);


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
            var query = Data.Where(x => x.Key.PropertyName == propertyName);




            return query.Select(pair => (T)pair.Value);
        }

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, IEnumerable<T> defaultValue)
        {
            var query = Data.Where(x => x.Key.PropertyName == propertyName);


            if (!query.Any())
            {
                return defaultValue;
            }

            return query.Select(pair => (T)pair.Value);
        }

        public void Dispose()
        {
            
        }



    }



}
