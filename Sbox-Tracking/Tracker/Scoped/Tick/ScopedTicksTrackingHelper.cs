using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    internal class ScopedTicksTrackingHelper : ScopedTrackingHelper
    {
        private readonly ScopedTickSettings Settings;


        internal ScopedTicksTrackingHelper(TrackerStorage storageData, ScopedTickSettings scopedTickSettings)
            : base(storageData)
        {
            Settings = scopedTickSettings;
        }


        public bool PropertyExists(string propertyName)
        {
            return RawPropertyExists(propertyName, Settings.MinTick, Settings.MaxTick);
        }

        public bool Exists()
        {
            return RawExists(Settings.MinTick, Settings.MaxTick);
        }


        public int Count()
        {
            return RawCount(Settings.MinTick, Settings.MaxTick);
        }


        #region Typed TryGet Latest


        public bool TryGetTypedLatestValueAtOrPreviousTick<T>(string propertyName, int minTick, out int outputTick, out T output, bool logError = false)
        {
            Settings.ClampMinAndWarn(ref minTick);

            if (TryGetRawLatestValueAtOrPreviousTick(propertyName, out outputTick, out var rawOutput, minTick, Settings.MaxTick, Settings.Filter) && rawOutput.HasValue)
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

        public bool TryGetTypedLatestValueAtOrNextTick<T>(string propertyName, int maxTick, out int outputTick, out T output, bool logError = false)
        {
            Settings?.ClampMaxAndWarn(ref maxTick);


            if (TryGetRawLatestValueAtOrNextTick(propertyName, out outputTick, out var rawOutput, Settings.MinTick, maxTick, filter: Settings.Filter) && rawOutput.HasValue)
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

        public bool TryGetTypedLatestValueAtTick<T>(string propertyName, int tick, out T output, bool logError = false)
        {
            var targetTick = tick;

            Settings?.ClampAndWarn(ref targetTick);

            if (TryGetRawLatestValueAtTick(propertyName, targetTick, out var rawOutput, filter: Settings.Filter) && rawOutput.HasValue)
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
            Settings.ClampAndWarn(ref tick);


            if (TryGetRawDetailedValueAtTick(propertyName, tick, out var rawOutput, filter: Settings.Filter) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));
                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} at tick {tick}. Returning default.");

            output = null;
            return false;
        }

        public bool TryGetTypedDetailedValuesAtOrNextTick<T>(string propertyName, int minTick, out int outputTick, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {
            Settings.ClampMinAndWarn(ref minTick);

            if (TryGetRawDetailedValuesAtOrNextTick(propertyName, out outputTick, out var rawOutput, minTick: minTick, maxTick: Settings.MaxTick, filter: Settings.Filter) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));

                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} between ticks {minTick} and {Settings.MaxTick}. Returning default.");

            output = null;
            return false;
        }

        public bool TryGetTypedDetailedValuesAtOrPreviousTick<T>(string propertyName, int maxTick, out int outputTick, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {
            Settings.ClampMaxAndWarn(ref maxTick);

            if (TryGetRawDetailedValuesAtOrPreviousTick(propertyName, out outputTick, out var rawOutput, maxTick: maxTick, minTick: Settings.MinTick, filter: Settings.Filter) && rawOutput != null)
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
