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
        Exclude 
    }

    public class TagFilter
    {
        // Dictionary to hold the filtering options for each tag
        public Dictionary<string, FilterOption> Tags { get; set; }
        public FilterOption DefaultFilterOption { get; set; }

        public TagFilter()
        {
            Tags = new Dictionary<string, FilterOption>();
            DefaultFilterOption = FilterOption.Include;  // Default behavior
        }

        // Adds a tag with associated filter option
        public void AddTag(string tag, FilterOption filterOption)
        {
            Tags[tag] = filterOption;
        }

        // Checks whether a tag should be included based on the filter option
        public bool ShouldInclude(string tag)
        {
            if (Tags.TryGetValue(tag, out FilterOption filterOption))
            {
                return filterOption == FilterOption.Include;
            }

            // If tag is not present, return default behavior
            return DefaultFilterOption == FilterOption.Include;
        }
    }
}
