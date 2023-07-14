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

            if(Entity is ModelEntity targetModelEnt && DisplacedEntity is ModelEntity displacementModelEntity)
            {
                displacementModelEntity.Model = targetModelEnt.Model;
                displacementModelEntity.RenderColor = new Color(0, 0, 100, 0.6f);
            }

            base.OnActivate();
        }


        [GameEvent.Tick.Server]
        public void Tick()
        {
            if (Tracker == null)
                return;

            using( var tracker = Tracker.Scope() )
            {
                // Entity part

                DisplacedEntity.Position = tracker.GetOrPreviousOrDefault<Vector3>(nameof(Entity.Position), Time.Tick - 100, Entity.Position);
                DisplacedEntity.Rotation = tracker.GetOrPreviousOrDefault<Rotation>(nameof(Entity.Rotation), Time.Tick - 100, Entity.Rotation);



                //Log.Info(item);

                //Log.Info(tracker.Get<Vector3>(nameof(Entity.Position), Time.Tick - 100));

            }


        }


        protected override void OnDeactivate()
        {
            DisplacedEntity.Delete();

            base.OnDeactivate();
        }


    }
}
