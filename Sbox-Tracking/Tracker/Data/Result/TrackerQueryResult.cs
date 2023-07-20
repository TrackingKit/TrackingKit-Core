using System.Collections.Generic;

namespace Tracking
{
    public class TrackerQueryResult : TrackerBaseQueryResult
    {
        KeyValuePair<TrackerKey, object> Value { get; set; }
    }


}
