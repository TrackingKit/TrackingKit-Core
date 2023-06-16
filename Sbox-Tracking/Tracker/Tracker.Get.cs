using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox;
using System.Threading.Tasks;

namespace Tracking
{
    // TODO: Should we just let 

    public partial class Tracker : ITrackerReadOnly
    {
        protected bool CanGet( int tick )
        {
            // If we're scoped check conditions.
            if ( IsScoped )
            {
                // Is the tick in the range.
                if ( (tick <= OutputFilterSettings.MinTick) || (tick >= OutputFilterSettings.MaxTick) )
                {
                    Log.Error("Tick is not in range of scope");
                    return false;
                }
            }


            return true;
        }

        protected IEnumerable<TrackerKey> GetKeysByPropertyName(string propertyName)
        {
            var filteredPropertyKeys = Values.Keys
               .Where(x => x.PropertyName == propertyName);

            if (filteredPropertyKeys.Count() == 0)
            {
                Log.Error($"Key doesn't exist {propertyName}");
                return null;
            }

            return filteredPropertyKeys;
        }

        public T GetProperty<T>(string propertyName, int tick) => GetPropertyDetailed<T>(propertyName, tick).Last(); // TODO: Probably unoptimised.

        public IEnumerable<T> GetPropertyDetailed<T>(string propertyName, int tick)
        {
            if (!CanGet(tick)) return default;

            var filteredPropertyKeys = GetKeysByPropertyName(propertyName);
            if (filteredPropertyKeys == null) return default;


            var filteredKeys = filteredPropertyKeys
               .Where(x => x.Tick == tick);

            if(!filteredKeys.Any())
            {
                Log.Error($"Keys exist but, no key's found at tick: {tick} for propertyName: {propertyName} ");
            }

            // TODO: Consider Filter Keys also.

            var filteredValues = filteredKeys.Select(x => Values[x]).OfType<T>();

            // TODO: If hides any values due to OfType we spew error instead as this is hiding data
            // that might be corrupted somehow, I think, just needs considering.

            return filteredValues;
        }

        public int SpecificObjectTick { get; set; } = 0;

        public object Scoped { get; set; }

        public T GetObject<T>(T obj, int tick) where T : IManualTrackableObject
        {
            SpecificObjectTick = tick;

            Scoped = obj;

            return obj;
        }


        public T GetPropertyOrLast<T>(string propertyName, int tick) => GetPropertyDetailedOrLast<T>(propertyName, tick).Last();

        public IEnumerable<T> GetPropertyDetailedOrLast<T>(string propertyName, int tick)
        {
            // Get keys for the specific propertyName
            var filteredPropertyKeys = GetKeysByPropertyName(propertyName);
            if (filteredPropertyKeys == null) return default;

            // Get keys for the specific tick
            var specificTickKeys = filteredPropertyKeys
               .Where(x => x.Tick == tick);

            IEnumerable<T> result;

            if (specificTickKeys.Count() > 0)
            {
                // If keys for specific tick are found, get the associated values
                result = specificTickKeys.Select(x => Values[x]).OfType<T>();
            }
            else
            {
                // If no keys are found for the specific tick, get the last recorded property values
                result = filteredPropertyKeys
                    .OrderByDescending(x => x.Tick)  // Order by tick in descending order to get the latest ones first
                    .Select(x => Values[x]).OfType<T>();
            }

            return result;
        }


    }

    // Design note: This is interface only as it requires a filter, so I recommend you use Scope(tick). Potentially changed.
    public partial class Tracker : ITrackerTickReadOnly
    {

        IEnumerable<T> ITrackerTickReadOnly.GetPropertyDetailed<T>(string propertyName)
        {
            if (!IsScoped)
            {
                Log.Error("Not scoped");
                return default;
            }

            

            return default;
        }

        T ITrackerTickReadOnly.GetProperty<T>(string propertyName)
        {
            if (!IsScoped)
            {
                Log.Error("Not scoped");
                return default;
            }

            return default;

        }

        T ITrackerTickReadOnly.GetObject<T>( T obj )
        {
            if (!IsScoped)
            {
                Log.Error("Not scoped");
                return default;
            }

            Scoped = obj;

            // As its specific tick.
            SpecificObjectTick = OutputFilterSettings.MinTick;

            return obj;
        }
            

        T ITrackerTickReadOnly.GetPropertyOrLast<T>(string propertyName)
        {
            if (!IsScoped)
            {
                Log.Error("Not scoped");
                return default;
            }

            if (!OutputFilterSettings.IsSpecificTick)
            {
                Log.Error("not specific tick");
                return default;
            }



            return GetPropertyOrLast<T>(propertyName, OutputFilterSettings.MinTick);

        }

        IEnumerable<T> ITrackerTickReadOnly.GetPropertyDetailedOrLast<T>(string propertyName)
        {
            if (!IsScoped)
            {
                Log.Error("Not scoped");
                return default;
            }

            return default;

        }
    }

    
}
