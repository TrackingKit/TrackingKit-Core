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

public class TrackerQueryData
{
    public string PropertyName { get; set; }
    public int Tick { get; set; }
    public int Version { get; set; }
    public TaggedData TaggedData { get; set; }
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

    // TODO: Organise to correct way.
    // As it make sense to do tick first in here as tick is highest order.
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

    public bool Exists(int tick)
    {
        return data.Values.Any(tickDict => tickDict.ContainsKey(tick));
    }

    public bool Exists(int tick, string propertyName)
    {
        return data.TryGetValue(propertyName, out var tickDict) && tickDict.ContainsKey(tick);
    }

    public bool Exists(int tick, string propertyName, int version)
    {
        return data.TryGetValue(propertyName, out var tickDict)
               && tickDict.TryGetValue(tick, out var versionDict)
               && versionDict.ContainsKey(version);
    }

    public void Remove(int tick)
    {
        foreach (var tickDict in data.Values)
        {
            tickDict.Remove(tick);
        }
    }

    public void Remove(int tick, string propertyName)
    {
        if (data.TryGetValue(propertyName, out var tickDict))
        {
            tickDict.Remove(tick);
        }
    }

    public void Remove(int tick, string propertyName, int version)
    {
        if (data.TryGetValue(propertyName, out var tickDict)
            && tickDict.TryGetValue(tick, out var versionDict))
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

    public IEnumerable<TrackerQueryData> GetValuesBetweenTicks(int minTick, int maxTick)
    {
        foreach (var propertyPair in data)
        {
            var propertyName = propertyPair.Key;
            var tickDict = propertyPair.Value;

            var ticksInRange = tickDict.Keys.SkipWhile(t => t < minTick).TakeWhile(t => t <= maxTick);

            foreach (var tick in ticksInRange)
            {
                foreach (var version in tickDict[tick].Keys)
                {
                    var taggedData = tickDict[tick][version];
                    yield return new TrackerQueryData
                    {
                        PropertyName = propertyName,
                        Tick = tick,
                        Version = version,
                        TaggedData = taggedData
                    };
                }
            }
        }
    }


    public IEnumerable<TrackerQueryData> GetValuesAtTick(int tick)
    {
        foreach (var propertyPair in data)
        {
            var propertyName = propertyPair.Key;
            var tickDict = propertyPair.Value;

            if (tickDict.TryGetValue(tick, out var versionDict))
            {
                foreach (var version in versionDict.Keys)
                {
                    var taggedData = versionDict[version];
                    yield return new TrackerQueryData
                    {
                        PropertyName = propertyName,
                        Tick = tick,
                        Version = version,
                        TaggedData = taggedData
                    };
                }
            }
        }
    }




}
