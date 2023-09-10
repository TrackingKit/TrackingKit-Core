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

        


        private static void Tick()
        {


            int currentLine = 0;


            var allTrackerData = TrackerSystem.GetAll();



            foreach (var tracker in allTrackerData)
            {
                currentLine++;
            }

            if (Targetted == null)
                return;

            bool objRefExists = Targetted.TryGetTarget(out object item);

            // Detailed
            if (objRefExists && TrackerSystem.Get(item) != null)
            {
                DynamicTracker tracker = TrackerSystem.Get(item);

                currentLine++;

                // Loop over the tracker's data and display it
                using(var scope = tracker.ScopeByTicks())
                {
                    currentLine++;

                    //DebugOverlay.ScreenText($"Total Values: {scope.Count()}", currentLine);

                    /*
                    foreach(var key in scope.GetDistinctKeys())
                    {
                        currentLine++;

                        DebugOverlay.ScreenText($"Key: {key}", currentLine);

                    }
                    */
                }
            }

        }
    }
}
