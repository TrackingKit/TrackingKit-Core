using System;
using System.Collections.Generic;
using Sandbox;

namespace Tracking
{
    public struct TrackerKey : IEquatable<TrackerKey>, IComparable<TrackerKey>
    {
        public string PropertyName { get; }

        public int Tick { get; }

        public readonly double Second => TimeUtility.TickToSecond(Tick);

        public int Version { get; }



        public IReadOnlyCollection<string> Tags { get; }


        public TrackerKey(string propertyName, int tick, int version)
        {
            PropertyName = string.Intern(propertyName); // We store in pool, not sure if best.
            Tick = tick;
            Version = version;
            Tags = new HashSet<string>();
        }

        public TrackerKey(string propertyName, int tick, int version, IReadOnlyCollection<string> tags )
        {
            PropertyName = string.Intern(propertyName); // We store in pool, not sure if best.
            Tick = tick;
            Version = version;
            Tags = new HashSet<string>(tags);
        }


        public bool Equals(TrackerKey other)
        {
            return PropertyName == other.PropertyName && Tick == other.Tick && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            return obj is TrackerKey key && Equals(key);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PropertyName, Tick, Version);
        }

        public int CompareTo(TrackerKey other)
        {
            int propertyComparison = string.Compare(PropertyName, other.PropertyName, StringComparison.Ordinal);
            if (propertyComparison != 0) return propertyComparison;

            int tickComparison = Tick.CompareTo(other.Tick);
            if (tickComparison != 0) return tickComparison;

            return Version.CompareTo(other.Version);
        }
    }


}