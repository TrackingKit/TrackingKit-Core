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

        #region Tick(s)

        public ScopedTickTracker ScopeByTick(int specificTick, params string[] tags)
        {

            return default;
            //return new ScopedTickTracker(Data);
        }


        public ScopedTicksTracker ScopeByTicks(int minTick, int maxTick, TagList tags = default)
        {

            ScopedTickSettings scopedSettings = new(minTick, maxTick, tags);

            return new ScopedTicksTracker(Data, scopedSettings);
        }


        public ScopedTicksTracker ScopeByTicks(TagList tags)
        {

            ScopedTickSettings scopedSettings = new(int.MinValue, int.MaxValue, tags);

            return new ScopedTicksTracker(Data, scopedSettings);
        }

        // TODO: is this needed if params function above does same?
        public ScopedTicksTracker ScopeByTicks()
        {

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
