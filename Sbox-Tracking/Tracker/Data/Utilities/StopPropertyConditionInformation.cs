using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    public class StopPropertyConditionInformation : TrackingPropertyInformation
    {
        public int AccumulatedSoFar { get; }

        internal StopPropertyConditionInformation(string propertyName, int tick, int version, TrackerMetaData taggedData, int accumulatedSoFar)
            : base(propertyName, tick, version, taggedData)
        {
            AccumulatedSoFar = accumulatedSoFar;
        }

        internal StopPropertyConditionInformation(string propertyName, int tick, int version, object data, IReadOnlyCollection<string> tags, int accumulatedSoFar)
            : base(propertyName, tick, version, data, tags)
        {
            AccumulatedSoFar = accumulatedSoFar;
        }
    }
}
