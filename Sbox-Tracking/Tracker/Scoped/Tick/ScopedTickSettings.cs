using Sandbox;

namespace Tracking
{
    public interface IScopedTickSettings
    {
        int MinTick { get; }

        int MaxTick { get; }

        IReadOnlyTagFilter Filter { get; }

        void ClampAndWarn(ref int tick);
    }

    public partial class ScopedTickSettings : IScopedTickSettings
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
                    Log.Error("Not specific tick");
                    return -1; // return an invalid value or throw an exception
                }

                return MaxTick;
            }
        }

        public ScopedTickSettings(int minTick, int maxTick, TagFilter filter = default)
        {
            if(minTick > maxTick)
            {
                Log.Warning("Swapped MinTick and MaxTick as MinTick greater than MaxTick");
                MinTick = maxTick;
                MaxTick = minTick;
            }
            else
            {
                MinTick = minTick;
                MaxTick = maxTick;
                Filter = filter;
            }


        }

        public void ClampAndWarn(ref int tick)
        {
            if (tick < MinTick)
            {
                Log.Warning($"Tick {tick} is less than ScopedSettings.MinTick. Clamping to ScopedSettings.MinTick.");
                tick = MinTick;
            }
            else if (tick > MaxTick)
            {
                Log.Warning($"Tick {tick} is greater than ScopedSettings.MaxTick. Clamping to ScopedSettings.MaxTick.");
                tick = MaxTick;
            }
        }

        public int ClampAndWarn(int tick)
        {
            if (tick < MinTick)
            {
                Log.Warning($"Tick {tick} is less than ScopedSettings.MinTick. Clamping to ScopedSettings.MinTick.");
                return MinTick;
            }
            else if (tick > MaxTick)
            {
                Log.Warning($"Tick {tick} is greater than ScopedSettings.MaxTick. Clamping to ScopedSettings.MaxTick.");
                return MaxTick;
            }

            return tick;
        }


    }
}