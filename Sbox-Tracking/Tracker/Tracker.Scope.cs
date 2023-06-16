using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public class OutputFilterSettings
    {
        public int MinTick { get; init; }

        public int MaxTick { get; init; }

        public bool IsSpecificTick => MinTick == MaxTick;

        /// <summary> Any tags we should filter out. </summary>
        public string[] Tags { get; init; }

    }

    public partial class Tracker
    {
        public bool IsScoped => OutputFilterSettings != null;

        protected OutputFilterSettings OutputFilterSettings { get; set; }


        // We reset Filter once done with scope.
        public void Dispose()
        {
            OutputFilterSettings = null;
        }

        protected bool CanScope(int minTick, int maxTick, bool supressMessages = true, params string[] idents)
        {
            if (IsScoped)
            {
                if(!supressMessages) Log.Error("Already scoped, please close current scope");

                return false;
            }

            if(minTick > maxTick)
            {
                if (!supressMessages) Log.Warning("MinTick greater than MaxTick");
            }




            return true;
        }


        // TODO: Sort this all out.

        public ITrackerTickReadOnly Scope(int specificTick, params string[] idents)
        {
            if (!CanScope(specificTick, specificTick, false, idents)) return null;


            OutputFilterSettings = new OutputFilterSettings()
            {
                MinTick = specificTick,
                MaxTick = specificTick,
                Tags = idents
            };

            return this;
        }


        // Scope exists so we ensure the user knows if they're going out of bounds.
        public ITrackerReadOnly Scope(int minTick, int maxTick, params string[] idents)
        {
            if (!CanScope(minTick, maxTick, false, idents)) return null;


            OutputFilterSettings = new OutputFilterSettings()
            {
                MinTick = minTick,
                MaxTick = maxTick,
                Tags = idents
            };

            return this;
        }

        public ITrackerReadOnly Scope(params string[] idents)
        {
            if (!CanScope(int.MinValue, int.MaxValue, false, idents)) return null;


            OutputFilterSettings = new OutputFilterSettings()
            {
                MinTick = int.MinValue,
                MaxTick = int.MaxValue,
                Tags = idents
            };

            return this;
        }

        public ITrackerReadOnly Scope()
        {
            if (!CanScope(int.MinValue, int.MaxValue, false)) return null;


            OutputFilterSettings = new OutputFilterSettings()
            {
                MinTick = int.MinValue,
                MaxTick = int.MaxValue,
            };

            return this;
        }

    }
}
