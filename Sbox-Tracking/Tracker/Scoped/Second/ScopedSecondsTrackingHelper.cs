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

        public bool TryGetTypedDetailedValues<T>(string propertyName, out double outputSecond, out IEnumerable<(int Version, T Data)> output, TickSearchMode searchMode, double? minSecond = null, double? maxSecond = null, bool logError = false)
        {
            outputSecond = 0; // Initialize to default

            double finalMinSecond = minSecond.HasValue ? minSecond.Value : TimeUtility.TickToSecond(Settings.MinTick);
            double finalMaxSecond = maxSecond.HasValue ? maxSecond.Value : TimeUtility.TickToSecond(Settings.MaxTick);

            switch (searchMode)
            {
                case TickSearchMode.AtTick:
                    if (finalMinSecond != finalMaxSecond)
                    {
                        Log.Warning($"In 'AtTick' mode, minSecond and maxSecond should be equal. Setting maxSecond to {finalMinSecond}.");
                        finalMaxSecond = finalMinSecond; // In case of AtTick, maxSecond should be same as minSecond
                    }
                    if (minSecond.HasValue) Settings.ClampAndWarn(ref finalMinSecond);
                    if (maxSecond.HasValue) Settings.ClampAndWarn(ref finalMaxSecond);
                    break;
                case TickSearchMode.AtOrNextTick:
                    if (minSecond.HasValue) Settings.ClampMinAndWarn(ref finalMinSecond);
                    break;
                case TickSearchMode.AtOrPreviousTick:
                    if (maxSecond.HasValue) Settings.ClampMaxAndWarn(ref finalMaxSecond);
                    break;
            }

            int minTick = TimeUtility.SecondToTick(finalMinSecond);
            int maxTick = TimeUtility.SecondToTick(finalMaxSecond);

            if (TryGetRawDetailedValues(propertyName, minTick, maxTick, out int outputTick, out var rawOutput, Settings.Filter, searchMode) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));
                outputSecond = TimeUtility.TickToSecond(outputTick);
                return true;
            }

            string errorMessage = searchMode switch
            {
                TickSearchMode.AtTick => $"Can't find values for {propertyName} at second {finalMinSecond}. Returning default.",
                TickSearchMode.AtOrNextTick => $"Can't find values for {propertyName} between seconds {finalMinSecond} and {finalMaxSecond}. Returning default.",
                TickSearchMode.AtOrPreviousTick => $"Can't find values for {propertyName} between seconds {finalMinSecond} and {finalMaxSecond}. Returning default.",
                _ => "An unknown error occurred."
            };

            if (logError) Log.Warning(errorMessage);

            output = null;
            return false;
        }




        #endregion

    }
}
