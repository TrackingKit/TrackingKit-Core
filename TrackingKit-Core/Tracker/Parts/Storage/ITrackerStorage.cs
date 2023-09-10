namespace Tracking
{
    public interface ITimeUnit
    {

    }

    public interface ITrackerStorage<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary> Gets the number of entries in the storage. </summary>
        int Count { get; }

        /// <summary> Retrieves all property names from the storage. </summary>
        IEnumerable<TKey> Properties { get; }

        bool TryGetTicks(TKey propertyName, out IEnumerable<ITimeUnit> timeUnits);

        /// <summary>
        /// Tries to retrieve a value associated with a property name, time unit, and version.
        /// </summary>
        bool TryGetValue<T>(TKey propertyName, ITimeUnit timeUnit, int version, out TaggedData<T> value)
            where T : notnull;

        /// <summary>
        /// Tries to retrieve all versions associated with a property name and time unit.
        /// </summary>
        bool TryGetVersions(TKey propertyName, ITimeUnit timeUnit, out IEnumerable<int> versions);

        /// <summary>
        /// Tries to retrieve the latest version for a property name and time unit.
        /// </summary>
        bool TryGetLatestVersion(TKey propertyName, ITimeUnit timeUnit, out int latestVersion);

        /// <summary> Adds a new value associated with a property name and time unit. </summary>
        bool AddValue<T>(TKey propertyName, ITimeUnit timeUnit, T value)
            where T : notnull;

        /// <summary> Adds a new value associated with a property name, time unit, and a specific version. </summary>
        bool AddValue<T>(TKey propertyName, ITimeUnit timeUnit, int version, T value)
            where T : notnull;

        /// <summary> Adds a new value associated with a property name, time unit, and tags. </summary>
        bool AddValueWithTags<T>(TKey propertyName, ITimeUnit timeUnit, T value, IReadOnlyCollection<string> tags)
            where T : notnull;

        /// <summary> Adds a new value associated with a property name, time unit, a specific version, and tags. </summary>
        bool AddValueWithTags<T>(TKey propertyName, ITimeUnit timeUnit, int version, T value, IReadOnlyCollection<string> tags)
            where T : notnull;

        /// <summary> Updates a value associated with a property name, time unit, and version. </summary>
        bool UpdateValue<T>(TKey propertyName, ITimeUnit timeUnit, int version, T value)
            where T : notnull;

        /// <summary> Updates the tags associated with a property name, time unit, and version. </summary>
        bool UpdateTags(TKey propertyName, ITimeUnit timeUnit, int version, IReadOnlyCollection<string> tags);

        /// <summary> Removes a value associated with a property name, time unit, and version. </summary>
        bool RemoveValue(TKey propertyName, ITimeUnit timeUnit, int version);

        /// <summary> Removes all values associated with a property name and time unit, regardless of version. </summary>
        bool RemoveValuesForTick(TKey propertyName, ITimeUnit timeUnit);
    }




}
