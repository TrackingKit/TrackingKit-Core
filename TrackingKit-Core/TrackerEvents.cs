using System;

namespace Tracking
{
    public class TrackerEventArgs : EventArgs
    {
        public DynamicTracker Tracker { get; }

        public TrackerEventArgs(DynamicTracker tracker)
        {
            Tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        }
    }

    public static class TrackerEvents
    {
        // Event declaration
        public static event EventHandler<TrackerEventArgs>? Added;
        public static event EventHandler<TrackerEventArgs>? Removed;

        // Methods to raise events
        public static void OnAdded(DynamicTracker tracker)
        {
            Added?.Invoke(null, new TrackerEventArgs(tracker));
        }

        public static void OnRemoved(DynamicTracker tracker)
        {
            Removed?.Invoke(null, new TrackerEventArgs(tracker));
        }
    }
}
