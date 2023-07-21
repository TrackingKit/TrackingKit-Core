using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public partial class ScopedTicksTracker : IDisposable
    {
        private TrackerData Data { get; }
        private ScopedTickSettings ScopedSettings { get; }

        internal ScopedTicksTracker(TrackerData data, ScopedTickSettings scopedSettings)
        {
            Data = data;
            ScopedSettings = scopedSettings;
        }

        private void ClampAndWarn(ref int tick)
        {
            if (tick < ScopedSettings.MinTick)
            {
                Log.Warning($"Tick {tick} is less than ScopedSettings.MinTick. Clamping to ScopedSettings.MinTick.");
                tick = ScopedSettings.MinTick;
            }
            else if (tick > ScopedSettings.MaxTick)
            {
                Log.Warning($"Tick {tick} is greater than ScopedSettings.MaxTick. Clamping to ScopedSettings.MaxTick.");
                tick = ScopedSettings.MaxTick;
            }
        }

        #region Count, DistinctKeys and Exists methods

        //[Obsolete("This doesnt consider scope.")]
        //public int Count() 
       //    => Data.Count;


        [Obsolete("This doesnt consider scope.")]
        public IEnumerable<string> GetDistinctKeys()
            => Data.DistinctKeys;

        // and tags?

        // TODO Log.Error scoping out of bounds 
        public bool Exists(string propertyName)
            => Data.Exists(new TrackerRangeQuery() 
            { 
                MinTick = ScopedSettings.MinTick, MaxTick = ScopedSettings.MaxTick, PropertyName = propertyName, Tags = ScopedSettings.Tags
            });




        public bool ExistsAtOrBefore(string propertyName, int tick)
        {
            ClampAndWarn(ref tick);

            // TODO: Exists
            return Data.Exists(new TrackerRangeQuery()
            {
                MinTick = ScopedSettings.MinTick,
                MaxTick = tick,
                PropertyName = propertyName,
                Tags = ScopedSettings.Tags
            });
        }

        public bool ExistsAtOrAfter(string propertyName, int tick)
        {
            ClampAndWarn(ref tick);

            return Data.Exists(new TrackerRangeQuery()
            {
                MinTick = tick,
                MaxTick = ScopedSettings.MaxTick,
                PropertyName = propertyName,
                Tags = ScopedSettings.Tags
            });
        }

        public bool ExistsAt(string propertyName, int tick)
        {
            ClampAndWarn(ref tick);

            return Data.Exists(new TrackerRangeQuery()
            {
                MinTick = ScopedSettings.MinTick,
                MaxTick = ScopedSettings.MaxTick,
                PropertyName = propertyName,
                Tags = ScopedSettings.Tags
            });
        }


        #endregion

        #region Get methods

        private T GetInternal<T>(string propertyName, int tick, bool logError, T defaultValue = default)
        {
            ClampAndWarn(ref tick);

            TrackerQuery trackerQuery = new TrackerQuery()
            {
                Tags = ScopedSettings.Tags,
                PropertyName = propertyName,
                Tick = tick,
            };

            if (Data.TryGetLatestValue(trackerQuery, out TrackerQueryResult result))
                return (T)result.Value.Value;

            if (logError)
                Log.Error($"No valid found for {propertyName}, {tick}");

            return defaultValue;
        }


        public T Get<T>(string propertyName, int tick)
            => GetInternal<T>(propertyName, tick, logError: true);

        public T GetOrDefault<T>(string propertyName, int tick, T defaultValue)
            => GetInternal<T>(propertyName, tick, logError: false, defaultValue);


        #endregion

        #region GetOrPrevious methods

        private T GetOrPreviousInternal<T>(string propertyName, int tick, bool logError, T defaultValue = default)
        {
            ClampAndWarn(ref tick);

            TrackerQuery trackerQuery = new TrackerQuery()
            {
                Tags = ScopedSettings.Tags,
                PropertyName = propertyName,
                Tick = tick,
            };

            // Try to get the value at the given tick
            bool found = Data.TryGetLatestValue(trackerQuery, out TrackerQueryResult result);

            // If no value was found at the given tick, try to get the latest value before that tick
            if (!found)
            {

                TrackerRangeQuery trackerRangedQuery = new TrackerRangeQuery()
                {
                    Tags = ScopedSettings.Tags,
                    PropertyName = propertyName,
                    MaxTick = tick,
                    MinTick = ScopedSettings.MinTick
                };



                found = Data.TryGetLatestValueAtPreviousAvailableTick(trackerRangedQuery, out TrackerQueryResult resultPrevious);

                // If no value was found even at the previous ticks, log an error and return default value
                if (!found)
                {
                    if (logError)
                    {
                        Log.Error($"No valid value found for {propertyName}, {tick} at previous ticks");
                    }

                    return defaultValue;
                }
            }

            return (T)result.Value.Value;
        }

        public T GetOrPrevious<T>(string propertyName, int tick)
            => GetOrPreviousInternal<T>(propertyName, tick, true);

        public T GetOrPreviousOrDefault<T>(string propertyName, int tick, T defaultValue)
            => GetOrPreviousInternal<T>(propertyName, tick, false, defaultValue);

        #endregion

        #region GetOrNext methods

        private T GetOrNextInternal<T>(string propertyName, int tick, bool logError, T defaultValue = default)
        {
            ClampAndWarn(ref tick);

            TrackerQuery trackerQuery = new TrackerQuery()
            {
                Tags = ScopedSettings.Tags,
                PropertyName = propertyName,
                Tick = tick,
            };

            // Try to get the value at the given tick
            bool found = Data.TryGetLatestValue(trackerQuery, out TrackerQueryResult result);

            // If no value was found at the given tick, try to get the earliest value after that tick
            if (!found)
            {
                TrackerRangeQuery trackerRangedQuery = new TrackerRangeQuery()
                {
                    Tags = ScopedSettings.Tags,
                    PropertyName = propertyName,
                    MinTick = tick,
                    MaxTick = ScopedSettings.MaxTick
                };

                found = Data.TryGetLatestValueAtNextAvailableTick(trackerRangedQuery, out TrackerQueryResult resultNext);

                // If a value was found at the next available tick, update the result
                if (found)
                {
                    result = resultNext;
                }
            }

            // If no value was found even at the next ticks, log an error and return default value
            if (!found)
            {
                if (logError)
                {
                    Log.Error($"No valid value found for {propertyName}, {tick} at next ticks");
                }

                return defaultValue;
            }

            return (T)result.Value.Value;
        }


        public T GetOrNext<T>(string propertyName, int tick)
            => GetOrNextInternal<T>(propertyName, tick, logError: true);

        public T GetOrNextOrDefault<T>(string propertyName, int tick, T defaultValue)
            => GetOrNextInternal<T>(propertyName, tick, logError: false, defaultValue);

        #endregion


        #region GetDetailed methods

        private IEnumerable<T> GetDetailedInternal<T>(string propertyName, int tick, bool logError, IEnumerable<T> defaultValue = default)
        {
            ClampAndWarn(ref tick);

            TrackerQuery trackerQuery = new TrackerQuery()
            {
                Tags = ScopedSettings.Tags,
                PropertyName = propertyName,
                Tick = tick,
            };

            bool found = Data.TryGetDetailedValue(trackerQuery, out TrackerDetailedQueryResult result);

            if (!found)
            {
                if (logError)
                {
                    Log.Error($"Failed to find any values for {propertyName}, {tick}");
                }

                return defaultValue;
            }

            return result.Values.Select(x => (T)x.Value);
        }


        public IEnumerable<T> GetDetailed<T>(string propertyName, int tick)
            => GetDetailedInternal<T>(propertyName, tick, logError: true);

        public IEnumerable<T> GetDetailedOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
            => GetDetailedInternal<T>(propertyName, tick, logError: false, defaultValue);

        #endregion

        #region GetDetailedOrPrevious methods

        private IEnumerable<T> GetDetailedOrPreviousInternal<T>(string propertyName, int tick, bool logError, IEnumerable<T> defaultValue = default)
        {
            ClampAndWarn(ref tick);

            TrackerQuery trackerQuery = new TrackerQuery()
            {
                Tags = ScopedSettings.Tags,
                PropertyName = propertyName,
                Tick = tick,
            };

            // First try to get the detailed value at the given tick
            bool found = Data.TryGetDetailedValue(trackerQuery, out TrackerDetailedQueryResult result);

            // If no value was found at the given tick, try to get the detailed value before that tick
            if (!found)
            {
                TrackerRangeQuery trackerRangedQuery = new TrackerRangeQuery()
                {
                    Tags = ScopedSettings.Tags,
                    PropertyName = propertyName,
                    MaxTick = tick,
                    MinTick = ScopedSettings.MinTick
                };

                found = Data.TryGetDetailedValuesAtPreviousAvailableTick(trackerRangedQuery, out result);
            }

            // If no value was found before the given tick, return the default value
            if (!found)
            {
                if (logError)
                {
                    Log.Error($"No valid values found for {propertyName}, {tick}");
                }

                return defaultValue;
            }

            return result.Values.Select(kv => (T)kv.Value);
        }


        public IEnumerable<T> GetDetailedOrPrevious<T>(string propertyName, int tick)
            => GetDetailedOrPreviousInternal<T>(propertyName, tick, logError: true);

        public IEnumerable<T> GetDetailedOrPreviousOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
            => GetDetailedOrPreviousInternal<T>(propertyName, tick, logError: false, defaultValue);

        #endregion

        #region GetDetailedOrNext methods

        private IEnumerable<T> GetDetailedOrNextInternal<T>(string propertyName, int tick, bool logError, IEnumerable<T> defaultValue = default)
        {
            ClampAndWarn(ref tick);

            TrackerQuery trackerQuery = new TrackerQuery()
            {
                Tags = ScopedSettings.Tags,
                PropertyName = propertyName,
                Tick = tick,
            };

            // First try to get the detailed value at the given tick
            bool found = Data.TryGetDetailedValue(trackerQuery, out TrackerDetailedQueryResult result);

            // If no value was found at the given tick, try to get the detailed value after that tick
            if (!found)
            {
                TrackerRangeQuery trackerRangedQuery = new TrackerRangeQuery()
                {
                    Tags = ScopedSettings.Tags,
                    PropertyName = propertyName,
                    MinTick = tick,
                    MaxTick = ScopedSettings.MaxTick
                };

                found = Data.TryGetDetailedValuesAtNextAvailableTick(trackerRangedQuery, out result);
            }

            // If no value was found after the given tick, return the default value
            if (!found)
            {
                if (logError)
                {
                    Log.Error($"No valid values found for {propertyName}, {tick}");
                }

                return defaultValue;
            }

            return result.Values.Select(kv => (T)kv.Value);
        }

        public IEnumerable<T> GetDetailedOrNext<T>(string propertyName, int tick)
            => GetDetailedOrNextInternal<T>(propertyName, tick, logError: true);

        public IEnumerable<T> GetDetailedOrNextOrDefault<T>(string propertyName, int tick, IEnumerable<T> defaultValue)
            => GetDetailedOrNextInternal<T>(propertyName, tick, logError: false, defaultValue);


        #endregion

        public void Dispose()
        {

        }
    }
}
