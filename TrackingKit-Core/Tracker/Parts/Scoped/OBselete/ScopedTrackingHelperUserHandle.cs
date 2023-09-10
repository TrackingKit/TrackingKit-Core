using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackingKit_Core;
using TrackingKit_Core.TrackingKit_Core.Factories;

namespace Tracking
{
    internal class ScopedTrackingHelperUserHandle
    {
        private readonly ScopedSettings Settings;
        private readonly InMemoryTrackerStorage Storage;

        private UnitTime TimeType { get; }

        private ITimeUnit GenerateDefaultTimeUnit()
        {
            return TimeType switch
            {
                UnitTime.Tick => new TickTimeUnit(default),
                UnitTime.Second => new SecondTimeUnit(default),
                _ => throw new NotImplementedException()
            };
        }

        public enum UnitTime
        {
            Second,
            Tick
        }

        internal ScopedTrackingHelperUserHandle(ScopedSettings scopedTickSettings, InMemoryTrackerStorage storage, UnitTime timeType)
        {
            Settings = scopedTickSettings;
            Storage = storage;

            TimeType = timeType;
        }

        public bool PropertyExists(string propertyName, ITimeUnit? minTick = null, ITimeUnit? maxTick = null)
        {
            // TODO: mAKE SURE SAME UNIT TYPE.

            ITimeUnit finalMinTimeUnit = minTick ?? GenerateDefaultTimeUnit();
            ITimeUnit finalMaxTimeUnit = maxTick ?? GenerateDefaultTimeUnit();

            finalMaxTimeUnit.ConvertTo<SecondTimeUnit>();

            bool output;

            try
            {
                output = ScopedTrackingHelper.RawPropertyExists(Storage, propertyName, Settings.MinUnitTime.ConvertToTick(), Settings.MaxUnitTime.ConvertToTick(), SearchMode.AtOrPrevious, Settings.Filter);
            }
            catch(ArgumentException ex)
            {
                LogFactory.Warning($"User error input: {ex}");
            }

            catch(Exception ex)
            {
                output = false;

                LogFactory.Error($"Failed to find if value exists issue: {ex}");
            }

            return false;
        }

        

        public bool Exists()
        {
            return ScopedTrackingHelper.RawExists(Storage, Settings.MinTick, Settings.MaxTick, SearchMode.AtOrPrevious);
        }

        public int Count()
        {
            return ScopedTrackingHelper.RawCount(Storage, Settings.MinTick, Settings.MaxTick);
        }

        #region Typed TryGet Latest

        public bool TryGetTypedLatestValue<T>(string propertyName, SearchMode searchMode, out int outputTick, out T output, ITimeUnit? minTick = null, ITimeUnit? maxTick = null, bool logError = false)
        {
            ITimeUnit finalMinTimeUnit = minTick ?? new TickTimeUnit(Settings.MinTick);
            ITimeUnit finalMaxTimeUnit = maxTick ?? new TickTimeUnit(Settings.MaxTick);

            // TODO: Double check both time units are the same.


            if (ScopedTrackingHelper.TryGetRawLatestValue(Storage, propertyName, searchMode, out outputTick, out var rawOutput, finalMinTimeUnit.ConvertToTick(), finalMaxTimeUnit.ConvertToTick(), Settings.Filter) && rawOutput.HasValue)
            {
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
                SearchMode.At => $"Can't find value for {propertyName} at {finalMinTimeUnit.Name} {finalMinTimeUnit}. Returning default.",
                SearchMode.AtOrNext => $"Can't find value for {propertyName} between {finalMinTimeUnit.Name}s {finalMinTimeUnit} and {finalMaxTimeUnit}. Returning default.",
                SearchMode.AtOrPrevious => $"Can't find value for {propertyName} between {finalMinTimeUnit.Name}s {finalMinTimeUnit} and {finalMaxTimeUnit}. Returning default.",
                _ => "An unknown error occurred."
            };

            if (logError) LogFactory.Warning(errorMessage);

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
                LogFactory.Error($"Unexpected type. Expected {typeof(T)}, but found {data.GetType()}. Returning default.");
                return default;
            }
        }

        public bool TryGetTypedDetailedValues<T>(string propertyName, out int outputTick, out IEnumerable<(int Version, T Data)> output, SearchMode searchMode, int? minTick = null, int? maxTick = null, bool logError = false)
        {
            // Assign default values from Settings if not provided
            int finalMinTick = minTick ?? Settings.MinTick;
            int finalMaxTick = maxTick ?? Settings.MaxTick;

            if (ScopedTrackingHelper.TryGetRawDetailedValues(Storage,propertyName, searchMode, out outputTick, out var rawOutput, finalMinTick, finalMaxTick, Settings.Filter) && rawOutput != null)
            {
                output = rawOutput.Select(item => (item.Version, Data: ConvertData<T>(item.Data, logError)));
                return true;
            }

            string errorMessage = searchMode switch
            {
                SearchMode.At => $"Can't find values for {propertyName} at tick {finalMinTick}. Returning default.",
                SearchMode.AtOrNext => $"Can't find values for {propertyName} between ticks {finalMinTick} and {finalMaxTick}. Returning default.",
                SearchMode.AtOrPrevious => $"Can't find values for {propertyName} between ticks {finalMinTick} and {finalMaxTick}. Returning default.",
                _ => "An unknown error occurred."
            };

            if (logError) LogFactory.Warning(errorMessage);

            outputTick = 0;
            output = null;

            return false;
        }

        #endregion
    }
}
