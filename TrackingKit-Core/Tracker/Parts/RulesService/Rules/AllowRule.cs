using System;
using Tracking.Rules;

namespace Tracking
{
    /// <summary>
    /// Allows to filter which properties are included, excluded, etc. at the time.
    /// </summary>
    public class AllowRule<TKey> : ITrackerRule<TKey>
        where TKey : IEquatable<TKey>
    {
        public readonly Filter<TKey> Filter = new();

        public bool? ShouldAdd(TKey propertyName, object obj)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Property name cannot be null.");

            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Object cannot be null.");


            // Throw null to avoid it accepting it without checking other rules.
            return Filter.ShouldInclude(propertyName) ? null : false;
        }
    }
}
