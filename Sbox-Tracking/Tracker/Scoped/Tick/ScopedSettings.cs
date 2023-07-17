namespace Tracking
{
    public partial class ScopedTickSettings
    {
        public int MinTick { get; }

        public int MaxTick { get; }

        public bool IsSpecificTick => MinTick == MaxTick;


        public string[] Tags { get; }

        public int SpecificTick
        {
            get
            {
                if (!IsSpecificTick)
                    Log.Error("Not specific tick");

                return MaxTick;
            }
        }

        public ScopedTickSettings(int minTick, int maxTick, params string[] tags)
        {
            MinTick = minTick;
            MaxTick = maxTick;
            Tags = tags;
        }
    }
}