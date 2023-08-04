using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    public class TrackingPropertyInformation : TrackerInformation
    {
        public string PropertyName { get; }

        internal TrackingPropertyInformation(string propertyName, int tick, int version, TrackerMetaData taggedData)
            : base(tick, version, taggedData)
        {
            PropertyName = propertyName;
        }


        internal TrackingPropertyInformation(string propertyName, int tick, int version, object data, IReadOnlyCollection<string> tags)
            : base(tick, version, data, tags)
        {
            PropertyName = propertyName;
        }

    }
}
