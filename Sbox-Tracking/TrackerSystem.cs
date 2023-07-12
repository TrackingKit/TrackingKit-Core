﻿using System;
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
                Values.Add(obj, new Tracker()); // replace Tracker with your ITracker implementation
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
            // TODO:
        }


        public static Tracker Get<T>(T obj)
            where T : class
        {
            return default;
        }

        public static Tracker GetOrRegister<T>(T obj)
            where T : class
        {
            return default;
        }



        public static void Clear()
            => Values.Clear();


    }
}
