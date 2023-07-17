using Sandbox;
using System.Collections.Generic;
using System;
using System.Linq;

public static class TimeUtility
{
    private static SortedDictionary<float, int> Seconds { get; set; } = new SortedDictionary<float, int>();

    public static int SecondToTick(float second)
    {
        // Check if the exact second value exists
        if (Seconds.TryGetValue(second, out int exactTick))
        {
            return exactTick;
        }

        // Get two closest seconds
        var lowerSeconds = Seconds.Where(pair => pair.Key < second);
        var upperSeconds = Seconds.Where(pair => pair.Key > second);

        if (!lowerSeconds.Any() || !upperSeconds.Any())
        {
            throw new Exception("Not enough data to interpolate tick.");
        }

        var lowerClosestSecond = lowerSeconds.Aggregate((x, y) => Math.Abs(x.Key - second) < Math.Abs(y.Key - second) ? x : y);
        var upperClosestSecond = upperSeconds.Aggregate((x, y) => Math.Abs(x.Key - second) < Math.Abs(y.Key - second) ? x : y);

        // Interpolate between these two closest seconds
        float tickSpan = upperClosestSecond.Value - lowerClosestSecond.Value;
        float secondSpan = upperClosestSecond.Key - lowerClosestSecond.Key;

        float ratio = (second - lowerClosestSecond.Key) / secondSpan;
        return (int)Math.Round(lowerClosestSecond.Value + tickSpan * ratio);
    }

    [GameEvent.Tick]
    private static void Tick()
    {
        Seconds.Add(Time.Now, Time.Tick);
    }
}
