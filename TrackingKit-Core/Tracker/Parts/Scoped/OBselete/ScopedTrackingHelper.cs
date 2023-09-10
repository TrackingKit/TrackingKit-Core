using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    /// <summary>
    /// <para>
    /// Provides utility methods for scoped tracking operations on a given TrackerStorage instance. 
    /// This helper facilitates operations such as checking the existence of properties, counting properties, 
    /// and retrieving raw or detailed values based on specific search modes and filtering criteria.
    /// </para>
    /// 
    /// <para>
    /// The methods are separated from the TrackerStorage to maintain a clear separation of concerns, 
    /// ensuring that the TrackerStorage remains focused on core storage operations while this helper 
    /// handles more specialized tracking queries and operations.
    /// </para>
    /// 
    /// </summary>
    internal static class ScopedTrackingHelper
    {
        private static void CheckPropertyGeneralArguments(string propertyName, int minTick, int maxTick)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Cannot be null or empty for propertyname");
            }
        }

        private static void CheckPropertyGeneralArguments(string propertyName, int minTick, int maxTick, SearchMode mode)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Cannot be null or empty for propertyname");
            }

            if(minTick != maxTick && mode == SearchMode.At)
            {
                throw new ArgumentException("Cannot be SearchedMode.At when minTick != maxTick");
            }
        }

        private static void CheckGeneralArguments(int minTick, int maxTick, SearchMode mode)
        {
            if (minTick != maxTick && mode == SearchMode.At)
            {
                throw new ArgumentException("Cannot be SearchedMode.At when minTick != maxTick");
            }
        }



        #region Exists Property

        public static bool RawPropertyExists(InMemoryTrackerStorage storage, string propertyName, int minTick, int maxTick, SearchMode searchMode, IReadOnlyTagFilter filter)
        {
            CheckPropertyGeneralArguments(propertyName, minTick, maxTick, searchMode);

            bool ShouldInclude(TaggedData data)
            {
                return filter.ShouldIncludes(data.Tags);
            }

            return RawPropertyExistsInternal(storage, propertyName, minTick, maxTick, searchMode, ShouldInclude);
        }

        public static bool RawPropertyExists(InMemoryTrackerStorage storage, string propertyName, int minTick, int maxTick, SearchMode searchMode)
        {
            CheckPropertyGeneralArguments(propertyName, minTick, maxTick, searchMode);

            // Always include when no filter is provided
            bool ShouldIncludeAlways(TaggedData data) => true;

            return RawPropertyExistsInternal(storage, propertyName, minTick, maxTick, searchMode, ShouldIncludeAlways);
        }

        private static bool RawPropertyExistsInternal(InMemoryTrackerStorage storage, string propertyName, int minTick, int maxTick, SearchMode searchMode, Func<TaggedData, bool> shouldIncludePredicate)
        {
            var ticks = storage.GetTickKeys(propertyName).Where(t => t >= minTick && t <= maxTick).OrderBy(t => t).ToList();
            if (!ticks.Any()) return false;

            switch (searchMode)
            {
                case SearchMode.At:
                    if (storage.ContainsTick(propertyName, minTick))
                    {
                        var versions = storage.GetVersionKeys(propertyName, minTick);
                        foreach (var version in versions)
                        {
                            if (shouldIncludePredicate(storage[propertyName, minTick, version]))
                                return true;
                        }
                    }
                    break;
                case SearchMode.AtOrNext:
                    foreach (var nextTick in ticks)
                    {
                        var versions = storage.GetVersionKeys(propertyName, nextTick);
                        foreach (var version in versions)
                        {
                            if (shouldIncludePredicate(storage[propertyName, nextTick, version]))
                                return true;
                        }
                    }
                    break;
                case SearchMode.AtOrPrevious:
                    for (int i = ticks.Count - 1; i >= 0; i--)
                    {
                        var prevTick = ticks[i];
                        var versions = storage.GetVersionKeys(propertyName, prevTick);
                        foreach (var version in versions)
                        {
                            if (shouldIncludePredicate(storage[propertyName, prevTick, version]))
                                return true;
                        }
                    }
                    break;
            }

            return false;
        }

        #endregion

        #region Exists

        public static bool RawExists(InMemoryTrackerStorage storage, int minTick, int maxTick, SearchMode searchMode, IReadOnlyTagFilter filter)
        {
            CheckGeneralArguments(minTick, maxTick, searchMode);

            bool ShouldInclude(TaggedData data)
            {
                return filter.ShouldIncludes(data.Tags);
            }

            return RawExistsInternal(storage, minTick, maxTick, searchMode, ShouldInclude);
        }

        public static bool RawExists(InMemoryTrackerStorage storage, int minTick, int maxTick, SearchMode searchMode)
        {
            CheckGeneralArguments(minTick, maxTick, searchMode);

            // Always include when no filter is provided
            bool ShouldIncludeAlways(TaggedData data) => true;

            return RawExistsInternal(storage, minTick, maxTick, searchMode, ShouldIncludeAlways);
        }

        private static bool RawExistsInternal(InMemoryTrackerStorage storage, int minTick, int maxTick, SearchMode searchMode, Func<TaggedData, bool> shouldIncludePredicate)
        {
            var allPropertyNames = storage.GetPropertyNamesKeys();

            foreach (var propertyName in allPropertyNames)
            {
                if (RawPropertyExistsInternal(storage, propertyName, minTick, maxTick, searchMode, shouldIncludePredicate))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region PropertyCount

        public static int RawPropertyCount(InMemoryTrackerStorage storage, string propertyName, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            CheckPropertyGeneralArguments(propertyName, minTick, maxTick);

            bool ShouldInclude(TaggedData data)
            {
                return filter.ShouldIncludes(data.Tags);
            }

            return RawPropertyCountInternal(storage, propertyName, minTick, maxTick, ShouldInclude);
        }

        public static int RawPropertyCount(InMemoryTrackerStorage storage, string propertyName, int minTick, int maxTick)
        {
            CheckPropertyGeneralArguments(propertyName, minTick, maxTick);

            // Always include when no filter is provided
            bool ShouldIncludeAlways(TaggedData data) => true;

            return RawPropertyCountInternal(storage, propertyName, minTick, maxTick, ShouldIncludeAlways);
        }

        private static int RawPropertyCountInternal(InMemoryTrackerStorage storage, string propertyName, int minTick, int maxTick, Func<TaggedData, bool> shouldIncludePredicate)
        {
            int count = 0;
            var ticks = storage.GetTickKeys(propertyName).Where(t => t >= minTick && t <= maxTick).OrderBy(t => t).ToList();
            if (!ticks.Any()) return 0;

            foreach (var tick in ticks)
            {
                var versions = storage.GetVersionKeys(propertyName, tick);
                foreach (var version in versions)
                {
                    if (shouldIncludePredicate(storage[propertyName, tick, version]))
                        count++;
                }
            }

            return count;
        }

        #endregion

        #region Count

        public static int RawCount(InMemoryTrackerStorage storage, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            bool ShouldInclude(TaggedData data)
            {
                return filter.ShouldIncludes(data.Tags);
            }

            return RawCountInternal(storage, minTick, maxTick, ShouldInclude);
        }

        public static int RawCount(InMemoryTrackerStorage storage, int minTick, int maxTick)
        {
            // Always include when no filter is provided
            bool ShouldIncludeAlways(TaggedData data) => true;

            return RawCountInternal(storage, minTick, maxTick, ShouldIncludeAlways);
        }

        private static int RawCountInternal(InMemoryTrackerStorage storage, int minTick, int maxTick, Func<TaggedData, bool> shouldIncludePredicate)
        {
            int totalCount = 0;
            var allPropertyNames = storage.GetPropertyNamesKeys();

            foreach (var propertyName in allPropertyNames)
            {
                totalCount += RawPropertyCountInternal(storage, propertyName, minTick, maxTick, shouldIncludePredicate);
            }

            return totalCount;
        }

        #endregion



        #region Raw TryGet Latest

        public static bool TryGetRawLatestValue(InMemoryTrackerStorage storage, string propertyName, SearchMode searchMode, out int foundTick, out (int Version, TaggedData Data)? output, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            CheckPropertyGeneralArguments(propertyName, minTick, maxTick, searchMode);

            output = null;
            foundTick = 0;

            bool ShouldInclude(TaggedData data)
            {
                if (filter == null) return true;
                return filter.ShouldIncludes(data.Tags);
            }

            if (searchMode == SearchMode.At)
            {
                if (storage.ContainsProperty(propertyName) && storage.ContainsTick(propertyName, minTick))
                {
                    if (storage.TryGetLatestVersion(propertyName, minTick, out int latestVersion) &&
                        storage.TryGetValue(propertyName, minTick, latestVersion, out TaggedData data) &&
                        ShouldInclude(data))
                    {
                        output = (latestVersion, data);
                        foundTick = minTick;
                        return true;
                    }
                }
            }
            else
            {
                var ticks = storage.GetTickKeys(propertyName).Where(t => t >= minTick && t <= maxTick).OrderBy(t => t).ToList();
                if (!ticks.Any()) return false;

                if (searchMode == SearchMode.AtOrNext)
                {
                    foreach (var nextTick in ticks)
                    {
                        if (storage.TryGetLatestVersion(propertyName, nextTick, out int latestVersion) &&
                            storage.TryGetValue(propertyName, nextTick, latestVersion, out TaggedData data) &&
                            ShouldInclude(data))
                        {
                            output = (latestVersion, data);
                            foundTick = nextTick;
                            return true;
                        }
                    }
                }
                else if (searchMode == SearchMode.AtOrPrevious)
                {
                    for (int i = ticks.Count - 1; i >= 0; i--)
                    {
                        var prevTick = ticks[i];
                        if (storage.TryGetLatestVersion(propertyName, prevTick, out int latestVersion) &&
                            storage.TryGetValue(propertyName, prevTick, latestVersion, out TaggedData data) &&
                            ShouldInclude(data))
                        {
                            output = (latestVersion, data);
                            foundTick = prevTick;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Raw TryGet Detailed

        public static bool TryGetRawDetailedValues(InMemoryTrackerStorage storage, string propertyName, SearchMode searchMode, out int foundTick, out IEnumerable<(int Version, TaggedData Data)> output, int minTick, int maxTick, IReadOnlyTagFilter filter)
        {
            CheckPropertyGeneralArguments(propertyName, minTick, maxTick, searchMode);


            List<(int Version, TaggedData Data)> result = new List<(int Version, TaggedData Data)>();
            foundTick = 0;

            bool ShouldInclude(TaggedData data)
            {
                if (filter == null) return true;
                return filter.ShouldIncludes(data.Tags);
            }

            if (searchMode == SearchMode.At)
            {
                if (storage.ContainsProperty(propertyName) && storage.ContainsTick(propertyName, minTick))
                {
                    foreach (var version in storage.GetVersionKeys(propertyName, minTick))
                    {
                        var data = storage[propertyName, minTick, version];
                        if (ShouldInclude(data))
                        {
                            result.Add((version, data));
                        }
                    }
                    if (result.Count > 0)
                    {
                        output = result;
                        foundTick = minTick;
                        return true;
                    }
                }
            }
            else
            {
                var ticks = storage.GetTickKeys(propertyName).Where(t => t >= minTick && t <= maxTick).OrderBy(t => t).ToList();
                if (!ticks.Any())
                {
                    output = null;
                    return false;
                }

                if (searchMode == SearchMode.AtOrNext)
                {
                    foreach (var nextTick in ticks)
                    {
                        foreach (var version in storage.GetVersionKeys(propertyName, nextTick))
                        {
                            var data = storage[propertyName, nextTick, version];
                            if (ShouldInclude(data))
                            {
                                result.Add((version, data));
                            }
                        }
                        if (result.Count > 0)
                        {
                            output = result;
                            foundTick = nextTick;
                            return true;
                        }
                    }
                }
                else if (searchMode == SearchMode.AtOrPrevious)
                {
                    for (int i = ticks.Count - 1; i >= 0; i--)
                    {
                        var prevTick = ticks[i];
                        foreach (var version in storage.GetVersionKeys(propertyName, prevTick))
                        {
                            var data = storage[propertyName, prevTick, version];
                            if (ShouldInclude(data))
                            {
                                result.Add((version, data));
                            }
                        }
                        if (result.Count > 0)
                        {
                            output = result;
                            foundTick = prevTick;
                            return true;
                        }
                    }
                }
            }

            output = null;
            return false;
        }

        #endregion


    }

    public enum SearchMode
    {
        At,
        AtOrNext,
        AtOrPrevious
    }
}
