using System;
using System.Collections.Generic;
using System.Linq;
using static Tracking.ScopedTrackingHelper;

namespace Tracking
{
    /// <summary>
    /// ScopedTicksTracker operates within a defined scope of ticks (timepoints) in a dataset.
    /// It provides methods to retrieve data based on specific tick values, focusing primarily
    /// on the latest version of data within each tick. This simplifies data access patterns when
    /// only the most recent versions of data in each tick are of interest.
    /// It includes various retrieval methods such as Get, GetOrNext, and GetOrPrevious. Each method
    /// is designed to handle data in the context of the latest version within the tick range.
    /// </summary>
    public partial class ScopedTicksTracker : IDisposable
    {
        private ScopedTicksTrackingHelper DataHelper { get; }

        internal ScopedTicksTracker(TrackerStorage data, ScopedTickSettings scopedSettings)
        {
            DataHelper = new(data, scopedSettings);

        }

        public int Count()
            => DataHelper.Count();


        // Yes?
        //public int Count(string propertyName)
        //    => DataHelper.PropertyCount(propertyName);

        public bool Exists()
            => DataHelper.Exists();

        public bool Exists(string propertyName)
            => DataHelper.PropertyExists(propertyName);



        #region Get methods

        private T GetInternal<T>(string propertyName, int tick, bool logError, T defaultValue = default)
        {
            if (DataHelper.TryGetTypedLatestValueAtTick<T>(propertyName, tick, out var result, logError: logError))
            {
                return result;
            }

            return defaultValue;
        }


        public T Get<T>(string propertyName, int tick)
            => GetInternal<T>(propertyName, tick, logError: true);

        public T GetOrDefault<T>(string propertyName, int tick, T defaultValue)
            => GetInternal<T>(propertyName, tick, logError: false, defaultValue);


        #endregion

        #region GetOrPrevious methods

        private (int Tick, T Data) GetOrPreviousInternal<T>(string propertyName, int tick, bool logError, T defaultValue = default)
        {
            // Try to get the latest value before or at that tick
            if (DataHelper.TryGetTypedLatestValueAtOrPreviousTick(propertyName, tick, out var resultTick, out T value, logError: logError))
            {
                return (resultTick,value);
            }

            return (tick, defaultValue);
        }




        public (int Tick, T Data) GetOrPrevious<T>(string propertyName, int tick)
            => GetOrPreviousInternal<T>(propertyName, tick, true);

        public (int Tick, T Data) GetOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
            => GetOrPreviousInternal<T>(propertyName, tick, false, defaultValue);

        #endregion

        #region GetOrNext methods

        private (int Tick, T Data) GetOrNextInternal<T>(string propertyName, int tick, bool logError, T defaultValue = default)
        {
            // Try to get the latest value before or at that tick
            if (DataHelper.TryGetTypedLatestValueAtOrNextTick(propertyName, tick, out var resultTick, out T value, logError: logError))
            {
                return (resultTick, value);
            }

            return (tick, defaultValue);
        }




        public (int Tick, T Data) GetOrNext<T>(string propertyName, int tick)
            => GetOrNextInternal<T>(propertyName, tick, logError: true);

        public (int Tick, T Data) GetOrNextOrDefault<T>(string propertyName, int tick, T defaultValue)
            => GetOrNextInternal<T>(propertyName, tick, logError: false, defaultValue);

        #endregion


        #region GetDetailed methods

        private IEnumerable<(int Version, T Value)> GetDetailedInternal<T>(string propertyName, int tick, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out _, out var value, TickSearchMode.AtTick, minTick: tick, maxTick: tick, logError: logError))
            {
                return value;
            }

            return defaultValue ?? Enumerable.Empty<(int Version, T Value)>();
        }



        public IEnumerable<(int Version, T Value)> GetDetailed<T>(string propertyName, int tick)
            => GetDetailedInternal<T>(propertyName, tick, logError: true);

        public IEnumerable<(int Version, T Value)> GetDetailedOrDefault<T>(string propertyName, int tick, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedInternal<T>(propertyName, tick, logError: false, defaultValue);


        #endregion

        #region GetDetailedOrPrevious methods

        private (int Tick, IEnumerable<(int Version, T Value)> Data) GetDetailedOrPreviousInternal<T>(string propertyName, int tick, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out var tickResult, out var value, TickSearchMode.AtOrPreviousTick, maxTick: tick, logError: logError))
            {
                return (tickResult, value);
            }

            return (tick, defaultValue ?? Enumerable.Empty<(int Version, T Value)>());
        }



        public (int Tick, IEnumerable<(int Version, T Value)> Data) GetDetailedOrPrevious<T>(string propertyName, int tick)
            => GetDetailedOrPreviousInternal<T>(propertyName, tick, logError: true);

        public (int Tick, IEnumerable<(int Version, T Value)> Data) GetDetailedOrPreviousOrDefault<T>(string propertyName, int tick, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedOrPreviousInternal<T>(propertyName, tick, logError: false, defaultValue);

        #endregion


        #region GetDetailedOrNext methods

        private (int Tick, IEnumerable<(int Version, T Value)> Data) GetDetailedOrNextInternal<T>(string propertyName, int tick, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out var tickResult, out var value, TickSearchMode.AtOrNextTick, minTick: tick, logError: logError))
            {
                return (tickResult, value);
            }

            return (tick, defaultValue ?? Enumerable.Empty<(int Version, T Value)>());
        }

        public (int Tick, IEnumerable<(int Version, T Value)> Data) GetDetailedOrNext<T>(string propertyName, int tick)
            => GetDetailedOrNextInternal<T>(propertyName, tick, logError: true);

        public (int Tick, IEnumerable<(int Version, T Value)> Data) GetDetailedOrNextOrDefault<T>(string propertyName, int tick, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedOrNextInternal<T>(propertyName, tick, logError: false, defaultValue);

        #endregion



        public void Dispose()
        {

        }
    }
}
