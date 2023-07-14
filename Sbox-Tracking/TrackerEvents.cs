using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public static class TrackerEvents
    {
        public class AddedAttribute : EventAttribute
        {
            public AddedAttribute()
                : base("tracker.added")
            {
            }

            public static void Run(object obj, Tracker tracker)
            {
                Event.Run("tracker.added", obj, tracker);
            }
        }

        public class RemovedAttribute : EventAttribute
        {
            public RemovedAttribute()
                : base("tracker.removed")
            {
            }

            public static void Run(Tracker tracker)
            {
                Event.Run("tracker.removed", tracker);
            }
        }
    }
}
