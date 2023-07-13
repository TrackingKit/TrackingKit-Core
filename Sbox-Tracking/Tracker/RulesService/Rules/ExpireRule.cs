using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Sandbox;

namespace Tracking.Rules
{
    public partial class ExpireRule : TrackerRule
    {
        // This can be inaccurate if tickrate changes

        /// <summary> How much till it expires </summary>
        public int Seconds { get; set; } = 5;

        // TODO: Might be better we just have TimeSince on TrackerKey.
        private double SecondsSince(int pastTick)
        {
            int currentTick = Time.Tick;
            int tickDifference = currentTick - pastTick;

            // If each tick represents a time interval equal to the inverse of the tick rate
            double timeIntervalPerTick = 1.0 / Game.TickRate;  // the time interval (in seconds) that each tick represents
            double timeSinceInSeconds = tickDifference * timeIntervalPerTick;

            return timeSinceInSeconds;
        }

        public override Optional<bool> ShouldDelete(TrackerKey key, object obj)
        {

            return Optional<bool>.Of(true);
            //if( SecondsSince(key.Tick) > Seconds)
            //    return Optional<bool>.Of(false);

            return base.ShouldDelete(key, obj);
        }

    }
}
