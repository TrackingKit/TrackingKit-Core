using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public partial class TrackingModelEntityComponent<TModelEntity> : TrackingEntityComponent<TModelEntity>
        where TModelEntity : ModelEntity
    {




        protected override void PostStep()
        {
            TrackCondition(nameof(ModelEntity.Model), Entity.Model);
            // Bones?

            base.PostStep();
        }
    }
}
