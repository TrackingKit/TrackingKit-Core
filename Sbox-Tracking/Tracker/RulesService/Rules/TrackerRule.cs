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

        // Optional as some rules might not have a context.

        public virtual Optional<bool> ShouldAdd(string propertyName, object obj) => Optional<bool>.None;

        public virtual Optional<bool> ShouldDelete(TrackerKey key, object obj) => Optional<bool>.None;


    }
}
