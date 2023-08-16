﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TrackingKit;

namespace Tracking.Rules
{
    public partial class ExpireRule : TrackerRule
    {
        // This can be inaccurate if tickrate changes

        /// <summary> How much till it expires </summary>
        public int Seconds { get; set; } = 5;

        public override bool? ShouldDelete(TrackerKey key, object obj)
        {

            return null;

            var tickEstimate = TimeUtility.SecondToTick(TimeFactory.Now - Seconds);


            // If lower than tick estimate amount then has estimated to of passed expire amount.
            if (tickEstimate < key.Tick)
                return true;

            return base.ShouldDelete(key, obj);
        }

    }
}