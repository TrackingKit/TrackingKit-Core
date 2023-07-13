using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.Gizmo;

namespace Tracking
{
    public class ScopeSettings
    {
        public int MinTick { get; init; }

        public int MaxTick { get; init; }

        public bool IsSpecificTick => MinTick == MaxTick;

        /// <summary> Any tags we should filter out. </summary>
        public string[] Tags { get; init; }

    }

    public partial class Tracker
    {
        protected bool IsScoped => ScopeSettings != null;

        protected ScopeSettings ScopeSettings { get; set; }


        // We reset Filter once done with scope.
        public void Dispose()
        {
            ScopeSettings = null;
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


            ScopeSettings = new ScopeSettings()
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


            ScopeSettings = new ScopeSettings()
            {
                MinTick = minTick,
                MaxTick = maxTick,
                Tags = idents
            };

            return default;
        }

        public ITrackerReadOnly Scope(params string[] idents)
        {
            if (!CanScope(int.MinValue, int.MaxValue, false, idents)) return null;


            ScopeSettings = new ScopeSettings()
            {
                MinTick = int.MinValue,
                MaxTick = int.MaxValue,
                Tags = idents
            };

            return default;
        }

        public ITrackerReadOnly Scope()
        {
            if (!CanScope(int.MinValue, int.MaxValue, false)) return null;


            ScopeSettings = new ScopeSettings()
            {
                MinTick = int.MinValue,
                MaxTick = int.MaxValue,
            };

            return default;
        }

    }
}
