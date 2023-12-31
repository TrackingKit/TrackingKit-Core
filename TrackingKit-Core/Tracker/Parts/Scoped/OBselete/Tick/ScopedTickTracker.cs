﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;
using static Tracking.ScopedTrackingHelper;

namespace Tracking
{
    public sealed class ScopedTickTracker : IDisposable
    {
        private readonly ScopedTrackingHelperUserHandle DataHelper;

        private readonly int targetTick;

        internal ScopedTickTracker(InMemoryTrackerStorage data, ScopedSettings scopedSettings)
        {
            targetTick = scopedSettings.MinTick;

            DataHelper = new(data, scopedSettings);
        }


        public int Count()
            => DataHelper.Count();

        public bool Exists(string propertyName)
            => DataHelper.Exists(propertyName);

        public bool Exists()
            => DataHelper.Exists();

        private T GetInternal<T>(string propertyName, int tick, bool logError, T defaultValue = default)
        {
            if (DataHelper.TryGetTypedLatestValue<T>(propertyName, SearchMode.At, out _, out var result, minTick: tick, maxTick: tick, logError: logError))
            {
                return result;
            }

            return defaultValue;
        }

        public T Get<T>(string propertyName)
            => GetInternal<T>(propertyName, targetTick, logError: true);

        public T GetOrDefault<T>(string propertyName, T defaultValue)
            => GetInternal<T>(propertyName, targetTick, logError: false, defaultValue);



        private IEnumerable<(int Version, T Value)> GetDetailedInternal<T>(string propertyName, int targetTick, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out _, out var value, SearchMode.At, minTick: targetTick, maxTick: targetTick, logError: logError))
            {
                return value;
            }

            return defaultValue ?? Enumerable.Empty<(int Version, T Value)>(); // Return the original tick and default values, if specified.
        }



        public IEnumerable<(int Version, T Value)> GetDetailed<T>(string propertyName, int targetTick)
            => GetDetailedInternal<T>(propertyName, targetTick, logError: true);

        public IEnumerable<(int Version, T Value)> GetDetailedOrDefault<T>(string propertyName, int targetTick, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedInternal<T>(propertyName, targetTick, logError: false, defaultValue);



        public void Dispose()
        {

        }
    }
}
