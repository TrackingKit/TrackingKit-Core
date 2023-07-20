using System.Collections.Generic;

namespace Tracking
{
    public class TrackerDetailedQueryResult : TrackerBaseQueryResult
    {
        public IEnumerable<KeyValuePair<TrackerKey, object>> Values { get; set; }
    }


}
