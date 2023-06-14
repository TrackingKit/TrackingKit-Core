using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public class TrackerFilter
    {
        public int MinTick { get; set; }

        public int MaxTick { get; set; }


        //public int SpecificTick => MinTick;

        public bool IsScoped { get; set; }

        public string[] Tags { get; set; }

        public bool IsSpecificTick => MinTick == MaxTick;


        public bool InRange(int tick) => (tick >= MinTick) || (tick <= MaxTick);

        public bool InRange(int[] ticks) => ticks.All( x => InRange(x) ) ;

    }

    public partial class Tracker
    {
        public TrackerFilter Filter { get; set; } = new TrackerFilter();


        // We reset Filter once done with scope.
        public void Dispose() => Filter = new TrackerFilter();


        // TODO: Sort this all out.

        public ITrackerTickReadOnly Scope(int specificTick, params string[] idents)
        {
            if (Filter.IsScoped)
            {
                Log.Error($"Already scoped, filter: {Filter}");
                return default;
            }


            if( RecordedRange.min >= specificTick || specificTick >= RecordedRange.max)
            {
                Log.Error("This tick wasn't recorded during this interval");
                return default;
            }

            Filter.MaxTick = specificTick;
            Filter.MinTick = specificTick;

            return this;
        }

        public ITrackerReadOnly Scope(int minTick, int maxTick, params string[] idents)
        {
            if (Filter.IsScoped)
            {
                Log.Error($"Already scoped, filter: {Filter}");
                return default;
            }


            if (RecordedRange.min <= minTick || maxTick >= RecordedRange.max)
            {
                Log.Error("This tick wasn't recorded during this interval");
                return default;
            }

            Filter.MaxTick = minTick;
            Filter.MaxTick = maxTick;
            Filter.Tags = idents;



            return this;
        }

        public ITrackerReadOnly Scope(params string[] idents)
        {
            if (Filter.IsScoped)
            {
                Log.Error($"Already scoped, filter: {Filter}");
                return default;
            }


            Filter.Tags = idents;

            return Scope();
        }

        public ITrackerReadOnly Scope()
        {
            if (Filter.IsScoped)
            {
                Log.Error($"Already scoped, filter: {Filter}");
                return default;
            }

            Filter.MaxTick = int.MaxValue;
            Filter.MaxTick = int.MinValue;

            return this;
        }

    }
}
