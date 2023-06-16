using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    /// <summary>
    /// 
    /// </summary>
    public partial class InputFilterSettings
    {
        #region Whitelist

        protected IDictionary<string, FilterType> ValueFilters { get; set; } = new Dictionary<string, FilterType>();


        // What we assume the filter is for any elements not recorded.
        public FilterType DefaultType { get; set; } = FilterType.Whitelist;
        
        public bool IsPropertyWhitelisted(string propertyName)
        {
            bool exists = ValueFilters.TryGetValue(propertyName, out FilterType filterType);

            if (exists)
                return filterType == FilterType.Whitelist;
            else
                return DefaultType == FilterType.Whitelist;
        }

        public void Whitelist(string propertyName) => ValueFilters.Add(propertyName, FilterType.Whitelist);

        public void Blacklist(string propertyName) => ValueFilters.Add(propertyName, FilterType.Blacklist);


        public enum FilterType
        {
            Whitelist,
            Blacklist,
        }

        #endregion


        /// <summary> 
        /// <para>
        /// Allow consecutive duplicate values recorded.
        /// </para>
        /// 
        /// <para>Example: Such as if we update position to 0 twice in a row we wont re-record it. </para>
        /// 
        /// <para> <strong>Note:</strong> This being disabled can cause performance decrease for less potentially stored. </para>
        /// </summary>
        public bool AllowConsecutiveDuplicateValues { get; set; } = false;

    }
}
