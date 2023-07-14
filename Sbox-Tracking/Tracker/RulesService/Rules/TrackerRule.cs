using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Tracking.Rules
{
    public abstract class TrackerRule
    {
        public ITrackerDataReadOnly Data { get; internal init; }

        




        // Optional as some rules might not have a context.

        public virtual bool? ShouldAdd(string propertyName, object obj) => null;

        public virtual bool? ShouldDelete(TrackerKey key, object obj) => null;


    }
}
