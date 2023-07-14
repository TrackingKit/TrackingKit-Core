using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Sandbox.Components
{
    public partial class TrackingDisplacementEntityComponent : EntityComponent<Entity>
    {
        [Net] protected Entity DisplacedEntity { get; set; }

        protected Tracker Tracker => TrackerSystem.GetOrRegister(Entity);

        protected override void OnActivate()
        {
            // needs tracking component
            Entity.Components.GetOrCreate<TrackingEntityComponent>();

            if (Entity is ModelEntity)
            {
                DisplacedEntity = new ModelEntity();
            }
            else if (Entity is AnimatedEntity)
            {
                DisplacedEntity = new AnimatedEntity();
            }
            else
                DisplacedEntity = new Entity();

            base.OnActivate();
        }


        [GameEvent.Tick.Server]
        public void Tick()
        {
            using( var tracker = Tracker.Scope() )
            {
                // Entity part

                DisplacedEntity.Position = tracker.GetOrPreviousOrDefault<Vector3>(nameof(Entity.Position), Time.Tick, Entity.Position);
                DisplacedEntity.Rotation = tracker.GetOrPreviousOrDefault<Rotation>(nameof(Entity.Rotation), Time.Tick, Entity.Rotation);

                var item = tracker.GetOrPreviousOrDefault(nameof(Entity.Position), Time.Tick, Entity.Position);


            }


        }


    }
}
