using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox;
using System.Threading.Tasks;

namespace Tracking
{
    public partial class Tracker
    {
        public T GetProperty<T>(string propertyName, int tick) => GetPropertyDetailed<T>(propertyName, tick).Last(); // TODO: Probably unoptimised.

        public IEnumerable<T> GetPropertyDetailed<T>(string propertyName, int tick)
        {
            if(Filter != null)
            {
                if (!Filter.InRange(tick))
                {
                    Log.Error("Tick provided not in range");
                    return default;
                }
            }

            var filteredPropertyKeys = Values.Keys
               .Where( x => x.PropertyName == propertyName );

            if (filteredPropertyKeys.Count() == 0)
            {
                Log.Error($"Key doesnt exist {propertyName}");
                return default;
            }

            var filteredKeys = filteredPropertyKeys
               .Where(x => x.Tick == tick);

            if(filteredKeys.Count() == 0)
            {
                Log.Error($"Keys exist but, no key's found at tick: {tick} for propertyName: {propertyName} ");
            }

            // TODO: Consider Filter Keys also.

            var filteredValues = filteredKeys.Select(x => Values[x]).OfType<T>();

            // TODO: If hides any values due to OfType we spew error instead as this is hiding data
            // that might be corrupted somehow, I think, just needs considering.

            return filteredValues;
        }

        public IEnumerable<T> GetPropertyDetailed<T>(string propertyName)
        {
            if (Filter == null)
                throw new Exception("Requires a Filter");

            if (!Filter.IsSpecificTick)
                throw new Exception("Requires a specific tick filter");

            return GetPropertyDetailed<T>(propertyName, Filter.MinTick);
        }

        public T GetProperty<T>(string propertyName) => GetProperty<T>(propertyName, Filter.IsSpecificTick ? Filter.MinTick : throw new Exception("Not a specific tick scope") );


        public T GetObject<T>(int tick) where T : IManualTrackableObject
        {
            throw new NotImplementedException();
        }

        public T GetObject<T>() where T : IManualTrackableObject
        {
            throw new NotImplementedException();
        }

        public T GetPropertyOrLast<T>(string propertyName, int tick)
        {
            throw new NotImplementedException();
        }

        public T GetPropertyOrLast<T>(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}
