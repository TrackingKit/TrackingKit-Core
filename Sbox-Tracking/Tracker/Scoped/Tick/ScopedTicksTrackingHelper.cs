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

        public bool TryGetTypedLatestValue<T>(string propertyName, SearchMode searchMode, out int outputTick, out T output, int? minTick = null, int? maxTick = null, bool logError = false)
        {
            // Assign default values from Settings if not provided
            int finalMinTick = minTick ?? Settings.MinTick;
            int finalMaxTick = maxTick ?? Settings.MaxTick;

            switch (searchMode)
            {
                case SearchMode.At:
                    if (finalMinTick != finalMaxTick)
                    {
                        Log.Warning($"In 'At' mode, minTick and maxTick should be equal. Setting maxTick to {finalMinTick}.");
                        finalMaxTick = finalMinTick; // In case of AtTick, maxTick should be same as minTick
                    }
                    Settings.ClampAndWarn(ref finalMinTick);
                    Settings.ClampAndWarn(ref finalMaxTick);
                    break;
                case SearchMode.AtOrNext:
                    Settings.ClampMinAndWarn(ref finalMinTick);
                    break;
                case SearchMode.AtOrPrevious:
                    Settings.ClampMaxAndWarn(ref finalMaxTick);
                    break;
            }

            if (TryGetRawLatestValue(propertyName, out outputTick, out var rawOutput, finalMinTick, finalMaxTick, Settings.Filter, searchMode) && rawOutput.HasValue)
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

            string errorMessage = searchMode switch
            {
                SearchMode.At => $"Can't find value for {propertyName} at tick {finalMinTick}. Returning default.",
                SearchMode.AtOrNext => $"Can't find value for {propertyName} between ticks {finalMinTick} and {finalMaxTick}. Returning default.",
                SearchMode.AtOrPrevious => $"Can't find value for {propertyName} between ticks {finalMinTick} and {finalMaxTick}. Returning default.",
                _ => "An unknown error occurred."
            };

            if (logError) Log.Warning(errorMessage);


            outputTick = 0;
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

        public bool TryGetTypedDetailedValues<T>(string propertyName, out int outputTick, out IEnumerable<(int Version, T Data)> output, SearchMode searchMode, int? minTick = null, int? maxTick = null, bool logError = false)
        {
            // Assign default values from Settings if not provided
            int finalMinTick = minTick ?? Settings.MinTick;
            int finalMaxTick = maxTick ?? Settings.MaxTick;

            switch (searchMode)
            {
                case SearchMode.At:
                    Settings.ClampAndWarn(ref finalMinTick);
                    if (finalMinTick != finalMaxTick)
                    {
                        Log.Warning("System issue: In 'At' mode, minTick and maxTick should be equal. Setting maxTick to minTick.");
                        finalMaxTick = finalMinTick; // In case of AtTick, maxTick should be same as minTick
                    }
                    break;
                case SearchMode.AtOrNext:
                    Settings.ClampMinAndWarn(ref finalMinTick);
                    break;
                case SearchMode.AtOrPrevious:
                    Settings.ClampMaxAndWarn(ref finalMaxTick);
                    break;
            }

            if (TryGetRawDetailedValues(propertyName, finalMinTick, finalMaxTick, out outputTick, out var rawOutput, Settings.Filter, searchMode) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data.Data, logError)));
                return true;
            }

            string errorMessage = searchMode switch
            {
                SearchMode.At => $"Can't find values for {propertyName} at tick {finalMinTick}. Returning default.",
                SearchMode.AtOrNext => $"Can't find values for {propertyName} between ticks {finalMinTick} and {finalMaxTick}. Returning default.",
                SearchMode.AtOrPrevious => $"Can't find values for {propertyName} between ticks {finalMinTick} and {finalMaxTick}. Returning default.",
                _ => "An unknown error occurred."
            };

            if (logError) Log.Warning(errorMessage);

            outputTick = 0;
            output = null;

            return false;
        }



        #endregion

    }
}
