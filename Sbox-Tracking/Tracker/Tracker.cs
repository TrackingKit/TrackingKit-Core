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


    public partial class Tracker
    {
        protected TrackerData Data { get; set; } = new();

        internal Tracker()
        {
            Event.Register(this);

            Rules.Register<AllowRule>();


            //Rules.Register<ExpireRule>();
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
            //Log.Info(Data.RecordedRange.minTick);
            Log.Info(Data.RecordedRange);


            if (Time.Tick % Game.TickRate == 0)
            {
                // TODO: Rule Expire is expensive I think.
                HandleDelete();
            }


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

            //Log.Info($"Deleting: {keysToCleanUp.Count}");

            foreach(var key in keysToCleanUp)
                Data.RemoveValue(key);

        }



        /// <summary>
        /// Adds the value of a property with the specified name within the current groups.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="tick"></param>
        /// <param name="idents">The identifiers for the property.</param>
        public void Add(string propertyName, object value, int tick, params string[] idents)
        {
            int latestRecordedVersion = Data.GetLatestVersion(propertyName, tick);

            var keyVersion = latestRecordedVersion + 1;

            var keyTags = idents.AsEnumerable().Concat(CurrentBuildTags).ToArray();

            TrackerKey trackerKey = new()
            {
                PropertyName = propertyName,
                Version = latestRecordedVersion + 1,
                Tags = idents.AsEnumerable().Concat(CurrentBuildTags).ToArray(),
                Tick = tick,
            };


            // Check rules if can add.
            var result = Rules.GetAll<TrackerRule>().ShortCircuitForResult(c => c.ShouldAdd(propertyName, value));

            if (result.HasValue && result.Value == false)
                return;

            Data.AddValue(trackerKey, value);
        }

        public void Add(string propertyName, object value, params string[] idents)
            => Add(propertyName, value, Time.Tick, idents);

        [Obsolete("Needs implementing")]
        public void Remove(string propertyName, int tick)
        {
            
        }


        [Obsolete("Needs implementing")]
        public void RemoveAllVersions(string propertyName, int tick, int version)
        {
            TrackerKey trackerKey = new()
            {
                PropertyName = propertyName,
                Version = version,
                Tick = tick,
            };

            Data.RemoveValue(trackerKey);
        }


    }

}