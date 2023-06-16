using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;
using Sandbox;

namespace DisplacedEntity
{
    [Library("Displaced Entity")]
    public partial class TimeDisplacedModelEntity : TrackingModelEntity
    {
        private ModelEntity DisplacedEntity { get; set; } 

        public override void Spawn()
        {
            Tracker = new Tracker();

            WaitScuffedTime = 0f;

            Model = Model.Load("models/citizen/citizen.vmdl");

            SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

            base.Spawn();
        }

        TimeSince WaitScuffedTime { get; set; } = 0f;

        [GameEvent.Tick.Server]
        public void Tick()
        {
            if (WaitScuffedTime < 2f) return;

            var aimtick = Time.Tick - Game.TickRate * 1;

            // 1s delay.
            using (var scoped = Tracker.Scope(aimtick) )
            {
                if (Tracker.GetKeyExists(nameof(Position)))
                {
                    if (DisplacedEntity == null)
                    {
                        DisplacedEntity = new ModelEntity();
                        DisplacedEntity.Model = Model.Load("models/citizen/citizen.vmdl");
                        DisplacedEntity.RenderColor = new Color(0,0,255, 0.5f);
                        
                    }

                    DisplacedEntity.Position = scoped.GetPropertyOrLast<Vector3>(nameof(Position));
                }

                if (Tracker.GetKeyExists(nameof(Rotation)))
                {
                    if (DisplacedEntity == null)
                    {
                        DisplacedEntity = new ModelEntity();
                        DisplacedEntity.Model = Model.Load("models/citizen/citizen.vmdl");
                        DisplacedEntity.RenderColor = new Color(0, 0, 255, 0.5f);

                    }

                    DisplacedEntity.Rotation = scoped.GetPropertyOrLast<Rotation>(nameof(Rotation));
                }



                //DisplacedEntity.Rotation = scoped.GetPropertyOrLast<Rotation>(nameof(Rotation));
            }
        }

    }
}
