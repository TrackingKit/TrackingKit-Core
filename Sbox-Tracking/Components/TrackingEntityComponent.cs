using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Sandbox.Components
{
    public partial class TrackingEntityComponent : EntityComponent, ISingletonComponent
    {

        protected Tracker Tracker => Tracking.TrackerSystem.GetOrRegister(Entity);


        public bool TrackBodyParts { get; set; } = false;

        public bool TrackBones { get; set; } = false;



        [GameEvent.Physics.PostStep]
        protected virtual void PhysicsPostStep()
        {
            Tracker?.StartGroup("tracking.automatic");

            RecordHandle();

            Tracker?.EndGroup("tracking.automatic");

        }

        protected virtual void RecordHandle()
        {
            // Entity Properties.
            Tracker?.Add(nameof(Entity.LocalPosition), Entity.LocalPosition);
            Tracker?.Add(nameof(Entity.LocalRotation), Entity.LocalRotation);
            Tracker?.Add(nameof(Entity.Parent), Entity.Parent);
            Tracker?.Add(nameof(Entity.Owner), Entity.Owner);

            Tracker?.Add(nameof(Entity.Velocity), Entity.Velocity);
            Tracker?.Add(nameof(Entity.BaseVelocity), Entity.BaseVelocity);
            Tracker?.Add(nameof(Entity.LocalVelocity), Entity.LocalVelocity);


            if (Entity is ModelEntity modelEntity)
            {
                if (TrackBodyParts)
                    Tracker?.Add("BodyParts", modelEntity.Name); // TODO Actual get bodyparts.

                if (TrackBones)
                    Tracker?.Add("Bones", modelEntity.Name); // TODO: Actual bones.
            }
            else
                return; // No point carrying on if not this level.

            if (Entity is AnimatedEntity animatedEntity)
            {
                // Properties.
            }
            else
                return; // No point carrying on if not this level.
        }

    }
}
