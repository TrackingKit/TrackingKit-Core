using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingKit_Core
{
    public enum TrackerStorageType
    {
        SQL,

        /// <summary> Where everything is stored inside memory. </summary>
        InMemory,

        [Obsolete("Decide on this.")]
        JSON,

        Hybrid
    }
}
