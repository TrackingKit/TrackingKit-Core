using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





namespace Tracking
{

    public partial class Tracker
    {

        #region Tick(s)

        public ScopedTickTracker ScopeByTick(int specificTick, TagFilter filter = default)
        {

            return default;
            //return new ScopedTickTracker(Data);
        }


        public ScopedTicksTracker ScopeByTicks(int minTick, int maxTick, TagFilter filter = default)
        {

            ScopedTickSettings scopedSettings = new(minTick, maxTick, filter);

            return new ScopedTicksTracker(Data, scopedSettings);
        }


        public ScopedTicksTracker ScopeByTicks(TagFilter filter = default)
        {

            ScopedTickSettings scopedSettings = new(int.MinValue, int.MaxValue, filter);

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

        public ScopedSecondsTracker ScopeBySeconds(TagFilter filter = default)
        {
            ScopedSecondSettings scopedSettings = new(float.MinValue, float.MaxValue, filter);

            return new ScopedSecondsTracker(Data, scopedSettings);
        }

        public ScopedSecondsTracker ScopeBySeconds(float minSecond, float maxSecond, TagFilter filter = default)
        {
            ScopedSecondSettings scopedSettings = new(minSecond, maxSecond, filter);

            return new ScopedSecondsTracker(Data, scopedSettings);
        }


        #endregion

    }
}
