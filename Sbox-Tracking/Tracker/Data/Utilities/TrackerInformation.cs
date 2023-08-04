using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{

    public class TrackerInformation : IComparable<TrackerInformation>
    {
        public int Tick { get; }
        public int Version { get; }
        public object Data { get; }
        public IReadOnlyCollection<string> Tags { get; }

        internal TrackerInformation(int tick, int version, TrackerMetaData taggedData)
        {
            Tick = tick;
            Version = version;
            Data = taggedData.Data;
            Tags = taggedData.Tags;
        }

        internal TrackerInformation(int tick, int version, object data, IReadOnlyCollection<string> tags)
        {
            Tick = tick;
            Version = version;
            Data = data;
            Tags = tags;
        }

        public int CompareTo(TrackerInformation other)
        {
            if (other == null) return 1;

            int tickComparison = Tick.CompareTo(other.Tick);
            if (tickComparison != 0) return tickComparison;

            int versionComparison = Version.CompareTo(other.Version);
            if (versionComparison != 0) return versionComparison;

            // You may add more comparison logic if needed

            return 0;
        }
    }
}
