
using System;
using System.Collections.Generic;
using System.Linq;
using Tracking;

namespace Tracking
{
    public static partial class TrackerSystem
    {
        private static Dictionary<WeakReference<object>, Tracker> Values { get; set; } = new();

        public static void Register<T>(T obj) where T : class
        {
            var weakReference = new WeakReference<object>(obj);

            if (!Values.Keys.Any(wr => wr.TryGetTarget(out var target) && Equals(target, obj)))
            {
                var tracker = new Tracker();
                Values.Add(weakReference, tracker);

                TrackerEvents.OnAdded(tracker);
            }
        }

        public static void Unregister<T>(T obj) where T : class
        {
            var weakReference = Values.Keys.FirstOrDefault(wr => wr.TryGetTarget(out var target) && Equals(target, obj));

            if (weakReference != null)
            {
                Values.Remove(weakReference);
            }
        }

        // TODO: not each tick.
        public static void CleanUp()
        {
            var deadKeys = GetDeadKeys();
            RemoveDeadKeys(deadKeys);
        }

        private static List<WeakReference<object>> GetDeadKeys()
        {
            var deadKeys = new List<WeakReference<object>>();

            foreach (var key in Values.Keys)
            {
                if (!key.TryGetTarget(out var target))
                {
                    deadKeys.Add(key);
                }
            }

            return deadKeys;
        }

        private static void RemoveDeadKeys(List<WeakReference<object>> deadKeys)
        {
            foreach (var key in deadKeys)
            {
                TrackerEvents.OnRemoved(Values[key]);
                Values.Remove(key);
            }
        }

        public static Tracker Get<T>(T obj) where T : class
        {
            var weakReference = Values.Keys.FirstOrDefault(wr => wr.TryGetTarget(out var target) && Equals(target, obj));

            return weakReference != null ? Values[weakReference] : null;
        }

        public static Tracker GetOrRegister<T>(T obj) where T : class
        {
            var weakReference = Values.Keys.FirstOrDefault(wr => wr.TryGetTarget(out var target) && Equals(target, obj));

            if (weakReference == null)
            {
                weakReference = new WeakReference<object>(obj);
                var tracker = new Tracker();

                Values.Add(weakReference, tracker);

                TrackerEvents.OnAdded(tracker);
            }

            return Values[weakReference];
        }

        public static void Clear() 
            => Values.Clear();

        internal static IEnumerable<(object key, Tracker obj)> GetAll()
        {
            // Create a list to store the results
            var results = new List<(object key, Tracker obj)>();

            // Loop over the dictionary
            foreach (var pair in Values)
            {
                // If the weak reference is still alive (i.e., the target object hasn't been garbage collected), 
                // add the target object and associated tracker to the results
                if (pair.Key.TryGetTarget(out var target))
                {
                    results.Add((target, pair.Value));
                }
            }

            // Return the results
            return results;
        }

    }
}
