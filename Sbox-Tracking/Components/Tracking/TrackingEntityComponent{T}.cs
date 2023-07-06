using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    public partial class TrackingEntityComponent<TEntity> : EntityComponent<TEntity>, ISingletonComponent
        where TEntity : Entity
    {
        protected ITracker Tracker { get; private set; }

        // TODO: maybe readonly part has scope in it etc. and then we 
        // have interfaces limiting to tick etc.
        public ITrackerReadOnly TrackerReadOnly => Tracker;



        private readonly Dictionary<string, int> Hashes = new();

        protected void TrackCondition(string name, object obj)
        {
            if (obj == null) return;

            if (!Hashes.ContainsKey(name))
                Hashes.Add(name, obj.GetHashCode());

            if (Hashes[name] == obj.GetHashCode())
                return;
            else
            {
                Hashes[name] = obj.GetHashCode();
                Tracker?.Set(name, obj);
            }
        }

        // TODO: Do we check if control already has last value recorded already? This might be bad icl,
        // maybe a filter setting. Just keep this code now for avoiding data duplication.

        [GameEvent.Physics.PostStep(Priority = int.MaxValue)]
        protected virtual void PostStep()
        {
            TrackCondition(nameof(Entity.Position), Entity.Position);
            TrackCondition(nameof(Entity.LocalPosition), Entity.LocalPosition);

            TrackCondition(nameof(Entity.Rotation), Entity.Rotation);
            TrackCondition(nameof(Entity.LocalRotation), Entity.LocalRotation);


            TrackCondition(nameof(Entity.Scale), Entity.Scale);
            TrackCondition(nameof(Entity.LocalScale), Entity.LocalScale);

            TrackCondition(nameof(Entity.LocalVelocity), Entity.LocalVelocity);
            TrackCondition(nameof(Entity.Velocity), Entity.Velocity);

            // TODO: IF we do parent then we can calculate Velocity, Scale etc with parent relative
            // to local.
            TrackCondition(nameof(Entity.Parent), Entity.Parent);

            TrackCondition(nameof(Entity.Owner), Entity.Owner);
        }

    }
}
