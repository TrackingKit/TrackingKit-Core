﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tracking;
using Tracking.Rules;

namespace Tracking.RulesService
{
    internal class TrackerRuleTypeComparer : IEqualityComparer<TrackerRule>
    {
        public bool Equals(TrackerRule x, TrackerRule y)
        {
            if (x == null || y == null)
                return false;

            return x.GetType() == y.GetType();
        }

        public int GetHashCode(TrackerRule obj)
        {
            return obj.GetType().GetHashCode();
        }
    }

    [JsonConverter(typeof(TrackerRulesServiceConverter))]
    public partial class TrackerRulesService : ICloneable
    {
        internal TrackerStorage Data { private get; set; }


        internal TrackerRulesService(TrackerStorage data)
        {
            Data = data;
        }

        internal TrackerRulesService()
        {

        }

        // TODO: Rules enabled option?

        protected HashSet<TrackerRule> Rules { get; set; } = new HashSet<TrackerRule>(new TrackerRuleTypeComparer());


        public T GetOrRegister<T>()
            where T : TrackerRule, new()
        {
            var rule = Rules.OfType<T>().FirstOrDefault();
            if (rule == null)
            {
                rule = new T()
                {
                    Data = Data,
                };

                Rules.Add(rule);
            }

            return rule;
        }

        public void Register<T>()
            where T : TrackerRule, new()
        {
            if (!Rules.OfType<T>().Any())
            {
                var rule = new T()
                {
                    Data = Data 
                };

                Rules.Add(rule);
            }
        }

        public void UnRegister<T>()
            where T : TrackerRule, new()
        {
            var rule = Rules.OfType<T>().FirstOrDefault();
            if (rule != null)
            {
                Rules.Remove(rule);
            }
        }

        public T Get<T>()
            where T : TrackerRule
        {
            return Rules.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetAll<T>()
            where T : TrackerRule
        {
            return Rules.OfType<T>();
        }


        internal void Add<T>(T obj)
            where T : TrackerRule
        {
            Rules.Add(obj);
        }

        public object Clone()
        {
            TrackerRulesService clonedTrackerRulesService = new(Data);

            // Deep copy.
            clonedTrackerRulesService.Rules = new(Rules);


            return clonedTrackerRulesService;
        }

    }
}