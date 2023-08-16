using Sandbox;
using TrackingKit_Core.TrackingKit_Core.Factories;

namespace Tracking
{

    public partial class ScopedTickSettings
    {
        public int MinTick { get; }

        public int MaxTick { get; }

        public bool IsSpecificTick => MinTick == MaxTick;

        public IReadOnlyTagFilter Filter { get; }

        public int SpecificTick
        {
            get
            {
                if (!IsSpecificTick)
                {
                    LogFactory.Error("Not specific tick");
                    return -1; // return an invalid value or throw an exception
                }

                return MaxTick;
            }
        }

        public ScopedTickSettings(int minTick, int maxTick, TagFilter filter = default)
        {
            if (minTick > maxTick)
            {
                LogFactory.Warning("Swapped MinTick and MaxTick as MinTick greater than MaxTick");
                MinTick = maxTick;
                MaxTick = minTick;
            }
            else
            {
                MinTick = minTick;
                MaxTick = maxTick;
            }

            Filter = filter;
        }

        public void ClampMinAndWarn(ref int minTick)
        {
            if(minTick < MinTick)
            {
                LogFactory.Warning($"minTick: {minTick} is less than ScopedSettings.MinTick.");
            }
        }

        public void ClampMaxAndWarn(ref int maxTick)
        {
            if (maxTick > MaxTick)
            {
                LogFactory.Warning($"maxTick: {maxTick} is bigger than ScopedSettings.MaxTick.");
            }
        }

        public void ClampAndWarn(ref int tick)
        {
            if (tick < MinTick)
            {
                LogFactory.Warning($"minTick: {tick} is less than ScopedSettings.MinTick.");
            }

            if (tick > MaxTick)
            {
                LogFactory.Warning($"maxTick: {tick} is bigger than ScopedSettings.MaxTick.");
            }

        }



    }
}