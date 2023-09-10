using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    public sealed class ScopedSecondTracker : IDisposable
    {
        private readonly ScopedSecondsTrackingHelper DataHelper;

        private readonly double targetSecond;

        internal ScopedSecondTracker(InMemoryTrackerStorage data, ScopedSecondSettings scopedSettings)
        {
            targetSecond = scopedSettings.MinSecond;

            DataHelper = new(data, scopedSettings);
        }


        public int Count()
            => DataHelper.Count();

        public bool Exists(string propertyName)
            => DataHelper.PropertyExists(propertyName);

        public bool Exists()
            => DataHelper.Exists();

        private T GetInternal<T>(string propertyName, double time, bool logError, T defaultValue = default)
        {
            if (DataHelper.TryGetTypedLatestValue<T>(propertyName, ScopedTrackingHelper.SearchMode.At, out _, out var result, minSecond: time, maxSecond: time, logError: logError))
            {
                return result;
            }

            return defaultValue;
        }

        public T Get<T>(string propertyName)
            => GetInternal<T>(propertyName, targetSecond, logError: true);

        public T GetOrDefault<T>(string propertyName, T defaultValue)
            => GetInternal<T>(propertyName, targetSecond, logError: false, defaultValue);



        private IEnumerable<(int Version, T Value)> GetDetailedInternal<T>(string propertyName, double second, bool logError, IEnumerable<(int Version, T Value)> defaultValue = default)
        {
            if (DataHelper.TryGetTypedDetailedValues<T>(propertyName, out _, out var value, ScopedTrackingHelper.SearchMode.At, minSecond: second, maxSecond: second, logError: logError))
            {
                return value;
            }

            return defaultValue ?? Enumerable.Empty<(int Version, T Value)>(); // Return the original tick and default values, if specified.
        }



        public IEnumerable<(int Version, T Value)> GetDetailed<T>(string propertyName)
            => GetDetailedInternal<T>(propertyName, targetSecond, logError: true);

        public IEnumerable<(int Version, T Value)> GetDetailedOrDefault<T>(string propertyName, IEnumerable<(int Version, T Value)> defaultValue)
            => GetDetailedInternal<T>(propertyName, targetSecond, logError: false, defaultValue);



        public void Dispose()
        {

        }
    }
}
