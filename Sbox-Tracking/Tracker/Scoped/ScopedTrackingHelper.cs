using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    internal class ScopedTrackingHelper : IValid
    {
        public bool IsValid => Storage == null;


        private readonly TrackerStorage Storage;

        private readonly IScopedTickSettings Settings;


        internal ScopedTrackingHelper(TrackerStorage storageData)
        {
            Storage = storageData;
        }


        internal ScopedTrackingHelper(TrackerStorage storageData, IScopedTickSettings scopedTickSettings)
        {
            Storage = storageData;
            Settings = scopedTickSettings;

        }






        // TODO: Some count cache mechanism?

        #region Count and RawCount

        protected int RawCount(int minTick, int maxTick)
        {
            
            if(Storage.TryGetValue(out var result, minTick, maxTick))
            {
                // Assuming you want to count the number of TrackerMetaData objects
                int sum = 0;
                foreach (var property in result)
                {
                    foreach (var tick in property.Value)
                    {
                        sum += tick.Value.Count; // Add the count of TrackerMetaData for this tick
                    }
                }

                return sum;
            }

            return 0;
        }

        public int Count()
        {
            return RawCount(Settings.MinTick, Settings.MaxTick);
        }

        #endregion

        #region Exists

        protected bool RawPropertyExists(string propertyName, int minTick, int maxTick)
        {
            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0;

            return Storage.TryGetPropertyValue(propertyName, out var result, minTick: minTick, maxTick: maxTick, stopCondition: stopCondition);
        }

        protected bool RawExists(int minTick, int maxTick)
        {
            Func<StopPropertyConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0;

            return Storage.TryGetValue(out var result, minTick: minTick, maxTick: maxTick, stopCondition: stopCondition);
        }

        public bool PropertyExists(string propertyName)
        {
            return RawPropertyExists(propertyName, Settings.MinTick, Settings.MaxTick);
        }

        public bool Exists()
        {
            return RawExists(Settings.MinTick, Settings.MaxTick);
        }


        #endregion





        #region Raw TryGet Latest


        // TODO Count one? check hash if need to do again etc

        protected bool TryGetRawLatestValueAtTick(string propertyName, ref int tick, out (int Version, TrackerMetaData Data)? output)
        {
            Settings?.ClampAndWarn(ref tick);


            if (Storage.TryGetPropertyValue(
                propertyName, out var value, minTick: tick, maxTick: tick,
                shouldInclude: x => (Settings?.Filter?.ShouldIncludes(x.Tags) ?? true)))
            {
                if (value.TryGetValue(tick, out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    // Get the highest key value directly
                    var highestKey = trackerMetaData.Keys.Max();
                    var correspondingValue = trackerMetaData[highestKey];

                    // Assuming you want to use highestKey as Version
                    output = (highestKey, correspondingValue);
                    return true;
                }
            }

            output = null;
            return false;
        }




        protected bool TryGetRawLatestValueAtNextTick(string propertyName, out int outputTick, out (int Version, TrackerMetaData Data)? output, int minTick, int maxTick)
        {
            outputTick = 0; // Initialize outputTick

            int? lastTick = null;

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = Settings?.Filter?.ShouldIncludes(x.Tags) ?? true;
                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition))
            {
                if (value.Keys.Count() == 1 && value.TryGetValue(value.Keys.First(), out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    var highestKey = trackerMetaData.Keys.Max();
                    var correspondingValue = trackerMetaData[highestKey];

                    output = (highestKey, correspondingValue);
                    outputTick = value.Keys.First(); // Set the outputTick value
                    return true;
                }
            }

            output = null;
            return false;
        }


        protected bool TryGetRawLatestValueAtPreviousTick(string propertyName, out int outputTick, out (int Version, TrackerMetaData Data)? output, int minTick, int maxTick)
        {
            outputTick = 0; // Initialize outputTick
            int? lastTick = null;

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = Settings?.Filter?.ShouldIncludes(x.Tags) ?? true;
                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition, ascending: false))
            {
                if (value.Keys.Count() == 1 && value.TryGetValue(value.Keys.First(), out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    var highestKey = trackerMetaData.Keys.Max();
                    var correspondingValue = trackerMetaData[highestKey];

                    output = (highestKey, correspondingValue);
                    outputTick = value.Keys.First(); // Set the outputTick value
                    return true;
                }
            }

            output = null;
            return false;
        }





        #endregion

        #region Raw TryGet Detailed

        protected bool TryGetRawDetailedValueAtTick(string propertyName, int tick, out IEnumerable<(int Version, TrackerMetaData Data)> output)
        {
            if (Storage.TryGetPropertyValue(
                propertyName, out var value, minTick: tick, maxTick: tick,
                shouldInclude: x => (Settings?.Filter?.ShouldIncludes(x.Tags) ?? true)))
            {
                if (value.TryGetValue(tick, out var trackerMetaData) && trackerMetaData.Count > 0)
                {
                    output = trackerMetaData.Select(kvp => (Version: kvp.Key, Data: kvp.Value)).ToList();
                    return true;
                }
            }

            output = null;
            return false;
        }

        protected bool TryGetRawDetailedValuesAtNextTick(string propertyName, out int outputTick, out IEnumerable<(int Version, TrackerMetaData Data)> output, int minTick, int maxTick)
        {
            outputTick = 0; // Initialize outputTick
            int? lastTick = null;
            List<(int Version, TrackerMetaData Data)> results = new List<(int Version, TrackerMetaData Data)>();

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = Settings?.Filter?.ShouldIncludes(x.Tags) ?? true;

                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition))
            {
                var firstTickData = value.FirstOrDefault();
                if (firstTickData.Value != null)
                {
                    results.AddRange(firstTickData.Value.Select(kvp => (Version: kvp.Key, Data: kvp.Value)));
                    outputTick = firstTickData.Key; // Set the outputTick value
                }
            }

            if (results.Any())
            {
                output = results;
                return true;
            }

            output = null;
            return false;
        }

        protected bool TryGetRawDetailedValuesAtPreviousTick(string propertyName, out int outputTick, out IEnumerable<(int Version, TrackerMetaData Data)> output, int minTick, int maxTick)
        {
            outputTick = 0; // Initialize outputTick
            int? lastTick = null;
            List<(int Version, TrackerMetaData Data)> results = new List<(int Version, TrackerMetaData Data)>();

            Func<StopConditionInformation, bool> stopCondition = info => info.AccumulatedSoFar > 0 && (lastTick.HasValue && lastTick.Value != info.Tick);

            Func<TrackerInformation, bool> shouldInclude = x =>
            {
                if (lastTick.HasValue && lastTick.Value != x.Tick)
                {
                    return false;
                }

                bool result = Settings?.Filter?.ShouldIncludes(x.Tags) ?? true;
                if (result)
                {
                    lastTick = x.Tick;
                }
                return result;
            };

            if (Storage.TryGetPropertyValue(propertyName, out var value, minTick: minTick, maxTick: maxTick, shouldInclude: shouldInclude, stopCondition: stopCondition, ascending: false))
            {
                var firstTickData = value.FirstOrDefault();
                if (firstTickData.Value != null)
                {
                    results.AddRange(firstTickData.Value.Select(kvp => (Version: kvp.Key, Data: kvp.Value)));
                    outputTick = firstTickData.Key; // Set the outputTick value
                }
            }

            if (results.Any())
            {
                output = results;
                return true;
            }

            output = null;
            return false;
        }



        #endregion


        #region Typed TryGet Latest

        private int HandleTickBound(int? tick, int? defaultTick, string tickName, bool logError)
        {
            if (!tick.HasValue)
            {
                int resolvedTick = defaultTick ?? (tickName == "minTick" ? int.MinValue : int.MaxValue);

                if (defaultTick.HasValue && logError)
                {
                    Log.Warning($"{tickName} is not specified. Using the scoped setting's value of {resolvedTick}.");
                }

                return resolvedTick;
            }

            return tick.Value;
        }



        public bool TryGetTypedLatestValueAtPreviousTick<T>(string propertyName, int minTick, out int outputTick, out T output, bool logError = false)
        {

            if (TryGetRawLatestValueAtPreviousTick(propertyName, out outputTick, out var rawOutput, minTick, Settings.MaxTick) && rawOutput.HasValue)
            {
                if (rawOutput.Value.Data.Data is T typedValue)
                {
                    output = typedValue;
                    return true;
                }
                else
                {
                    Log.Error($"Unexpected type for {propertyName}. Expected {typeof(T)}, but found {rawOutput.Value.Data.Data.GetType()}. Returning default.");
                }
            }

            if (logError) Log.Warning($"Can't find value for {propertyName}. Returning default.");
            output = default;
            return false;
        }

        public bool TryGetTypedLatestValueAtNextTick<T>(string propertyName, int maxTick, out int outputTick, out T output, bool logError = false)
        {

            if (TryGetRawLatestValueAtNextTick(propertyName, out outputTick, out var rawOutput, Settings.MinTick, maxTick) && rawOutput.HasValue)
            {
                if (rawOutput.Value.Data.Data is T typedValue)
                {
                    output = typedValue;
                    return true;
                }
                else
                {
                    Log.Error($"Unexpected type for {propertyName}. Expected {typeof(T)}, but found {rawOutput.Value.Data.Data.GetType()}. Returning default.");
                }
            }

            if (logError) Log.Warning($"Can't find value for {propertyName}. Returning default.");

            output = default;
            return false;
        }

        public bool TryGetTypedLatestValueAtTick<T>(string propertyName, ref int tick, out T output, bool logError = false)
        {
            Settings?.ClampAndWarn(ref tick);

            if (TryGetRawLatestValueAtTick(propertyName, ref tick, out var rawOutput) && rawOutput.HasValue)
            {
                if (rawOutput.Value.Data.Data is T typedValue)
                {
                    output = typedValue;
                    return true;
                }
                else
                {
                    Log.Error($"Unexpected type for {propertyName}. Expected {typeof(T)}, but found {rawOutput.Value.Data.Data.GetType()}. Returning default.");
                }
            }

            if (logError) Log.Warning($"Can't find value for {propertyName}. Returning default.");
            output = default;
            return false;
        }

        #endregion

        #region Typed TryGet Detailed

        private T ConvertData<T>(object data, bool logError = false)
        {
            if (data is T typedValue)
            {
                return typedValue;
            }
            else
            {
                Log.Error($"Unexpected type. Expected {typeof(T)}, but found {data.GetType()}. Returning default.");
                return default;
            }
        }

        public bool TryGetTypedDetailedValueAtTick<T>(string propertyName, int tick, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {
            Settings?.ClampAndWarn(ref tick);


            if (TryGetRawDetailedValueAtTick(propertyName, tick, out var rawOutput) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));
                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} at tick {tick}. Returning default.");
            output = null;
            return false;
        }

        public bool TryGetTypedDetailedValuesAtNextTick<T>(string propertyName, int minTick, out int outputTick, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {

            if (TryGetRawDetailedValuesAtNextTick(propertyName, out outputTick, out var rawOutput, minTick: minTick, maxTick: Settings.MaxTick) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));

                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} between ticks {minTick} and {Settings.MaxTick}. Returning default.");

            output = null;
            return false;
        }

        public bool TryGetTypedDetailedValuesAtPreviousTick<T>(string propertyName, int maxTick, out int outputTick, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {

            if (TryGetRawDetailedValuesAtPreviousTick(propertyName, out outputTick, out var rawOutput, maxTick: maxTick, minTick: Settings.MinTick) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));
                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} between ticks {Settings.MinTick} and {maxTick}. Returning default.");
            output = null;
            return false;
        }

        #endregion


    }
}
