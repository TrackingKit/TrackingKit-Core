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
            // Disabled for now as its bad for: GetObject how I set it up.

            /*
            if (Tracker?.IsScoped ?? false && Tracker.SpecificObjectTick  && Tracker.GetKeyExists(propertyName))
            {
                if (propertyName == nameof(LocalBodyParts))
                    Log.Info("hi");

                return Tracker.GetPropertyOrLast<T>(propertyName, Tracker.SpecificObjectTick);

            }
            */
            

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


        //public 


        private IEnumerable<Transform> internal_bodyparts
        {
            get
            {
                List<Transform> transforms = new List<Transform>();

                
                var bodyCount = PhysicsGroup.BodyCount;
                

                for (int i = 0; i < bodyCount; i++)
                {

                    // we make it local as we can use parent and this to calculate normal BodyParts.
                    var tx = PhysicsGroup.GetBody( i ).Transform;

                    

                    transforms.Add( tx );
                }

                return transforms;
            }

            set
            {

                var bodyCount = PhysicsGroup.BodyCount;

                if (value.Count() != bodyCount)
                {
                    Log.Error("Bone count is not correct");
                    return;
                }

                var valueList = value.ToList();


                for (int i = 0; i < bodyCount; i++)
                {
                    var tx = valueList[i];


                    PhysicsGroup.GetBody(i).Transform.ToLocal(tx);
                }
            }
        }

        public IEnumerable<Transform> BodyParts
        {
            get => GetProperty(nameof(BodyParts), internal_bodyparts);
            set => SetProperty(nameof(BodyParts), value, v => internal_bodyparts = v);
        }


        private IEnumerable<Transform> internal_Bones
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

                // TODO: I commented out but doesnt make sense when we go from world in set,
                // dont wnna translate.

                //var localPos = Position;
                //var localRot = Rotation.Inverse;

                var valueList = value.ToList();

                for (int i = 0; i < bones; i++)
                {
                    // XXXXX Translate from Local to Actual.
                    var tx = valueList[i];
                    //tx.Position = (tx.Position - localPos) * localRot + Position;
                    //tx.Rotation = Rotation * (localRot * tx.Rotation);
                    //tx.Scale = Scale;

                    SetBoneTransform(i, tx);
                }
            }
        }

        


        private IEnumerable<Transform> internal_localbones
        {
            get
            {
                List<Transform> transforms = new List<Transform>();
                var bones = BoneCount;

                for (int i = 0; i < bones; i++)
                {
                    // We want it related to entity (local)
                    var tx = GetBoneTransform(i, false);
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

                var valueList = value.ToList();

                for (int i = 0; i < bones; i++)
                {
                    var tx = valueList[i];

                    SetBoneTransform(i, tx, false);
                }
            }
        }

        public IEnumerable<Transform> LocalBones
        {
            get => GetProperty(nameof(LocalBones), internal_localbones);
            set => SetProperty(nameof(LocalBones), value, v => internal_localbones = v);
        }

        /// <summary>
        /// 
        /// 
        /// <para><strong>NOTE:</strong> You should probably not track this and keep a track of Transforms of PhysicBodies</para>
        /// </summary>
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


        public void T()
        {
            
        }



        private Dictionary<string, int> Hashes = new Dictionary<string, int>();

        private void TrackCondition(string name, object obj)
        {
            if (obj == null) return;

            if (!Hashes.ContainsKey(name))
                Hashes.Add(name, obj.GetHashCode());

            if ( Hashes[name] == obj.GetHashCode())
                return;
            else
            {
                Hashes[name] = obj.GetHashCode();
                Tracker?.Set(name, obj);
            }
        }
        
        [GameEvent.Physics.PostStep(Priority = int.MaxValue)]
        private void RecordProperties()
        {
            if ( PhysicsBody == null ) return;

            if (Game.IsClient) return;


            // TODO: Add Tag Engine/internal? As if these are different to last tick we aint got a clue at this point how this happened here except from Engine probably.


            

            TrackCondition(nameof(Model), Model);
            TrackCondition(nameof(Health), Health);

            // TODO: We need to instead consider a smart detection for related items such as Position and Rotation to
            // avoid data repitition.

            // or maybe in Position just do:

            //Transform.PointToWorld(LocalPosition);
            // When we're transferring from one to another.

            TrackCondition(nameof(Position), Position);
            TrackCondition(nameof(Rotation), Rotation);
            TrackCondition(nameof(LocalPosition), LocalPosition);
            TrackCondition(nameof(Scale), Scale);
            TrackCondition(nameof(LocalScale), LocalScale);

            TrackCondition(nameof(LocalVelocity), LocalVelocity);
            TrackCondition(nameof(Velocity), Velocity);
            TrackCondition(nameof(Parent), Parent);
            TrackCondition(nameof(Owner), Owner);



            TrackCondition(nameof(BodyParts), BodyParts);


            // You should be using PhysicsGroup for tracking as they basically relate to bones, 99% on this.
            // But bones keeping to easily able to add bones on top of models to each other etc.

            //TrackCondition(nameof(Bones), Bones);
        }

    }
}
