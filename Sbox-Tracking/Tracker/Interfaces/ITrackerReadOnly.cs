using System;
using System.Collections.Generic;

namespace Tracking
{
    /// <summary>
    /// Provides read-only access to tracked properties.
    /// </summary>
    public interface ITrackerReadOnly : IDisposable
    {

        /// <summary>
        /// Gets the value of the property with the specified name at the specified tick.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="tick">The tick at which to get the value.</param>
        /// <returns>The value of the property at the specified tick.</returns>
        T GetProperty<T>(string propertyName, int tick);

        /// <summary>
        /// Attempts to get the value of the property at that instance however, will try get previous if doesnt exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="tick"></param>
        /// <returns></returns>
        T GetPropertyOrLast<T>(string propertyName, int tick);


        /// <summary>
        /// Gain access to all versions that property was changed at that tick.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="tick">The tick at which to get the value.</param>
        /// <returns>The values of the property at the specified tick.</returns>
        IEnumerable<T> GetPropertyDetailed<T>(string propertyName, int tick);

        /// <summary>
        /// 
        /// <inheritdoc cref="GetPropertyDetailed{T}(string, int)"/>
        /// If this doesn't exist will get the last tick version instead.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="tick"></param>
        /// <returns></returns>
        IEnumerable<T> GetPropertyDetailedOrLast<T>(string propertyName, int tick);


        /// <summary>
        /// Allows us to check if a key exists, before attempting to get property if never recorded yet, useful for <see cref="TrackingEntity"/> etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        bool GetKeyExists(string propertyName);
    }
}
