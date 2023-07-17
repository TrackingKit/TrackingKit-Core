using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public sealed class ScopedSecondsTracker
    {
        private IEnumerable<(TrackerKey Key, object Value)> Data { get; }


        internal ScopedSecondsTracker(IEnumerable<(TrackerKey Key, object Value)> data)
        {
            Data = data;
        }


    }
}
