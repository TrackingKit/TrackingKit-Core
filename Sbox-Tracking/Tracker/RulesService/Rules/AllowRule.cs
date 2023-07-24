using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Tracking.Rules
{

    /// <summary> Allows to filter what proerties are whitelisted, blacklisted etc at the time. </summary>
    public partial class AllowRule : TrackerRule
    {
        public enum Filter
        {
            Whitelisted,
            Blacklisted,
        }

        protected Dictionary<string, Filter> Properties { get; set; } = new Dictionary<string, Filter>();


        public Filter Default { get; set; } = Filter.Whitelisted;


        public void AddProperty(string propertyName, Filter filter)
        {

            if (!Properties.ContainsKey(propertyName))
            {
                Properties.Add(propertyName, filter);
            }
        }

        public void RemoveProperty(string propertyName)
        {
            if (Properties.ContainsKey(propertyName))
            {
                Properties.Remove(propertyName);
            }
        }



        public override bool? ShouldAdd(string propertyName, object obj)
        {
            // Check if the propertyName or obj is null
            if (propertyName == null || obj == null)
                return null;

            // Check if the property exists in the Properties dictionary
            if (Properties.TryGetValue(propertyName, out Filter propertyFilter))
            {
                // If property is blacklisted, don't add
                if (propertyFilter == Filter.Blacklisted)
                {
                    return false;
                }

                // If property is whitelisted, add
                if (propertyFilter == Filter.Whitelisted)
                {
                    return true;
                }
            }

            // If property doesn't exist in the dictionary, check the Default filter
            if (Default == Filter.Blacklisted)
            {
                return false;
            }

            return base.ShouldAdd(propertyName, obj);
        }
    }
}
