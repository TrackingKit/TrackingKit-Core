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
                displacementModelEntity.Model = (targetModelEnt.Model);
                displacementModelEntity.RenderColor = new Color(0, 0, 100, 0.6f);
            }

            base.OnActivate();
        }


        public int DisplacementSeconds { get; set; } = 2;


        [GameEvent.Tick.Server]
        public void Tick()
        {
            if (Tracker == null)
                return;



            var displacementTime = Time.Now - 1;


            using(var tracker = Tracker.ScopeBySecond(Time.Now - 1))
            {
                Log.Info(tracker.Count());
            }

            // TODO: This logic just feels wrong and we should be doing something like "Second" here idk tho.
            
            using( var tracker = Tracker.ScopeBySeconds(displacementTime, Time.Now))
            {
                Vector3 lastKnownPosition = DisplacedEntity.Position;
                Rotation lastKnownRotation = DisplacedEntity.Rotation;

                // Check if tracking data exists for both position and rotation at displacementTick
                if (tracker.Exists( nameof(Entity.Position) ) && tracker.Exists( nameof(Entity.Rotation) ))
                {
                    // Get the position and rotation of the Entity at displacementTick
                    var positionOfTracked = tracker.GetOrNextOrDefault<Vector3>(nameof(Entity.Position), displacementTime, lastKnownPosition).Data;
                    var rotationOfTracker = tracker.GetOrNextOrDefault<Rotation>(nameof(Entity.Rotation), displacementTime, lastKnownRotation).Data;

                    // Apply the tracked position and rotation to the DisplacedEntity if they're not the same already
                    if (DisplacedEntity.Position != positionOfTracked || DisplacedEntity.Rotation != rotationOfTracker)
                    {
                        DisplacedEntity.EnableDrawing = true;
                        DisplacedEntity.Position = positionOfTracked;
                        DisplacedEntity.Rotation = rotationOfTracker;
                    }
                    else
                    {
                        // Hide the DisplacedEntity if its position and rotation are the same as the Entity's ones
                        if (DisplacedEntity.Position == Entity.Position && DisplacedEntity.Rotation == Entity.Rotation)
                        {
                            DisplacedEntity.EnableDrawing = false;
                        }
                    }
                }
                else
                {
                    // Keep the DisplacedEntity at its last displacement position and rotation if there's no tracking data
                    DisplacedEntity.Position = lastKnownPosition;
                    DisplacedEntity.Rotation = lastKnownRotation;
                    DisplacedEntity.EnableDrawing = false;
                }
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
