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
        // TODO: This is annoying internal private?
        internal TrackerStorage Data { get; } = new();

        internal Tracker()
        {
            Event.Register(this);

            // Requires data.
            Rules = new TrackerRulesService(Data);

            // TODO: Remove from here.
            Rules.Register<AllowRule>();
            Rules.Register<DuplicateRule>();
            Rules.Register<ExpireRule>();

        }

        ~Tracker() => Event.Unregister(this);

        /// <summary> Rules for Adding and Deleteing data. </summary>
        public TrackerRulesService Rules { get; private set; } 


        /// <summary> Ability to pause taking in values. </summary>
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
                HandleDelete();
            }
        }

        protected void HandleDelete()
        {

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


            var latestValueDataFound = Data.TryGetPropertyValue(propertyName, out var output, minTick: tick, maxTick: tick);


            int nextVersion;

            if (latestValueDataFound)
            {
                // Assuming output is a dictionary of ticks to another dictionary of versions to metadata
                // You might need to adjust this based on the exact structure of your data
                nextVersion = output.Max(kvp => kvp.Value.Max(innerKvp => innerKvp.Key)) + 1;
            }
            else
            {
                nextVersion = 1;
            }


            // Merging identifiers from the current scope with the identifiers passed as parameters
            var tags = idents.AsEnumerable().Concat(CurrentBuildTags).ToArray();

            // TODO: Key should come back for circuit.

            // Checking rules to see if the property can be added
            var resultRule = Rules.GetAll<TrackerRule>().ShortCircuit(c => c.ShouldAdd(propertyName, value));

            // If the result is false, we exit the method without adding the property
            if (resultRule.HasValue && resultRule.Value == false)
                return;

            // Adding the value with the given property name, tick, new version, value, and tags
            Data.SetValue(propertyName, tick, nextVersion, value, tags);
        }


        public void Add(string propertyName, object value, params string[] idents)
            => Add(propertyName, value, Time.Tick, idents);

        public void RemoveValues(string propertyName, int tick)
            => Data.RemoveValues(propertyName, tick);

        public void RemovSpecificVersion(string propertyName, int tick, int version)
            => Data.RemoveSpecificValue(propertyName, tick, version);


    }

}