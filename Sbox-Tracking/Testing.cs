using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Sandbox
{
    public partial class Testing
    {

        public void Test()
        {
            TrackerEntity w = new TrackerEntity();


            using (var item = w.Tracker.Scope(1, 30))
            {
                var entityAtTick = item.GetObject<TrackerEntity>( 20 );

                entityAtTick.Position = 2;

                Log.Info(entityAtTick.Position);
            }
        }
    }
}
