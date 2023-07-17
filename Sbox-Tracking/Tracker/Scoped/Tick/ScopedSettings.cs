namespace Tracking
{
    public partial class ScopedSettings
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

        public ScopedSettings(int minTick, int maxTick, params string[] tags)
        {
            MinTick = minTick;
            MaxTick = maxTick;
            Tags = tags;
        }
    }
}