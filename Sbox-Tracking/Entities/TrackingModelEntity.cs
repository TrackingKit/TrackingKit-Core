using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public class TrackingModelEntity : ModelEntity, IManualTrackableObject
    {
        protected T GetProperty<T>(string propertyName, T propertyValue)
        {
            if (Tracker?.IsScoped ?? false)
            {
                if (Tracker.GetKeyExists(propertyName))
                {
                    return Tracker.GetPropertyOrLast<T>(propertyName, Tracker.SpecificObjectTick );
                }
            }

            return propertyValue;
        }

        protected void SetProperty<T>(string propertyName, T value, Action<T> baseSetter)
        {
            if(Tracker?.IsScoped ?? false) // if scoped we cant set properties.
            {
                Log.Error($"Can't set property {propertyName} whilst scoped");
                return;
            }

            Tracker?.Set(propertyName, value);
            baseSetter(value);
        }

        public ITracker Tracker { get; set; }

        // TODO: Should I be doing it like this.
        public new Model Model
        {
            get => GetProperty(nameof(Model), base.Model);
            set => SetProperty(nameof(Model), value, v => base.Model = v);
        }



        // There isnt a nice way of doing this easily I dont think.
        protected IEnumerable<Transform> internal_Bones
        {
            get
            {
                List<Transform> transforms = new List<Transform>();
                var bones = BoneCount;

                for (int i = 0; i < bones; i++)
                {
                    var tx = GetBoneTransform(i);
                    transforms.Add(tx);
                }

                return transforms.AsEnumerable();
            }
            set
            {
                var bones = BoneCount;

                if (value.Count() != bones)
                {
                    Log.Error("Bone count is not correct");
                    return;
                }

                var localPos = Position;
                var localRot = Rotation.Inverse;
                var valueList = value.ToList();

                for (int i = 0; i < bones; i++)
                {
                    var tx = valueList[i];
                    tx.Position = (tx.Position - localPos) * localRot + Position;
                    tx.Rotation = Rotation * (localRot * tx.Rotation);

                    SetBoneTransform(i, tx);
                }
            }
        }

        public IEnumerable<Transform> Bones
        {
            get => GetProperty(nameof(Bones), internal_Bones );
            set => SetProperty(nameof(Bones), value, v => internal_Bones = v);
        } 

        #region Entity

        // Issue: This should be overridable?
        public new float Health
        {
            get => GetProperty(nameof(Health), base.Health);
            set => SetProperty(nameof(Health), value, v => base.Health = v);
        }


        // TODO: Maybe we do a smart conversion?
        public override Vector3 Position
        {
            get => GetProperty(nameof(Position), base.Position);
            set => SetProperty(nameof(Position), value, v => base.Position = v);
        }

        

        public override Angles AngularVelocity
        {
            get => GetProperty(nameof(AngularVelocity), base.AngularVelocity);
            set => SetProperty(nameof(AngularVelocity), value, v => base.AngularVelocity = v);
        }


        public override Vector3 LocalPosition
        {
            get => GetProperty(nameof(LocalPosition), base.LocalPosition);
            set => SetProperty(nameof(LocalPosition), value, v => base.LocalPosition = v);
        }

        public override Rotation LocalRotation
        {
            get => GetProperty(nameof(LocalRotation), base.LocalRotation);
            set => SetProperty(nameof(LocalRotation), value, v => base.LocalRotation = v);
        }

        public override float Scale
        {
            get => GetProperty(nameof(Scale), base.Scale);
            set => SetProperty(nameof(Scale), value, v => base.Scale = v);
        }

        public override float LocalScale
        {
            get => GetProperty(nameof(LocalScale), base.LocalScale);
            set => SetProperty(nameof(LocalScale), value, v => base.LocalScale = v);
        }


        public override Vector3 LocalVelocity
        {
            get => GetProperty(nameof(LocalVelocity), base.LocalVelocity);
            set => SetProperty(nameof(LocalVelocity), value, v => base.LocalVelocity = v);
        }

        public override Vector3 Velocity
        {
            get => GetProperty(nameof(Velocity), base.Velocity);
            set => SetProperty(nameof(Velocity), value, v => base.Velocity = v);
        }

        public override Entity Parent
        {
            get => GetProperty(nameof(Parent), base.Parent);
            set => SetProperty(nameof(Parent), value, v => base.Parent = v);
        }

        public override Entity Owner
        {
            get => GetProperty(nameof(Owner), base.Owner);
            set => SetProperty(nameof(Owner), value, v => base.Owner = v);
        }

        public override Rotation Rotation
        {
            get => GetProperty(nameof(Rotation), base.Rotation);
            set => SetProperty(nameof(Rotation), value, v => base.Rotation = v);
        }
        #endregion




        private int PreviousTickHash { get; set; } = 0;

        // This is a way of recording properties at the end of each tick from the internal physics inside the engine we dont access with normal
        // commands.
        //[GameEvent.Physics.PostStep]

        [GameEvent.Tick.Server]
        private void RecordProperties()
        {
            if ( PhysicsBody == null ) return;

            var builtHash1 = HashCode.Combine(Model, Health, Position, Rotation, LocalRotation, LocalPosition, Scale, LocalScale );

            var builtHash2 = HashCode.Combine(LocalVelocity, Velocity, Parent, Owner);

            var builtHash = HashCode.Combine(builtHash1, builtHash2);

            if(builtHash != PreviousTickHash)
            {
                PreviousTickHash = builtHash;

                // TODO: Add Tag Engine/internal? As if these are different to last tick we aint got a clue at this point how this happened here except from Engine probably.

                Tracker?.Set(nameof(Model), Model);
                Tracker?.Set(nameof(Health), Health);
                Tracker?.Set(nameof(Position), Position);
                Tracker?.Set(nameof(Rotation), Rotation);
                Tracker?.Set(nameof(LocalPosition), LocalPosition);
                Tracker?.Set(nameof(Scale), Scale);
                Tracker?.Set(nameof(LocalScale), LocalScale);

                Tracker?.Set(nameof(LocalVelocity), LocalVelocity);
                Tracker?.Set(nameof(Velocity), Velocity);
                Tracker?.Set(nameof(Parent), Parent);
                Tracker?.Set(nameof(Owner), Owner);

                Tracker?.Set(nameof(Bones), Bones);

                /*
                Model = Model;
                Health = Health;
                Position = Position;
                Rotation = Rotation;
                LocalRotation = LocalRotation;
                LocalPosition = LocalPosition;
                Scale = Scale;
                LocalScale = LocalScale;
                Scale = Scale;
                LocalScale = LocalScale;

                LocalVelocity = LocalVelocity;
                Velocity = Velocity;
                Parent = Parent;
                Owner = Owner;
                */

            }
        }

    }
}
