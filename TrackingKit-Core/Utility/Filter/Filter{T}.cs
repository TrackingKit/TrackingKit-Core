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
    public class Filter<T> : IReadOnlyFilter<T>, ICloneable
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

        /// <summary> Allows to clear all. </summary>
        public void ClearAll() => Values.Clear();

        /// <summary> Gets the number of distinct values currently in the filter. </summary>
        public int Count => Values.Count;


        /// <summary>
        /// Determines whether a value should be included based on the filter option.
        /// </summary>
        /// <param name="ident">The value to check.</param>
        /// <returns>True if the value should be included; otherwise, false.</returns>
        public bool ShouldInclude(T ident)
            => Values.TryGetValue(ident, out FilterOption filterOption) ? filterOption == FilterOption.Include : DefaultFilterOption == FilterOption.Include;


        /// <summary>
        /// Determines whether a specific value is already present in the filter.
        /// </summary>
        /// <param name="ident">The value to check.</param>
        /// <returns>True if the filter contains the value; otherwise, false.</returns>
        public bool Contains(T ident) => Values.ContainsKey(ident);

        /// <summary>
        /// Toggles the filter option for a given value. If the value is set to "Include", it changes to "Exclude" and vice versa. 
        /// If the value isn't in the filter, it will be set based on the opposite of the default filter option.
        /// </summary>
        /// <param name="ident">The value whose filter option should be toggled.</param>
        public void Negate(T ident)
        {
            if (Values.TryGetValue(ident, out FilterOption filterOption))
            {
                Values[ident] = filterOption == FilterOption.Include ? FilterOption.Exclude : FilterOption.Include;
            }
            else
            {
                Set(ident, DefaultFilterOption == FilterOption.Include ? FilterOption.Exclude : FilterOption.Include);
            }
        }

        /// <summary>
        /// Sets the filter option for multiple values at once.
        /// </summary>
        /// <param name="idents">The collection of values to set.</param>
        /// <param name="filterOption">The filter option to associate with the provided values.</param>
        public void SetBulk(IEnumerable<T> idents, FilterOption filterOption)
        {
            foreach (var ident in idents)
            {
                Set(ident, filterOption);
            }
        }

        /// <summary>
        /// Determines whether all values in the collection should be included based on the filter options.
        /// </summary>
        /// <param name="idents">The values to check.</param>
        /// <returns>True if all values should be included; otherwise, false.</returns>
        public bool ShouldIncludes(IEnumerable<T> idents)
            => idents.All(ShouldInclude);

        public Filter<T> Clone()
        {
            var copy = new Filter<T> { DefaultFilterOption = this.DefaultFilterOption };
            foreach (var item in Values)
            {
                copy.Set(item.Key, item.Value);
            }
            return copy;
        }

        object ICloneable.Clone() => Clone();
    }
}
