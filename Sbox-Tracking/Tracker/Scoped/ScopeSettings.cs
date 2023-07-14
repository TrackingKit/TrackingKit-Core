using System;

namespace Tracking
{
    public class ScopeSettings
    {
        public int MinTick { get; init; }

        public int MaxTick { get; init; }

        public bool IsSpecificTick => MinTick == MaxTick;

        public int SpecificTick
        {
            get
            {
                if (!IsSpecificTick)
                    throw new Exception("Not a specific tick");

                return MinTick;
            }
        }

        /// <summary> Any tags we should filter out. </summary>
        public string[] Tags { get; init; }

    }
}
