using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking.Rules;

namespace Tracking.Rules
{
    [Obsolete("This is not gonna be good.")]
    /// <summary> Duplicate rule ensures properties with same value as last time arent stored repeatadly. </summary>
    public class DuplicateRule : ITrackerRule
    {
        private Dictionary<string, object> lastAddedPerProperty = new Dictionary<string, object>();

        [Obsolete("Not implemented yet.")]
        public bool ShouldReplaceLastVersion { get; set; } = false;


        public bool? ShouldAdd(string propertyName, object obj)
        {
            if (lastAddedPerProperty.TryGetValue(propertyName, out var lastAdded) && Equals(lastAdded, obj))
            {
                // This is a duplicate of the last added object for this property, don't add it.
                return false;
            }
            else
            {

                // This is not a duplicate, add it and update the last added object for this property.
                lastAddedPerProperty[propertyName] = obj;
                return true;
            }
        }
    }
}
