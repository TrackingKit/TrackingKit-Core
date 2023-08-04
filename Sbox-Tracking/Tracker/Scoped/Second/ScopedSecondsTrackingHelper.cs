using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    internal class ScopedSecondsTrackingHelper : ScopedTrackingHelper
    {
        private readonly ScopedSecondSettings Settings;

        internal ScopedSecondsTrackingHelper(TrackerStorage storageData, ScopedSecondSettings scopedSecondSettings) : base(storageData)
        {
            Settings = scopedSecondSettings;
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


        public bool TryGetTypedLatestValueAtOrPreviousSecond<T>(string propertyName, double minSecond, out double outputSecond, out T output, bool logError = false)
        {
            Settings.ClampMinAndWarn(ref minSecond);

            if (TryGetRawLatestValueAtOrPreviousTick(propertyName, out var outputTick, out var rawOutput, TimeUtility.SecondToTick(minSecond), Settings.MaxTick, Settings.Filter) && rawOutput.HasValue)
            {
                outputSecond = TimeUtility.TickToSecond(outputTick);

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
            outputSecond = default;

            return false;
        }

        public bool TryGetTypedLatestValueAtOrNextSecond<T>(string propertyName, double maxSecond, out double outputSecond, out T output, bool logError = false)
        {
            Settings?.ClampMaxAndWarn(ref maxSecond);


            if (TryGetRawLatestValueAtOrNextTick(propertyName, out var outputTick, out var rawOutput, Settings.MinTick, TimeUtility.SecondToTick(maxSecond), filter: Settings.Filter) && rawOutput.HasValue)
            {
                outputSecond = TimeUtility.TickToSecond(outputTick);

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

            outputSecond = default;
            output = default;
            return false;
        }

        public bool TryGetTypedLatestValueAtSecond<T>(string propertyName, double second, out T output, bool logError = false)
        {
            Settings?.ClampAndWarn(ref second);

            if (TryGetRawLatestValueAtTick(propertyName, TimeUtility.SecondToTick(second), out var rawOutput, filter: Settings.Filter) && rawOutput.HasValue)
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

        public bool TryGetTypedDetailedValueAtSecond<T>(string propertyName, double second, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {
            Settings.ClampAndWarn(ref second);

            var convertedSecondToTick = TimeUtility.SecondToTick(second);

            if (TryGetRawDetailedValueAtTick(propertyName, convertedSecondToTick, out var rawOutput, filter: Settings.Filter) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));
                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} at tick {convertedSecondToTick}. Returning default.");

            output = null;
            return false;
        }

        public bool TryGetTypedDetailedValuesAtOrNextSecond<T>(string propertyName, double minSecond, out double outputSecond, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {
            Settings.ClampMinAndWarn(ref minSecond);

            var convertedSecondToTick = TimeUtility.SecondToTick(minSecond);


            if (TryGetRawDetailedValuesAtOrNextTick(propertyName, out var outputTick, out var rawOutput, minTick: convertedSecondToTick, maxTick: Settings.MaxTick, filter: Settings.Filter) && rawOutput != null)
            {
                outputSecond = TimeUtility.TickToSecond(outputTick);

                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));

                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} between seconds {convertedSecondToTick} and {Settings.MaxSecond}. Returning default.");

            outputSecond = default;
            output = null;
            return false;
        }

        public bool TryGetTypedDetailedValuesAtOrPreviousSecond<T>(string propertyName, double maxSecond, out double outputSecond, out IEnumerable<(int Version, T Data)> output, bool logError = false)
        {
            Settings.ClampMinAndWarn(ref maxSecond);

            var convertedSecondToTick = TimeUtility.SecondToTick(maxSecond);


            if (TryGetRawDetailedValuesAtOrPreviousTick(propertyName, out var outputTick, out var rawOutput, maxTick: convertedSecondToTick, minTick: Settings.MinTick, filter: Settings.Filter) && rawOutput != null)
            {
                outputSecond = TimeUtility.TickToSecond(outputTick);

                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));

                return true;
            }

            if (logError) Log.Warning($"Can't find values for {propertyName} between ticks {Settings.MinSecond} and {convertedSecondToTick}. Returning default.");

            outputSecond = default;
            output = null;
            return false;
        }

        #endregion

    }
}
