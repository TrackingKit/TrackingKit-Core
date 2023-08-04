using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public partial class ScopedSecondSettings
    {
        public double MinSecond { get; }

        public double MaxSecond { get; }

        public bool IsSpecificSecond => MinSecond == MaxSecond;

        public int MinTick => TimeUtility.SecondToTick(MinSecond);

        public int MaxTick => TimeUtility.SecondToTick(MaxSecond);


        public TagFilter Filter { get; }

        public double SpecificSecond
        {
            get
            {
                if (!IsSpecificSecond)
                    Log.Error("Not specific second");

                return MaxSecond;
            }
        }

        public ScopedSecondSettings(float minSecond, float maxSecond, TagFilter filter)
        {
            MinSecond = minSecond;
            MaxSecond = maxSecond;
            Filter = filter;
        }

        public void ClampMinAndWarn(ref double minSecond)
        {
            if (minSecond < MinSecond)
            {
                Log.Warning($"minSecond: {minSecond} is less than ScopedSettings.MinSecond.");
                minSecond = MinSecond;
            }
        }

        public void ClampMaxAndWarn(ref double maxSecond)
        {
            if (maxSecond > MaxSecond)
            {
                Log.Warning($"maxSecond: {maxSecond} is bigger than ScopedSettings.MaxSecond.");
                maxSecond = MaxSecond;
            }
        }

        public void ClampAndWarn(ref double second)
        {
            if (second < MinSecond)
            {
                Log.Warning($"minSecond: {second} is less than ScopedSettings.MinSecond.");
                second = MinSecond;
            }

            if (second > MaxSecond)
            {
                Log.Warning($"maxSecond: {second} is bigger than ScopedSettings.MaxSecond.");
                second = MaxSecond;
            }

        }

    }
}
