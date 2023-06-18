using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public partial class Tracker : ITracker
    {
        // TODO: Linked list, sort out a way of data.
        private readonly SortedDictionary<TrackerKey, object> Values = new SortedDictionary<TrackerKey, object>();

        /// <summary> Our filter of allowing entries. </summary>
        public InputFilterSettings InputFilterSettings { get; set; } = new InputFilterSettings();


        // Building

        public HashSet<string> CurrentBuildTags { get; set; } = new HashSet<string>();

        public void EndGroup(params string[] idents) => idents?.ToList().ForEach(ident => CurrentBuildTags.Remove(ident));

        public void StartGroup(params string[] idents) => idents?.ToList().ForEach(ident => CurrentBuildTags.Add(ident));


        public Tracker() => Event.Register(this);


        [GameEvent.Tick.Server]
        protected void Tick()
        {
            // Set the threshold for old ticks
            int oldTickThreshold = Time.Tick - 60;

            // Find keys which are older than the threshold
            var keysToRemove = Values.Keys.Where(key => key.Tick < oldTickThreshold).ToList();

            // Remove the old entries
            foreach (var key in keysToRemove)
            {
                Values.Remove(key);
            }
        }

        protected (int min, int max) RecordedRange => (Values.Min(x => x.Key.Tick), Values.Max(x => x.Key.Tick));

        // TODO: maybe allow people to set back in time but I aint sure on that at all.
        // It might feel a bit flawed.
        public void Set(string propertyName, object value, params string[] idents)
        {

            var latestRecordedVersion = Values.Keys
                .Where(x => x.Tick == Time.Tick && x.PropertyName == propertyName)
                .Select(key => key.Version)
                .DefaultIfEmpty(0) // Provide 0 as the default value if the collection is empty
                .Max();

            TrackerKey key = new()
            {
                PropertyName = propertyName,
                Version = latestRecordedVersion + 1,
                Tags = idents.AsEnumerable().Concat(CurrentBuildTags).ToArray(),
                Tick = Time.Tick,
            };


            if (!CanSet(propertyName, value)) return;

            Values.Add(key, value);
        }

        protected bool CanSet(string propertyName, object value)
        {
            if (!InputFilterSettings.IsPropertyWhitelisted(propertyName))
                return false;


            // TODO: Fix this.
            /*
            if (!InputFilterSettings.AllowConsecutiveDuplicateValues && GetKeyExists(propertyName) && GetPropertyOrLast<object>(propertyName, Time.Tick) == value)
                return false;
            */

            return true;
        }

        public bool GetKeyExists(string propertyName) => Values.Keys.Where(x => x.PropertyName == propertyName ).Any();


    }

}