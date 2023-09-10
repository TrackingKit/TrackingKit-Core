using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;
using Tracking.Rules;
using Tracking.RulesService;

namespace TrackingKit_Core
{
    public sealed class Tracker<TKey> : ITracker
        where TKey : IEquatable<TKey>
    {
        internal Tracker(TrackerStorageType Type)
        {
            switch (Type)
            {
                case TrackerStorageType.InMemory:
                    Data = new InMemoryTrackerStorage<TKey>();
                    break;  // This break was missing
                default:
                    break;
            }
        }



        internal ITrackerStorage<TKey> Data { get; }

        /// <summary> Rules for Adding and Deleteing data. </summary>
        public TrackerRulesService Rules { get; } = new();

        public GroupManager Groups { get; } = new();


        public bool Add(TKey propertyName, object value, DateTime timePoint, params string[] idents)
        {
            // TODO: TimeUnit conversion.

            // InternalAdd etc.
            return default;
        }

        public bool Add(TKey propertyName, object value, TimeSpan timePoint, params string[] idents)
        {
            // TODO: TimeUnit conversion.

            // InternalAdd etc.
            return default;
        }

        public bool Add(TKey propertyName, object value, ITimeUnit timePoint, params string[] idents)
        {
            //if (Pause)
            //    return;


            var latestValueDataFound = Data.TryGetLatestVersion(propertyName, timePoint.ToBaseUnit(), out int outputVersion);


            int nextVersion;

            if (latestValueDataFound)
            {
                // Assuming output is a dictionary of ticks to another dictionary of versions to metadata
                // You might need to adjust this based on the exact structure of your data
                nextVersion = outputVersion + 1;
            }
            else
            {
                nextVersion = 1;
            }


            // Merging identifiers from the current scope with the identifiers passed as parameters
            var tags = idents.AsEnumerable().Concat(Groups.GetCurrentGroupTags()).ToArray();

            // TODO: Key should come back for circuit.

            // Checking rules to see if the property can be added
            var resultRule = Rules.GetAll<ITrackerRule<TKey>>().ShortCircuit(c => c.ShouldAdd(propertyName, value));

            // If the result is false, we exit the method without adding the property
            if (resultRule.HasValue && resultRule.Value == false)
                return false;

            // TODO: Should we be checking if exists then throw expection and do update or what?

            // Adding the value with the given property name, tick, new version, value, and tags
            Data.AddValueWithTags(propertyName, tick, nextVersion, value, tags);
            return true;
        }


    }
}
