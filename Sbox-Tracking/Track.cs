﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public partial class Track
    {
        IDictionary<object, ITracker> Values { get; set; }

        // TODO: if has a ITrackableObject then reference that in values.
        public static void Register(object obj)
        {

        }

        public static void Unregister(object obj)
        {

        }

        [GameEvent.Physics.PreStep]
        public static void PhysicsPreStep()
        {
            // TODO: Basic "parser" for objects subscribed.
        }

        public static ITracker Get(object obj)
        {
            return default;
        }


    }
}