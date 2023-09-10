using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingKit_Core.TrackingKit_Core.Factories;

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
                    LogFactory.Error("Not specific second");

                return MaxSecond;
            }
        }

        public ScopedSecondSettings(float minSecond, float maxSecond, TagFilter filter)
        {
            if (maxSecond > TimeUtility.MaxSecondRecorded)
            {
                maxSecond = (float)TimeUtility.MaxSecondRecorded;
                LogFactory.Warning($"maxSecond was greater than the maximum recorded second, clamped to {maxSecond}.");
            }

            if (minSecond < TimeUtility.MinSecondRecorded)
            {
                minSecond = (float)TimeUtility.MinSecondRecorded;
                LogFactory.Warning($"minSecond was less than the minimum recorded second, clamped to {minSecond}.");
            }

            
            MinSecond = minSecond;
            MaxSecond = maxSecond;
            Filter = filter;
        }

        public void ClampMinAndWarn(ref double minSecond)
        {
            if (minSecond < MinSecond)
            {
                LogFactory.Warning($"minSecond: {minSecond} is less than ScopedSettings.MinSecond.");
                minSecond = MinSecond;
            }
        }

        public void ClampMaxAndWarn(ref double maxSecond)
        {
            if (maxSecond > MaxSecond)
            {
                LogFactory.Warning($"maxSecond: {maxSecond} is bigger than ScopedSettings.MaxSecond.");
                maxSecond = MaxSecond;
            }
        }

        public void ClampAndWarn(ref double second)
        {
            if (second < MinSecond)
            {
                LogFactory.Warning($"minSecond: {second} is less than ScopedSettings.MinSecond.");
                second = MinSecond;
            }

            if (second > MaxSecond)
            {
                LogFactory.Warning($"maxSecond: {second} is bigger than ScopedSettings.MaxSecond.");
                second = MaxSecond;
            }

        }

    }
}
