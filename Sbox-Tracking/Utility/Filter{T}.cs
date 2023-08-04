using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public enum FilterOption
    {
        Include,
        Exclude,
    }

    /// <summary>
    /// Represents a filter for a specific type T.
    /// </summary>
    /// <typeparam name="T">The type to filter. Must implement IEquatable.</typeparam>
    public class Filter<T> : IReadOnlyFilter<T>
        where T : IEquatable<T>
    {
        protected readonly Dictionary<T, FilterOption> Values;

        /// <summary>
        /// Gets or sets the default filter option.
        /// </summary>
        public FilterOption DefaultFilterOption { get; set; } = FilterOption.Include;

        /// <summary>
        /// Initializes a new instance of the Filter class.
        /// </summary>
        public Filter()
        {
            Values = new Dictionary<T, FilterOption>();
        }

        /// <summary>
        /// Adds or updates a value with an associated filter option.
        /// </summary>
        /// <param name="ident">The value to set.</param>
        /// <param name="filterOption">The filter option to associate with the value.</param>
        public void Set(T ident, FilterOption filterOption) 
            => Values[ident] = filterOption;

        /// <summary>
        /// Removes a value from the filter.
        /// </summary>
        /// <param name="ident">The value to remove.</param>
        public void Remove(T ident) 
            => Values.Remove(ident);

        /// <summary>
        /// Determines whether a value should be included based on the filter option.
        /// </summary>
        /// <param name="ident">The value to check.</param>
        /// <returns>True if the value should be included; otherwise, false.</returns>
        public bool ShouldInclude(T ident)
            => Values.TryGetValue(ident, out FilterOption filterOption) ? filterOption == FilterOption.Include : DefaultFilterOption == FilterOption.Include;

        /// <summary>
        /// Determines whether all values in the collection should be included based on the filter options.
        /// </summary>
        /// <param name="idents">The values to check.</param>
        /// <returns>True if all values should be included; otherwise, false.</returns>
        public bool ShouldIncludes(IEnumerable<T> idents)
            => idents.All(ShouldInclude);
    }


    public interface IReadOnlyFilter<T>
    {
        public bool ShouldInclude(T ident);

        bool ShouldIncludes(IEnumerable<T> idents);

    }
}
