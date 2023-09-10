using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingKit_Core
{
    public static class InstanceFactory
    {
        public static Func<string, ISQLTrackerStorage> GetOrCreateDatabase() => null;
    }
}
