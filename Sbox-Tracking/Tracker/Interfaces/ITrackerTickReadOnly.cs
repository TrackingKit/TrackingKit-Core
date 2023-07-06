using System;
using System.Collections.Generic;

namespace Tracking
{
    /// <summary> When scoping a single tick. </summary>
    public interface ITrackerTickReadOnly : IDisposable
    {
        /// <summary>
        /// Gets the value of the property with the specified name. (It gets the last version on that tick)
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property at the specified tick.</returns>
        T GetProperty<T>(string propertyName);

        /// <summary>
        /// <inheritdoc cref="GetProperty"/> If no value exists at that Tick we will get the last one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        T GetPropertyOrLast<T>(string propertyName);


        /// <summary>
        /// Gain access to all versions that property was changed during that tick.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The values of the property at the specified tick.</returns>
        IEnumerable<T> GetPropertyDetailed<T>(string propertyName);

        /// <summary>
        /// <inheritdoc cref="GetPropertyDetailed{T}(string)"/> If doesn't exist will get the last Tick one if exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        IEnumerable<T> GetPropertyDetailedOrLast<T>(string propertyName);
    }
}
