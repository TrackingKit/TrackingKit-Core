using Sandbox;
using System.Collections.Generic;
using System;

namespace Tracking
{
    public static class TimeUtility
    {
        private static List<KeyValuePair<double, int>> Seconds { get; set; } = new List<KeyValuePair<double, int>>();

        public static double MinSecondRecorded { get; private set; } = double.MaxValue;
        public static double MaxSecondRecorded { get; private set; } = double.MinValue;

        private static readonly int MAX_RECORDS = 1000;
        private static readonly int DISPERSION_INTERVAL_RECENT_25 = 1;
        private static readonly int DISPERSION_INTERVAL_NEXT_25 = 5;
        private static readonly int DISPERSION_INTERVAL_NEXT_25_AGAIN = 15; 
        private static readonly int DISPERSION_INTERVAL_OLDEST_25 = 30; 

        public static double TickToSecond(int tick)
        {
            if(tick >= Seconds[Seconds.Count - 1].Value)
            {
                return Time.Now;
            }

            CheckBoundsForTick(tick, out int index, out var lowerTicks, out var upperTicks);

            return Interpolate(lowerTicks.Key, upperTicks.Key, lowerTicks.Value, upperTicks.Value, tick);
        }

        public static int SecondToTick(double second)
        {
            if(second >= Seconds[Seconds.Count - 1].Key)
            {
                return Time.Tick;
            }

            CheckBoundsForSecond(second, out int index, out var lowerClosestSecond, out var upperClosestSecond);

            return (int)Math.Round(Interpolate(lowerClosestSecond.Value, upperClosestSecond.Value, lowerClosestSecond.Key, upperClosestSecond.Key, second));
        }

        [GameEvent.Tick]
        private static void Tick()
        {
            double currentTime = Time.Now;
            Seconds.Add(new KeyValuePair<double, int>(currentTime, Time.Tick));

            if (Seconds.Count > MAX_RECORDS)
            {
                //DisperseOlderData();
            }

            MinSecondRecorded = Math.Min(MinSecondRecorded, currentTime);
            MaxSecondRecorded = Math.Max(MaxSecondRecorded, currentTime);
        }

        private static readonly bool DEBUG = true;

        private static void DisperseOlderData()
        {
            int dispersionIndex = Seconds.Count / 2;
            List<KeyValuePair<double, int>> newSeconds = new List<KeyValuePair<double, int>>();

            int[] intervalsCount = new int[4]; // To store counts of each dispersion interval

            int recent25Count = dispersionIndex / 4;
            int next25Count = dispersionIndex / 4;
            int next25AgainCount = dispersionIndex / 4;
            int oldest25Count = dispersionIndex - recent25Count - next25Count - next25AgainCount;

            intervalsCount[0] = recent25Count;
            intervalsCount[1] = next25Count;
            intervalsCount[2] = next25AgainCount;
            intervalsCount[3] = oldest25Count;

            int currentInterval = DISPERSION_INTERVAL_RECENT_25;

            for (int i = 0; i < dispersionIndex; i += currentInterval)
            {
                double aggregateTime = 0;
                int aggregateTicks = 0;
                int count = 0;

                // Determine the dispersion interval based on the current index
                if (i < dispersionIndex / 4) currentInterval = DISPERSION_INTERVAL_RECENT_25;
                else if (i < dispersionIndex / 2) currentInterval = DISPERSION_INTERVAL_NEXT_25;
                else if (i < 3 * dispersionIndex / 4) currentInterval = DISPERSION_INTERVAL_NEXT_25_AGAIN;
                else currentInterval = DISPERSION_INTERVAL_OLDEST_25;

                for (int j = 0; j < currentInterval && (i + j) < dispersionIndex; j++)
                {
                    aggregateTime += Seconds[i + j].Key;
                    aggregateTicks += Seconds[i + j].Value;
                    count++;
                }

                if (count > 0)
                {
                    newSeconds.Add(new KeyValuePair<double, int>(aggregateTime / count, aggregateTicks / count));
                }
            }

            // Append the rest of the records
            for (int i = dispersionIndex; i < Seconds.Count; i++)
            {
                newSeconds.Add(Seconds[i]);
            }

            int recordsRemoved = Seconds.Count - newSeconds.Count;
            int totalCount = Seconds.Count;

            Seconds = newSeconds;

            if (DEBUG)
            {
                int originalValue = Seconds.Count + recordsRemoved;

                Log.Info("Dispersion Report:");
                Log.Info($"- Original Value: {originalValue} records");
                Log.Info($"- Records Removed: {recordsRemoved} records ({GetPercentage(recordsRemoved, originalValue)} of original storage)");
                Log.Info($"- Records Retained: {Seconds.Count} records ({GetPercentage(Seconds.Count, originalValue)} of original storage)");
                Log.Info($"- Total Records after Cleanup: {Seconds.Count} records");

                Log.Info($"- Most recent 25%: {recent25Count} records ({GetPercentage(recent25Count, totalCount)} of current storage)");
                Log.Info($"- Next 25%: {next25Count} records ({GetPercentage(next25Count, totalCount)} of current storage)");
                Log.Info($"- Next 25% again: {next25AgainCount} records ({GetPercentage(next25AgainCount, totalCount)} of current storage)");
                Log.Info($"- Oldest 25%: {oldest25Count} records ({GetPercentage(oldest25Count, totalCount)} of current storage)");
            }
        }



        private static string GetPercentage(int count, int totalCount)
        {
            double percentage = (double)count / totalCount * 100;
            return $"{Math.Round(percentage, 2)}%";
        }







        private static void CheckBoundsForTick(int tick, out int index, out KeyValuePair<double, int> lower, out KeyValuePair<double, int> upper)
        {
            index = CustomBinarySearch(Seconds, tick);
            if (index < 0) index = ~index - 1;
            lower = Seconds[index];
            upper = Seconds[index + 1];
        }

        private static void CheckBoundsForSecond(double second, out int index, out KeyValuePair<double, int> lower, out KeyValuePair<double, int> upper)
        {
            index = CustomBinarySearchBySecond(Seconds, second);
            if (index < 0) index = ~index - 1;
            if (index >= Seconds.Count - 1) index = Seconds.Count - 2;
            lower = Seconds[index];
            upper = Seconds[index + 1];
        }

        private static double Interpolate(double lowerValue, double upperValue, double lowerBound, double upperBound, double target)
        {
            double spanValue = upperValue - lowerValue;
            double spanBound = upperBound - lowerBound;

            if (spanBound == 0)
            {
                return lowerValue;
            }

            double ratio = (target - lowerBound) / spanBound;
            return lowerValue + spanValue * ratio;
        }

        private static int CustomBinarySearchBySecond(List<KeyValuePair<double, int>> list, double second)
        {
            int low = 0;
            int high = list.Count - 1;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (list[mid].Key < second)
                    low = mid + 1;
                else if (list[mid].Key > second)
                    high = mid - 1;
                else
                    return mid;
            }

            return ~low;
        }

        private static int CustomBinarySearch(List<KeyValuePair<double, int>> list, int tick)
        {
            int low = 0;
            int high = list.Count - 1;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (list[mid].Value < tick)
                    low = mid + 1;
                else if (list[mid].Value > tick)
                    high = mid - 1;
                else
                    return mid;
            }

            return ~low;
        }
    }
}
