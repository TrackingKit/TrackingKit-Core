using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public partial class TrackingDisplacementAnimatedEntityComponent<TAnimatedEntity> : TrackingDisplacementModelEntityComponent<TAnimatedEntity>
        where TAnimatedEntity : AnimatedEntity, new()
    {
        private TrackingModelEntityComponent<TAnimatedEntity> TrackingComponent => Entity?.Components?.Get<TrackingModelEntityComponent<TAnimatedEntity>>() ?? default;




        public override bool CanAddToEntity(Entity entity)
        {
            var comp = Entity?.Components?.Get<TrackingModelEntityComponent<TAnimatedEntity>>();

            // TODO: Maybe do polymorphism? as we keep dong this for up to 3 times, so we put method on just entity.
            if (comp == null)
                return false;

            return base.CanAddToEntity(entity);
        }

    }
}
