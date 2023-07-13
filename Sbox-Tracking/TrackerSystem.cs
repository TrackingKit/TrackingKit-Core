using Sandbox;
using Sandbox.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Tracking
{
    // IRules
    // public bool AllowDuplcates
    // get => Rules contains Duplicates
    // set => if value == true (Add Rules) else (Remove Ruels)

    public static partial class TrackerSystem
    {
        private static Dictionary<object, Tracker> Values { get; set; } = new Dictionary<object, Tracker>();

        public static void Register<T>(T obj) 
            where T : class
        {
            if (!Values.ContainsKey(obj))
            {
                Values.Add(obj, new Tracker());
            }
        }

        public static void Unregister<T>(T obj) where T : class
        {
            Values.Remove(obj);
        }

        /// <summary>
        /// Removes any dead refernces.
        /// </summary>
        public static void CleanUp()
        {
            // TODO
        }


        public static Tracker Get<T>(T obj)
            where T : class
        {
            Values.TryGetValue(obj, out Tracker value);

            return value;
        }

        public static Tracker GetOrRegister<T>(T obj)
            where T : class
        {
            // TODO: Optimise
            if (!Values.ContainsKey(obj))
            {
                Values.Add(obj, new Tracker());
            }

            return Values[obj];
        }



        public static void Clear()
            => Values.Clear();

        

    }

    [Library("test_prop"), Spawnable]
    public class TestingEntity : ModelEntity
    {


        public override void Spawn()
        {
            Components.Add(new TrackingEntityComponent());

            Model = Cloud.Model("garry.beachball");

            SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

            base.Spawn();
        }
    }
}
