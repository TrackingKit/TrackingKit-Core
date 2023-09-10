using Tracking.RulesService;
using Tracking;

namespace TrackingKit_Core
{
    [Obsolete("Not sure if useful")]
    public interface ITracker
    {
        /// <summary> Rules for Adding and Deleteing data. </summary>
        TrackerRulesService Rules { get; }

        GroupManager Groups { get; }
    }
}