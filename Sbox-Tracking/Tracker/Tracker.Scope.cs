using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.Gizmo;

namespace Tracking
{

    public partial class Tracker
    {
        
        protected bool CanScope(int minTick, int maxTick, bool supressMessages = true, params string[] idents)
        {
            if(minTick > maxTick)
            {
                if (!supressMessages) Log.Warning("MinTick greater than MaxTick");
            }

            return true;
        }


        // TODO: Sort this all out.

        public ScopedTickTracker ScopeByTick(int specificTick, params string[] tags)
        {
            if (!CanScope(specificTick, specificTick, false, tags)) return null;


            var scopeData = Data.GenerateScopeTicks(specificTick, specificTick, tags);

            return new ScopedTickTracker(scopeData);
        }


        public ScopedTicksTracker ScopeByTicks(int minTick, int maxTick, params string[] tags)
        {
            if (!CanScope(minTick, maxTick, false, tags)) return null;

            var scopeData = Data.GenerateScopeTicks(minTick, maxTick, tags);

            return new ScopedTicksTracker(scopeData);
        }

        public ScopedTicksTracker ScopeByTicks(params string[] idents)
        {
            if (!CanScope(int.MinValue, int.MaxValue, false, idents)) return null;


            var scopeData = Data.GenerateScopeTicks(int.MinValue, int.MaxValue, idents);

            return new ScopedTicksTracker(scopeData);
        }

        public ScopedTicksTracker ScopeByTicks()
        {
            if (!CanScope(int.MinValue, int.MaxValue, false)) return null;


            var scopeData = Data.GenerateScopeTicks(int.MinValue, int.MaxValue);

            return new ScopedTicksTracker(scopeData);
        }

    }
}
