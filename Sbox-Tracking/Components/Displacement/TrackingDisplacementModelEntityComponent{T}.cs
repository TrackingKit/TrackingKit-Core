using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public partial class TrackingDisplacementModelEntityComponent<TModelEntity> : TrackingDisplacementEntityComponent<TModelEntity>
        where TModelEntity : ModelEntity, new()
    {

        private TrackingModelEntityComponent<TModelEntity> TrackingComponent => Entity?.Components?.Get<TrackingModelEntityComponent<TModelEntity>>() ?? default;


        protected override void EntityDisplacementProcess()
        {

            // TODO: Process parts for ModelEntity.

            base.EntityDisplacementProcess();
        }

    }
}
