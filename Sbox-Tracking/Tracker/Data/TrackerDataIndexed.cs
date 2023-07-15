using System.Collections.Generic;
using System.Linq;

public class TaggedData
{
    public object Data { get; }
    public string[] Tags { get; }

    public TaggedData(object data, string[] tags)
    {
        Data = data;
        Tags = tags;
    }
}

public class TrackerIndexedData
{
    private readonly Dictionary<string, SortedDictionary<int, SortedDictionary<int, TaggedData>>> data = new();

    public int GetLatestVersion(string propertyName, int tick)
    {
        if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
        {
            return versionDict.Keys.Any() ? versionDict.Keys.Max() : 0;
        }

        return 0;
    }

    public int Count => data.Values.Sum(tickDict => tickDict.Values.Sum(versionDict => versionDict.Count));

    public void SetValue(string propertyName, int tick, int version, object value, string[] tags)
    {
        if (!data.ContainsKey(propertyName))
        {
            data[propertyName] = new SortedDictionary<int, SortedDictionary<int, TaggedData>>();
        }

        var tickDict = data[propertyName];

        if (!tickDict.ContainsKey(tick))
        {
            tickDict[tick] = new SortedDictionary<int, TaggedData>();
        }

        var versionDict = tickDict[tick];
        versionDict[version] = new TaggedData(value, tags);
    }

    public void Remove(string propertyName, int tick, int version)
    {
        if (data.TryGetValue(propertyName, out var tickDict) && tickDict.TryGetValue(tick, out var versionDict))
        {
            versionDict.Remove(version);

            if (!versionDict.Any())
            {
                tickDict.Remove(tick);

                if (!tickDict.Any())
                {
                    data.Remove(propertyName);
                }
            }
        }
    }

    public IEnumerable<(int tick, int version, TaggedData taggedData)> GetValuesForPropertyBetweenTicks(string propertyName, int minTick, int maxTick)
    {
        if (data.TryGetValue(propertyName, out var tickDict))
        {
            var ticksInRange = tickDict.Keys.SkipWhile(t => t < minTick).TakeWhile(t => t <= maxTick);

            foreach (var tick in ticksInRange)
            {
                foreach (var version in tickDict[tick].Keys)
                {
                    var taggedData = tickDict[tick][version];
                    yield return (tick, version, taggedData);
                }
            }
        }
    }

}
