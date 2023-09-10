using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingKit_Core.TrackingKit_Core.Factories;

namespace Tracking
{
    internal class ScopedSecondsTrackingHelper : ScopedTrackingHelper
    {
        private readonly ScopedSecondSettings Settings;

        private readonly ScopedTrackingHelperUserHandle Helper;

        internal ScopedSecondsTrackingHelper(InMemoryTrackerStorage storageData, ScopedSecondSettings scopedSecondSettings) : base(storageData)
        {
            Settings = scopedSecondSettings;

            Helper = new(storageData);
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


        public bool TryGetTypedLatestValue<T>(string propertyName, SearchMode searchMode, out double outputSecond, out T output, double? minSecond = null, double? maxSecond = null, bool logError = false)
        {

            double finalMinSecond = minSecond.HasValue ? minSecond.Value : TimeUtility.TickToSecond(Settings.MinTick);
            double finalMaxSecond = maxSecond.HasValue ? maxSecond.Value : TimeUtility.TickToSecond(Settings.MaxTick);

            switch (searchMode)
            {
                case SearchMode.At:
                    if (minSecond.HasValue) Settings.ClampAndWarn(ref finalMinSecond);
                    if (maxSecond.HasValue) Settings.ClampAndWarn(ref finalMaxSecond);
                    break;
                case SearchMode.AtOrNext:
                    if (minSecond.HasValue) Settings.ClampMinAndWarn(ref finalMinSecond);
                    break;
                case SearchMode.AtOrPrevious:
                    if (maxSecond.HasValue) Settings.ClampMaxAndWarn(ref finalMaxSecond);
                    break;
            }

            int minTick = TimeUtility.SecondToTick(finalMinSecond);
            int maxTick = TimeUtility.SecondToTick(finalMaxSecond);

            if (TryGetRawLatestValue(propertyName, out int outputTick, out var rawOutput, minTick, maxTick, null, searchMode) && rawOutput.HasValue)
            {
                outputSecond = TimeUtility.TickToSecond(outputTick);

                if (rawOutput.Value.Data is T typedValue)
                {
                    output = typedValue;
                    return true;
                }
                else
                {
                    LogFactory.Error($"Unexpected type for {propertyName}. Expected {typeof(T)}, but found {rawOutput.Value.Data.GetType()}. Returning default.");
                }
            }

            string errorMessage = searchMode switch
            {
                SearchMode.At => $"Can't find value for {propertyName} at second {finalMinSecond}. Returning default.",
                SearchMode.AtOrNext => $"Can't find value for {propertyName} at or after second {finalMinSecond}. Returning default.",
                SearchMode.AtOrPrevious => $"Can't find value for {propertyName} at or before second {finalMaxSecond}. Returning default.",
                _ => "An unknown error occurred."
            };

            if (logError) LogFactory.Warning(errorMessage);

            outputSecond = 0;
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
                LogFactory.Error($"Unexpected type. Expected {typeof(T)}, but found {data.GetType()}. Returning default.");
                return default;
            }
        }

        public bool TryGetTypedDetailedValues<T>(string propertyName, out double outputSecond, out IEnumerable<(int Version, T Data)> output, SearchMode searchMode, double? minSecond = null, double? maxSecond = null, bool logError = false)
        {

            double finalMinSecond = minSecond.HasValue ? minSecond.Value : TimeUtility.TickToSecond(Settings.MinTick);
            double finalMaxSecond = maxSecond.HasValue ? maxSecond.Value : TimeUtility.TickToSecond(Settings.MaxTick);

            switch (searchMode)
            {
                case SearchMode.At:
                    if (finalMinSecond != finalMaxSecond)
                    {
                        LogFactory.Warning($"In 'At' mode, minSecond and maxSecond should be equal. Setting maxSecond to {finalMinSecond}.");
                        finalMaxSecond = finalMinSecond; // In case of AtTick, maxSecond should be same as minSecond
                    }
                    if (minSecond.HasValue) Settings.ClampAndWarn(ref finalMinSecond);
                    if (maxSecond.HasValue) Settings.ClampAndWarn(ref finalMaxSecond);
                    break;
                case SearchMode.AtOrNext:
                    if (minSecond.HasValue) Settings.ClampMinAndWarn(ref finalMinSecond);
                    break;
                case SearchMode.AtOrPrevious:
                    if (maxSecond.HasValue) Settings.ClampMaxAndWarn(ref finalMaxSecond);
                    break;
            }

            int minTick = TimeUtility.SecondToTick(finalMinSecond);
            int maxTick = TimeUtility.SecondToTick(finalMaxSecond);

            if (TryGetRawDetailedValues(propertyName, minTick, maxTick, out int outputTick, out var rawOutput, Settings.Filter, searchMode) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data, logError)));
                outputSecond = TimeUtility.TickToSecond(outputTick);
                return true;
            }

            string errorMessage = searchMode switch
            {
                SearchMode.At => $"Can't find values for {propertyName} at second {finalMinSecond}. Returning default.",
                SearchMode.AtOrNext => $"Can't find values for {propertyName} between seconds {finalMinSecond} and {finalMaxSecond}. Returning default.",
                SearchMode.AtOrPrevious => $"Can't find values for {propertyName} between seconds {finalMinSecond} and {finalMaxSecond}. Returning default.",
                _ => "An unknown error occurred."
            };

            if (logError) LogFactory.Warning(errorMessage);

            outputSecond = 0;
            output = null;

            return false;
        }




        #endregion

    }
}
