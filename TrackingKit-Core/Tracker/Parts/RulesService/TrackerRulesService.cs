using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tracking;
using Tracking.Rules;
using TrackingKit_Core.Rosyln;

namespace Tracking.RulesService
{
    
    internal class TrackerRuleTypeComparer : IEqualityComparer<ITrackerRule>
    {
        public bool Equals(ITrackerRule x, ITrackerRule y)
        {
            if (x == null || y == null)
                return false;

            return x.GetType() == y.GetType();
        }

        public int GetHashCode(ITrackerRule obj)
        {
            return obj.GetType().GetHashCode();
        }
    }

    [JsonConverter(typeof(TrackerRulesServiceConverter))]
    public partial class TrackerRulesService : ICloneable
    {
        internal TrackerRulesService()
        {
            
        }

        // TODO: Rules enabled option?

        protected HashSet<ITrackerRule> Rules { get; set; } = new HashSet<ITrackerRule>(new TrackerRuleTypeComparer());


        public T GetOrRegister<T>()
            where T : ITrackerRule, new()
        {
            var rule = Rules.OfType<T>().FirstOrDefault();
            if (rule == null)
            {
                Rules.Add(rule);
            }

            return rule;
        }

        public void Register<T>()
            where T : ITrackerRule, new()
        {
            if (!Rules.OfType<T>().Any())
            {
                Rules.Add(new T());
            }
        }

        public void UnRegister<T>()
            where T : ITrackerRule, new()
        {
            var rule = Rules.OfType<T>().FirstOrDefault();
            if (rule != null)
            {
                Rules.Remove(rule);
            }
        }

        public T Get<T>()
            where T : ITrackerRule
        {
            return Rules.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetAll<T>()
            where T : ITrackerRule
        {
            return Rules.OfType<T>();
        }


        internal void Add<T>(T obj)
            where T : ITrackerRule
        {
            Rules.Add(obj);
        }

        public object Clone()
        {
            TrackerRulesService clonedTrackerRulesService = new();

            // Deep copy.
            clonedTrackerRulesService.Rules = new(Rules);


            return clonedTrackerRulesService;
        }

    }
}
