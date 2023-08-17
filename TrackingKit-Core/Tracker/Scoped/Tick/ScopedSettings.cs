using Sandbox;
using TrackingKit_Core;
using TrackingKit_Core.TrackingKit_Core.Factories;

namespace Tracking
{

    public partial class ScopedSettings
    {
        public ITimeUnit MinUnitTime { get; }

        public ITimeUnit MaxUnitTime { get; }

        public bool IsSpecificUnitTime => MinUnitTime == MaxUnitTime;

        public IReadOnlyTagFilter Filter { get; }

        public ITimeUnit SpecificUnitTime
        {
            get
            {
                // TODO: Better error handling etc.
                if (!IsSpecificUnitTime)
                {
                    //LogFactory.Error("Not specific");
                }

                return default;
            }
        }

        public ScopedSettings(ITimeUnit minTimeUnit, ITimeUnit maxTimeUnit, TagFilter? filter = null)
        {
            if (minTimeUnit.CompareTo(maxTimeUnit) > 0)
            {
                LogFactory.Warning("Swapped MinTick and MaxTick as MinTick greater than MaxTick");
                MinUnitTime = maxTimeUnit;
                MaxUnitTime = minTimeUnit;
            }
            else
            {
                MinUnitTime = minTimeUnit;
                MaxUnitTime = maxTimeUnit;
            }

            Filter = filter ?? new TagFilter();
        }

    }
}