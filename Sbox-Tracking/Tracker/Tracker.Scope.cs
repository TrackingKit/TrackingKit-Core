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

        public ScopedTickTracker Scope(int specificTick, params string[] idents)
        {
            if (!CanScope(specificTick, specificTick, false, idents)) return null;


            var scopeSettings = new ScopeSettings()
            {
                MinTick = specificTick,
                MaxTick = specificTick,
                Tags = idents
            };

            return new ScopedTickTracker(Data, scopeSettings);
        }


        public ScopedTracker Scope(int minTick, int maxTick, params string[] idents)
        {
            if (!CanScope(minTick, maxTick, false, idents)) return null;

            // TOOD: If canscope an issue maybe swap I think.
            var scopeSettings = new ScopeSettings()
            {
                MinTick = minTick,
                MaxTick = maxTick,
                Tags = idents
            };

            return new ScopedTracker(Data, scopeSettings);
        }

        public ScopedTracker Scope(params string[] idents)
        {
            if (!CanScope(int.MinValue, int.MaxValue, false, idents)) return null;


            var scopeSettings = new ScopeSettings()
            {
                MinTick = int.MinValue,
                MaxTick = int.MaxValue,
                Tags = idents,
            };

            return new ScopedTracker(Data, scopeSettings);
        }

        public ScopedTracker Scope()
        {
            if (!CanScope(int.MinValue, int.MaxValue, false)) return null;


            var scopeSettings = new ScopeSettings()
            {
                MinTick = int.MinValue,
                MaxTick = int.MaxValue,
            };

            return new ScopedTracker(Data, scopeSettings);
        }

    }
}
