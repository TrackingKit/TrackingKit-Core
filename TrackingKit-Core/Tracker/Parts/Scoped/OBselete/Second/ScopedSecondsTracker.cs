﻿using System;
using System.Collections.Generic;
using System.Linq;
using static Tracking.ScopedTrackingHelper;

namespace Tracking
{
    public partial class ScopedSecondsTracker : IDisposable
    {
        private ScopedSecondsTrackingHelper DataHelper { get; }

        internal ScopedSecondsTracker(InMemoryTrackerStorage data, ScopedSecondSettings scopedSettings)
        {
            DataHelper = new(data, scopedSettings);
        }


        public int Count()
            => DataHelper.Count();

        public bool Exists()
            => DataHelper.Exists();

        public bool Exists(string propertyName)
            => DataHelper.PropertyExists(propertyName);


        #region Get methods

        private T GetInternal<T>(string propertyName, double second, bool logError, T defaultValue = default)
        {
            if (DataHelper.TryGetTypedLatestValue<T>(propertyName, SearchMode.At, out _, out var result, minSecond: second, maxSecond: second, logError: logError))
            {
                return result;
            }

            return defaultValue;
        }

        public T Get<T>(string propertyName, double second)
            => GetInternal<T>(propertyName, second, logError: true);

        public T GetOrDefault<T>(string propertyName, double second, T defaultValue)
            => GetInternal<T>(propertyName, second, logError: false, defaultValue);

        #endregion

        #region GetOrPrevious methods

        private (double Second, T Data) GetOrPreviousInternal<T>(string propertyName, double second, bool logError, T defaultValue = default)
        {
            // Try to get the latest value before or at that second
            if (DataHelper.TryGetTypedLatestValue<T>(propertyName, SearchMode.AtOrPrevious, out var secondValue, out T value, maxSecond: second, logError: logError))
            {
                return (secondValue, value);
            }

            return (second, defaultValue);
        }

        public (double Second, T Data) GetOrPrevious<T>(string propertyName, double second)
            => GetOrPreviousInternal<T>(propertyName, second, true);

        public (double Second, T Data) GetOrPreviousOrDefault<T>(string propertyName, double second, T defaultValue)
            => GetOrPreviousInternal<T>(propertyName, second, false, defaultValue);

        #endregion

        #region GetOrNext methods

        private (double Second, T Data) GetOrNextInternal<T>(string propertyName, double second, bool logError, T defaultValue = default)
        {
            // Try to get the latest value before or at that second
            if (DataHelper.TryGetTypedLatestValue<T>(propertyName, SearchMode.AtOrNext, out var secondValue, out T value, minSecond: second, logError: logError))
            {
                return (secondValue, value);
            }

            return (second, defaultValue);
        }

        public (double Second, T Data) GetOrNext<T>(string propertyName, double second)
            => GetOrNextInternal<T>(propertyName, second, logError: true);

        public (double Second, T Data) GetOrNextOrDefault<T>(string propertyName, double second, T defaultValue)
            => GetOrNextInternal<T>(propertyName, second, logError: false, defaultValue);

        #endregion



        #region GetDetailed methods

        private IEnumerable<(int Version, T Value)> GetDetailedInternal<T>(string propertyName, double second, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out _, out var value, SearchMode.At, minSecond: second, maxSecond: second, logError: logError))
            {
                return value;
            }

            return defaultValue ?? Enumerable.Empty<(int Version, T Value)>();
        }




        public IEnumerable<(int Version, T Value)> GetDetailed<T>(string propertyName, double second)
            => GetDetailedInternal<T>(propertyName, second, logError: true);

        public IEnumerable<(int Version, T Value)> GetDetailedOrDefault<T>(string propertyName, double second, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedInternal<T>(propertyName, second, logError: false, defaultValue);


        #endregion

        #region GetDetailedOrPrevious methods

        private (double Second, IEnumerable<(int Version, T Value)> Data) GetDetailedOrPreviousInternal<T>(string propertyName, double second, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out var secondResult, out var value, SearchMode.AtOrPrevious, maxSecond: second, logError: logError))
            {
                return (secondResult, value);
            }

            return (second, defaultValue ?? Enumerable.Empty<(int Version, T Value)>());
        }



        public (double Second, IEnumerable<(int Version, T Value)> Data) GetDetailedOrPrevious<T>(string propertyName, double second)
            => GetDetailedOrPreviousInternal<T>(propertyName, second, logError: true);

        public (double Second, IEnumerable<(int Version, T Value)> Data) GetDetailedOrPreviousOrDefault<T>(string propertyName, double second, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedOrPreviousInternal<T>(propertyName, second, logError: false, defaultValue);

        #endregion


        #region GetDetailedOrNext methods

        private (double Second, IEnumerable<(int Version, T Value)> Data) GetDetailedOrNextInternal<T>(string propertyName, double second, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out var secondResult, out var value, SearchMode.AtOrNext, minSecond: second, logError: logError))
            {
                return (secondResult, value);
            }

            return (second, defaultValue ?? Enumerable.Empty<(int Version, T Value)>());
        }

        public (double Second, IEnumerable<(int Version, T Value)> Data) GetDetailedOrNext<T>(string propertyName, double second)
            => GetDetailedOrNextInternal<T>(propertyName, second, logError: true);

        public (double Second, IEnumerable<(int Version, T Value)> Data) GetDetailedOrNextOrDefault<T>(string propertyName, double second, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedOrNextInternal<T>(propertyName, second, logError: false, defaultValue);

        #endregion


        public void Dispose()
        {
            // Implementation of dispose if necessary
        }
    }
}
