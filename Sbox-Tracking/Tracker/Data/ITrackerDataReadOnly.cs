using System.Collections.Generic;

namespace Tracking
{
    public interface ITrackerDataReadOnly
    {
        int Count { get; }

        bool GetKeyExists(string propertyName);


        (int minTick, int maxTick) RecordedRange { get; }

        int GetLatestVersion(string propertyName, int tick);

        IEnumerable<KeyValuePair<TrackerKey, object>> GetValues();

        IEnumerable<KeyValuePair<TrackerKey, object>> Get(string propertyName, ScopeSettings settings);

        IEnumerable<string> Keys { get; }

    }
}