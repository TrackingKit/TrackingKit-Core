using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public partial class ScopedSecondSettings
    {
        public float MinSecond { get; }

        public float MaxSecond { get; }

        public bool IsSpecificSecond => MinSecond == MaxSecond;


        public string[] Tags { get; }

        public float SpecificTick
        {
            get
            {
                if (!IsSpecificSecond)
                    Log.Error("Not specific tick");

                return MaxSecond;
            }
        }

        public ScopedSecondSettings(float minSecond, float maxSecond, params string[] tags)
        {
            MinSecond = minSecond;
            MaxSecond = maxSecond;
            Tags = tags;
        }
    }
}
