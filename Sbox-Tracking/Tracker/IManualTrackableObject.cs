namespace Tracking
{
    /// <summary> A object that has a tracker handled by itself. </summary>
    public interface IManualTrackableObject
    {
        ITracker Tracker { get; set; }
    }
}