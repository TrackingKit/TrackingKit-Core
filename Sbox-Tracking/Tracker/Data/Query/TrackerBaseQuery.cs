using System.Collections.Generic;

namespace Tracking
{
    public class TrackerBaseQuery
    {
        public string PropertyName { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }


}
