using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    /// <summary>
    /// Provides tracking of properties within specified groups.
    /// </summary>
    public interface ITracker : ITrackerReadOnly, ITrackerTickReadOnly
    {
        bool IsScoped { get; }


        // TODO: Is this a good idea? Can stop properties we dont want to track in certain situations
        // but whitelist them all by default.

        /*
        bool IsTracked(string propertyName);

        void Unblacklist(string propertyName);

        void Blacklist(string propertyName);
        */

        /// <summary>
        /// Begins a new tracking scope with the specified identifiers and tick.
        /// </summary>
        /// <param name="specificTick">The specificTick for tracking.</param>
        /// <param name="idents">The identifiers for the scope.</param>
        /// <returns>A read-only version of the tracker for this scope.</returns>
        ITrackerTickReadOnly Scope(int specificTick, params string[] idents);

        /// <summary>
        /// Begins a new tracking scope with the specified identifiers and a range.
        /// </summary>
        /// <param name="minTick">The min range for tracking.</param>
        /// <param name="maxTick">The min range for tracking.</param>
        /// <param name="idents">The identifiers for the scope.</param>
        /// <returns>A read-only version of the tracker for this scope.</returns>
        ITrackerReadOnly Scope(int minTick, int maxTick, params string[] idents);

        /// <summary>
        /// Begins a new tracking scope with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the scope.</param>
        /// <returns>A read-only version of the tracker for this scope.</returns>
        ITrackerReadOnly Scope(params string[] idents);

        ITrackerReadOnly Scope();

        /// <summary>
        /// Begins a new group with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the group.</param>
        void StartGroup(params string[] idents);

        /// <summary>
        /// Ends the group with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the group.</param>
        void EndGroup(params string[] idents);

        /// <summary>
        /// Sets the value of a property with the specified name within the current groups.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="idents">The identifiers for the property.</param>
        void Set(string propertyName, object value, params string[] idents);

        bool KeyExistsInTracker(string propertyName);

    }

    /// <summary>
    /// Provides read-only access to tracked properties.
    /// </summary>
    public interface ITrackerReadOnly : IDisposable
    {
        /// <summary>
        /// Properties in GetObject will use <see cref="GetPropertyOrLast"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tick"></param>
        /// <returns></returns>
        T GetObject<T>(int tick)
            where T : IManualTrackableObject;

        /// <summary>
        /// Gets the value of the property with the specified name at the specified tick.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="tick">The tick at which to get the value.</param>
        /// <returns>The value of the property at the specified tick.</returns>
        T GetProperty<T>(string propertyName, int tick);

        T GetPropertyOrLast<T>(string propertyName, int tick);


        /// <summary>
        /// Gain access to all versions that property was changed at that tick.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="tick">The tick at which to get the value.</param>
        /// <returns>The values of the property at the specified tick.</returns>
        IEnumerable<T> GetPropertyDetailed<T>(string propertyName, int tick);

        IEnumerable<T> GetPropertyDetailed<T>(string propertyName);
    }

    public interface ITrackerTickReadOnly : IDisposable
    {
        T GetObject<T>()
            where T : IManualTrackableObject;

        /// <summary>
        /// Gets the value of the property with the specified name. (It gets the last version on that tick)
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property at the specified tick.</returns>
        T GetProperty<T>(string propertyName);

        T GetPropertyOrLast<T>(string propertyName);


        /// <summary>
        /// Gain access to all versions that property was changed during that tick.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The values of the property at the specified tick.</returns>
        IEnumerable<T> GetPropertyDetailed<T>(string propertyName);
    }
}
