namespace Tracking
{
    internal readonly struct TrackingLatestKey
    {
        public string PropertyName { get; }

        public int Tick { get; }
        public double Second => TimeUtility.TickToSecond(Tick);


        public TrackingLatestKey(string propertyName, int tick)
        {
            PropertyName = string.Intern(propertyName);
            Tick = tick;
        }
    }


}
