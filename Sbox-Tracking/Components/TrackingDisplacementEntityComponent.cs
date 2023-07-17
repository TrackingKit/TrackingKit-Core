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


        protected bool HasAuthroity => (Game.IsServer && IsServerOnly) 
            || (Game.IsServer && ShouldTransmit) 
            || (Game.IsClient && IsClientOnly);

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

            var previousTick = Time.Tick - 100;


            using ( var tracker = Tracker.ScopeByTicks(previousTick, Time.Tick) )
            {
                // Entity part

                // So sometimes cuts out and not sure why.

                if (tracker.ExistsAtOrBefore(nameof(Entity.Position), previousTick) &&
                     tracker.ExistsAtOrBefore(nameof(Entity.Rotation), previousTick))
                {

                    DisplacedEntity.EnableDrawing = true;

                    var positionOfTracked = tracker.GetOrPreviousOrDefault<Vector3>(nameof(Entity.Position), previousTick, Entity.Position);


                    var rotationOfTracker = tracker.GetOrPreviousOrDefault<Rotation>(nameof(Entity.Rotation), previousTick, Entity.Rotation);

                    // If not set we set else if done we will just hide again.
                    if (DisplacedEntity.Position != positionOfTracked || DisplacedEntity.Rotation != rotationOfTracker)
                    {

                        DisplacedEntity.Position = positionOfTracked;
                        DisplacedEntity.Rotation = rotationOfTracker;
                    }
                    else
                    {
                        DisplacedEntity.EnableDrawing = false;
                    }

                }
                




                //Log.Info(item);

                //Log.Info(tracker.Get<Vector3>(nameof(Entity.Position), Time.Tick - 100));

            }


        }


        protected override void OnDeactivate()
        {
            if(HasAuthroity)
                DisplacedEntity?.Delete();

            base.OnDeactivate();
        }


    }
}
