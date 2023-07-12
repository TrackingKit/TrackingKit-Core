using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public partial class TrackingAnimatedEntityComponent<TAnimatedEntity> : TrackingModelEntityComponent<TAnimatedEntity>
        where TAnimatedEntity : AnimatedEntity
    {
        /// <summary> This can be performance intensitve. </summary>
        public bool TrackAnimations { get; set; } = true;



        protected override void PostStep()
        {
            // TODO: Animations????

            base.PostStep();
        }

    }
}
