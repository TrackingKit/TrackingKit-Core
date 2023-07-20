using System.Collections.Generic;

namespace Tracking
{
    public class TrackerQueryResult : TrackerBaseQueryResult
    {
        public KeyValuePair<TrackerKey, object> Value { get; set; }
    }


}
