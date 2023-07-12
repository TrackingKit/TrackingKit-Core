using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public partial class TrackingDisplacementEntityComponent<TEntity> : EntityComponent<TEntity>
        where TEntity : Entity, new()
    {
        // TODO: TDisplacementParam? So we dont just copy TEntity blindlsly. 

        public TEntity DisplacementEntity { get; set; }

        private TrackingEntityComponent<TEntity> TrackingComponent => Entity?.Components?.Get<TrackingEntityComponent<TEntity>>() ?? default;




        [GameEvent.Physics.PostStep]
        protected void PostStep()
        {
            if (TrackingComponent == null)
                return;

            EntityDisplacementProcess();
        }

        // TODO: Better name?
        protected virtual void EntityDisplacementProcess()
        {
            var position = TrackingComponent.TrackerReadOnly.GetPropertyOrLast<Vector3>(nameof(Entity.Position), Time.Tick - 30);

            DisplacementEntity.Position = position;
        }




        protected override void OnActivate()
        {
            DisplacementEntity = new TEntity();

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            DisplacementEntity?.Delete();

            base.OnDeactivate();
        }


        public override bool CanAddToEntity(Entity entity)
        {
            var comp = Entity?.Components?.Get<TrackingEntityComponent<TEntity>>();

            if (comp == null)
                return false;

            return base.CanAddToEntity(entity);
        }

    }
}
