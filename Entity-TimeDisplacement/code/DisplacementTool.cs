using Sandbox.Components;
using Sandbox.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Sandbox
{
    [Library("tool_displacement", Title = "Displacement", Description = "View a displacement of the entity by 5 seconds.", Group = "fun")]
    public class DisplacementTool : BaseTool
    {
        public override void Simulate()
        {
            bool pressed = Input.Pressed("attack1");


            if ( !pressed )
                return;

            using (Prediction.Off())
            {
                var tr = DoTrace();

                if (!tr.Hit || !tr.Entity.IsValid())
                    return;


                if (tr.Entity is not ModelEntity modelEnt)
                    return;

                var tracker = TrackerSystem.Get(modelEnt);

                if(Game.IsServer)
                    modelEnt.Components.GetOrCreate<TrackingDisplacementEntityComponent>();


                if (Input.Pressed("reload"))
                {
                    var filter = new TagFilter();

                    filter.DefaultFilterOption = FilterOption.Include;

                    FileSystem.Data.WriteJson("wowzers.json", tracker.CopyData(filter: filter) ); // tracker.CopyData() 

                }
            }



            base.Simulate();
        }



    }
}
