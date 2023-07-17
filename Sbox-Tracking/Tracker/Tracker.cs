using System;
using System.Collections.Concurrent;
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
        protected TrackerData Data { get; } = new();

        internal Tracker()
        {
            Event.Register(this);

            // Requires data.
            Rules = new TrackerRulesService(Data);

            // TODO: Remove from here.
            Rules.Register<AllowRule>();
            Rules.Register<DuplicateRule>();

        }

        ~Tracker() => Event.Unregister(this);

        /// <summary> Rules for Adding and Deleteing data. </summary>
        public TrackerRulesService Rules { get; } 

        public bool Pause { get; set; }


        // Building

        public HashSet<string> CurrentBuildTags { get; } = new HashSet<string>();


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
            //Log.Info(Data.Count);


            if (Time.Tick % Game.TickRate == 0)
            {
                // TODO: Rule Expire is expensive I think.
                _ = HandleDelete();
            }
        }

        // TODO: Double check all this logic is fine.
        protected async Task HandleDelete()
        {
            ConcurrentBag<TrackerKey> keysToCleanUp = new();

            // Use Task.WhenAll to await multiple tasks at once
            await GameTask.WhenAll(Data.Query().Select(value =>
                GameTask.RunInThreadAsync(() =>
                {
                    var result = Rules.GetAll<TrackerRule>().ShortCircuit(c => c.ShouldDelete(value.Key, value.Value));
                    if (result.HasValue && result.Value == true)
                        keysToCleanUp.Add(value.Key);
                })));



            // I aint sure if thread safe. So just doing safe for now.
            foreach (var key in keysToCleanUp)
                Data.RemoveValue(key.PropertyName, key.Tick, key.Version);
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
            if (Pause)
                return;

            // Getting the latest version recorded for the specified property at a given tick
            int latestRecordedVersion = Data.GetLatestVersion(propertyName, tick);

            // Creating a new version by incrementing the latest recorded version
            var newVersion = latestRecordedVersion + 1;

            // Merging identifiers from the current scope with the identifiers passed as parameters
            var tags = idents.AsEnumerable().Concat(CurrentBuildTags).ToArray();

            // Checking rules to see if the property can be added
            var result = Rules.GetAll<TrackerRule>().ShortCircuit(c => c.ShouldAdd(propertyName, value));

            // If the result is false, we exit the method without adding the property
            if (result.HasValue && result.Value == false)
                return;

            // Adding the value with the given property name, tick, new version, value, and tags
            Data.AddValue(propertyName, tick, newVersion, value, tags);
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

            //Data.RemoveValue(trackerKey);
        }


    }

}