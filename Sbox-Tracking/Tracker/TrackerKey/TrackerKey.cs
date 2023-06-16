﻿using System;
using Sandbox;

namespace Tracking
{
    public struct TrackerKey : IEquatable<TrackerKey>, IComparable<TrackerKey>
    {

        public string PropertyName { get; init; }

        public int Tick { get; init; }

        public int Version { get; init; }

        public string[] Tags { get; init; }

        public int CompareTo(TrackerKey other)
        {
            // Compare Tick values
            int tickComparison = Tick.CompareTo(other.Tick);
            if (tickComparison != 0) return tickComparison;

            // Ticks are equal, so compare Version
            int versionComparison = Version.CompareTo(other.Version);
            if (versionComparison != 0) return versionComparison;

            // Ticks and Versions are equal, so compare PropertyName
            return String.Compare(PropertyName, other.PropertyName, StringComparison.Ordinal);
        }

        public bool Equals(TrackerKey other) => Tick == other.Tick && Version == other.Version && PropertyName == other.PropertyName;

        public override readonly int GetHashCode() => HashCode.Combine(Tick, Version, PropertyName);

    }

}