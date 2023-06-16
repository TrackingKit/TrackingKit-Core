using System;
using Sandbox;

namespace Tracking
{
    public struct TrackerKey : IEquatable<TrackerKey>, IComparable<TrackerKey>
    {

        public string PropertyName { get; init; }

        public int Tick { get; init; }

        public int Version { get; init; }

        public string[] Tags { get; init; }

        public int CompareTo(TrackerKey other) => other.Version - Version; // TODO: Is this right?

        public bool Equals(TrackerKey other) => Tick == other.Tick && Version == other.Version && PropertyName == other.PropertyName;

        public override readonly int GetHashCode() => HashCode.Combine(Tick, Version, PropertyName);

    }

}