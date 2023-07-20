using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public static class TrackingDebug
    {
        private static WeakReference<object> Targetted { get; set; }

        [ConCmd.Server("tracking_debug_view")]
        public static void Target(string query)
        {
            if (query == "")
            {
                Targetted = null;
            }

            var allTrackerData = TrackerSystem.GetAll();

            (object key, Tracker obj)? targetItem = default;

            foreach (var item in allTrackerData)
            {
                if (item.key.ToString().Contains(query))
                {
                    targetItem = item;
                }
            }

            if (targetItem.HasValue && targetItem != null)
            {
                Targetted = new WeakReference<object>(targetItem.Value.key);
            }

        }


        [GameEvent.Tick.Server]
        private static void Tick()
        {


            int currentLine = 0;


            var allTrackerData = TrackerSystem.GetAll();

            DebugOverlay.ScreenText($"Tracker Keys | Count: {allTrackerData.Count()}", currentLine);


            foreach (var tracker in allTrackerData)
            {
                currentLine++;
                DebugOverlay.ScreenText($"{tracker.key.ToString()}", currentLine);
            }

            if (Targetted == null)
                return;

            bool objRefExists = Targetted.TryGetTarget(out object item);

            // Detailed
            if (objRefExists && TrackerSystem.Get(item) != null)
            {
                Tracker tracker = TrackerSystem.Get(item);

                currentLine++;
                DebugOverlay.ScreenText($"Detailed View for {item.ToString()}", currentLine);

                // Loop over the tracker's data and display it
                using(var scope = tracker.ScopeByTicks())
                {
                    currentLine++;

                    //DebugOverlay.ScreenText($"Total Values: {scope.Count()}", currentLine);

                    foreach(var key in scope.GetDistinctKeys())
                    {
                        currentLine++;

                        DebugOverlay.ScreenText($"Key: {key}", currentLine);

                    }

                }
            }

        }
    }
}
