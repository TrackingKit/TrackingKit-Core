using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tracking.RulesService;

namespace Tracking
{
    public partial class TrackingPresistanceData
    {
        internal TrackingPresistanceData(TrackerStorage data, TrackerRulesService rules)
        {
            //TrackingData = data;
            TrackerRules = rules;

        }

        //public TrackerStorage TrackingData { get; }

        public TrackerRulesService TrackerRules { get; }

    }

    // Tracker.Presistance
    public partial class Tracker
    {
        public TrackingPresistanceData CopyData(bool saveRules = false, TagFilter filter = default)
        {
            throw new Exception();  

            /*

            //var dataCloned = Data.Clone(copy);
            var rulesCloned = (TrackerRulesService)Rules.Clone();


            //TrackingPresistanceData presistanceData = new(dataCloned, rulesCloned);


            return presistanceData;
            */
        }


        public void MergeData(TrackingPresistanceData presistanceData)
        {
            //Data.Merge(presistanceData.TrackingData);
        }

        public void LoadData(TrackingPresistanceData presistanceData)
        {
            //Data = presistanceData.TrackingData;
            //Rules = presistanceData.TrackerRules;

        }

    }
}
