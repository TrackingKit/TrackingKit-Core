using System;
using Sandbox;

namespace Tracking
{
    public struct TrackerKey : IEquatable<TrackerKey>, IComparable<TrackerKey>
    {
        public TrackerKey()
        {
            Tick = Time.Tick;
        }

        public string PropertyName { get; init; }

        public int Tick { get; }

        public int Version { get; init; }

        public string[] Tags { get; init; }

        public int CompareTo(TrackerKey other) => other.Version - Version; // TODO: Is this right?

        public bool Equals(TrackerKey other) => Tick == other.Tick && Version == other.Version;

        public override readonly int GetHashCode() => HashCode.Combine(Tick, Version);
    }

}