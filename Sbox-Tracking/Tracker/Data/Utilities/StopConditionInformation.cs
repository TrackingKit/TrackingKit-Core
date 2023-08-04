using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    public class StopConditionInformation : TrackerInformation
    {
        public int AccumulatedSoFar { get; }

        internal StopConditionInformation(int tick, int version, TrackerMetaData taggedData, int accumulatedSoFar)
            : base(tick, version, taggedData)
        {
            AccumulatedSoFar = accumulatedSoFar;
        }
    }
}
