using Sandbox;
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
        public TrackingPresistanceData(TrackerData data, TrackerRulesService rules)
        {
            Data = data;
            Rules = rules;

        }

        public TrackerData Data { get; }

        public TrackerRulesService Rules { get; }

    }

    // Tracker.Presistance
    public partial class Tracker
    {
        public TrackingPresistanceData CopyData(bool saveRules = false, TagFilter filter = default)
        {
            var copy = new TrackerRangeQuery()
            {
                Filter = filter,
            };


            var dataCloned = Data.Clone(copy);



            TrackingPresistanceData presistanceData = new(dataCloned, default);


            return presistanceData;
        }


        public void MergeData(TrackingPresistanceData presistanceData)
        {
            Data.Merge(presistanceData.Data);
        }

        public void LoadData(TrackingPresistanceData presistanceData)
        {
            Data = presistanceData.Data;
            Rules = presistanceData.Rules;

        }

    }
}
