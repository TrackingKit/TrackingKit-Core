namespace Tracking
{
    public interface ITrackableObject
    {
        ITracker Tracker { get; set; }
    }
}