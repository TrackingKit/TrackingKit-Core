using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Tracking.Rules;
using Tracking.RulesService;

namespace Tracking
{
    public class TrackerData
    {
        private readonly SortedDictionary<TrackerKey, object> Values = new SortedDictionary<TrackerKey, object>();
        private readonly SortedDictionary<(string PropertyName, int Tick), int> LatestVersions = new SortedDictionary<(string, int), int>();

        public int Count => Values.Count;

        public bool GetKeyExists(string propertyName)
            => Values.Keys.Where(x => x.PropertyName == propertyName ).Any();


        public int GetLatestVersion(string propertyName, int tick)
        {
            var key = (PropertyName: propertyName, Tick: tick);
            LatestVersions.TryGetValue(key, out int latestRecordedVersion);
            return latestRecordedVersion;
        }

        public void UpdateVersion(string propertyName, int tick, int version)
        {
            var key = (PropertyName: propertyName, Tick: tick);
            LatestVersions[key] = version;
        }

        public void AddValue(TrackerKey key, object value)
        {
            Values[key] = value;
        }

        public void RemoveValue(TrackerKey key)
        {
            Values.Remove(key);

            // Remove version associated with this key
            var versionKey = (PropertyName: key.PropertyName, Tick: key.Tick);
            if (LatestVersions.ContainsKey(versionKey) && LatestVersions[versionKey] == key.Version)
            {
                LatestVersions.Remove(versionKey);
            }
        }

        public IEnumerable<KeyValuePair<TrackerKey, object>> GetValues() => Values;
    }


    public partial class Tracker
    {
        protected TrackerData Data { get; set; } = new();

        internal Tracker()
        {
            Event.Register(this);

            Rules.Register<AllowRule>();
            Rules.Register<ExpireRule>();
        }

        ~Tracker() => Event.Unregister(this);

        /// <summary> Rules for Adding and Deleteing data. </summary>
        public TrackerRulesService Rules { get; set; } = new TrackerRulesService();


        // Building

        public HashSet<string> CurrentBuildTags { get; set; } = new HashSet<string>();


        /// <summary>
        /// Ends the group with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the group.</param>
        public void EndGroup(params string[] idents) 
            => idents?.ToList().ForEach(ident => CurrentBuildTags.Remove(ident));

        /// <summary>
        /// Begins a new group with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the group.</param>
        public void StartGroup(params string[] idents) 
            => idents?.ToList().ForEach(ident => CurrentBuildTags.Add(ident));





        [GameEvent.Tick.Server]
        protected void Tick()
        {
            //Log.Info(Data.Count);

            HandleDelete();

            // TODO: duplicate rule
        }

        protected void HandleDelete()
        {
            List<TrackerKey> keysToCleanUp = new();

            foreach(var value in Data.GetValues())
            {
                var result = Rules.GetAll<TrackerRule>().ShortCircuitForResult(c => c.ShouldDelete(value.Key, value.Value));

                if(result.HasValue && result.Value)
                    keysToCleanUp.Add(value.Key);
            }

            Log.Info($"Deleting: {keysToCleanUp.Count}");

            foreach(var key in keysToCleanUp)
                Data.RemoveValue(key);

        }



        // TODO: maybe allow people to set back in time but I aint sure on that at all.
        // It might feel a bit flawed.

        /// <summary>
        /// Sets the value of a property with the specified name within the current groups.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="idents">The identifiers for the property.</param>
        public void Set(string propertyName, object value, params string[] idents)
        {
            int tick = Time.Tick;
            int latestRecordedVersion = Data.GetLatestVersion(propertyName, tick);

            TrackerKey trackerKey = new()
            {
                PropertyName = propertyName,
                Version = latestRecordedVersion + 1,
                Tags = idents.AsEnumerable().Concat(CurrentBuildTags).ToArray(),
                Tick = tick,
            };

            // Update the version tracker
            Data.UpdateVersion(propertyName, tick, trackerKey.Version);

            // Check rules if can add.
            var result = Rules.GetAll<TrackerRule>().ShortCircuitForResult(c => c.ShouldAdd(propertyName, value));

            if (result.HasValue && result.Value == false)
                return;

            Data.AddValue(trackerKey, value);
        }




    }

}