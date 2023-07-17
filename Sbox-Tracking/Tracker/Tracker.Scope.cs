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

        #region Tick(s)

        public ScopedTickTracker ScopeByTick(int specificTick, params string[] tags)
        {
            if (!CanScope(specificTick, specificTick, false, tags)) return null;


            return default;
            //return new ScopedTickTracker(Data);
        }


        public ScopedTicksTracker ScopeByTicks(int minTick, int maxTick, params string[] tags)
        {
            if (!CanScope(minTick, maxTick, false, tags)) return null;


            ScopedTickSettings scopedSettings = new(minTick, maxTick, tags);

            return new ScopedTicksTracker(Data, scopedSettings);
        }


        public ScopedTicksTracker ScopeByTicks(params string[] tags)
        {
            if (!CanScope(int.MinValue, int.MaxValue, false, tags)) return null;

            ScopedTickSettings scopedSettings = new(int.MinValue, int.MaxValue, tags);

            return new ScopedTicksTracker(Data, scopedSettings);
        }

        // TODO: is this needed if params function above does same?
        public ScopedTicksTracker ScopeByTicks()
        {
            if (!CanScope(int.MinValue, int.MaxValue, false)) return null;


            ScopedTickSettings scopedSettings = new(int.MinValue, int.MaxValue);

            return new ScopedTicksTracker(Data, scopedSettings);
        }

        #endregion


        #region Second(s)

        public ScopedSecondsTracker ScopeBySeconds(params string[] tags)
        {
            ScopedSecondSettings scopedSettings = new(float.MinValue, float.MaxValue, tags);

            return new ScopedSecondsTracker(Data, scopedSettings);
        }

        public ScopedSecondsTracker ScopeBySeconds(float minSecond, float maxSecond, params string[] tags)
        {
            ScopedSecondSettings scopedSettings = new(minSecond, maxSecond, tags);

            return new ScopedSecondsTracker(Data, scopedSettings);
        }




        #endregion

    }
}
