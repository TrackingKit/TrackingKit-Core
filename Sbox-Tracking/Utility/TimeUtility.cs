using Sandbox;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Tracking
{
    public static class TimeUtility
    {
        private static SortedDictionary<double, int> Seconds { get; set; } = new SortedDictionary<double, int>();

        public static int SecondToTick(double second)
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

            var lowerClosestSecond = lowerSeconds.Last();
            var upperClosestSecond = upperSeconds.First();

            // Calculate the difference in ticks between the two closest seconds. This gives us the number of ticks 
            // that occur in the span between these two seconds.
            double tickSpan = upperClosestSecond.Value - lowerClosestSecond.Value;

            // Calculate the difference in seconds between the two closest seconds.
            double secondSpan = upperClosestSecond.Key - lowerClosestSecond.Key;


            // Calculate the ratio of how far the input second is between the two closest seconds.
            double ratio = (second - lowerClosestSecond.Key) / secondSpan;


            // Use this ratio to interpolate the corresponding tick value.
            return (int)Math.Round(lowerClosestSecond.Value + tickSpan * ratio);
        }

        [GameEvent.Tick]
        private static void Tick()
        {
            double currentTime = Time.Now;
            if (Seconds.ContainsKey(currentTime))
            {
                Seconds[currentTime] = Time.Tick;
            }
            else
            {
                Seconds.Add(currentTime, Time.Tick);
            }
        }
    }
}
