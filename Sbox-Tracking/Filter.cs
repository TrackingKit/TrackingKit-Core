using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public enum FilterOption
    {
        Include,
        Exclude,
    }

    public class Filter<T>
        where T : IEquatable<T>
    {


        protected Dictionary<T, FilterOption> Values { get; set; } = new();

        public FilterOption DefaultFilterOption { get; set; } = FilterOption.Include;


        // Adds a tag with associated filter option
        public void Set(T ident, FilterOption filterOption)
            => Values[ident] = filterOption;

        public void Remove(T ident) 
            => Values.Remove(ident);


        // Checks whether a tag should be included based on the filter option
        public bool ShouldInclude(T ident)
        {
            if (Values.TryGetValue(ident, out FilterOption filterOption))
                return filterOption == FilterOption.Include;

            // If tag is not present, return default behavior
            return DefaultFilterOption == FilterOption.Include;
        }

    }

}
