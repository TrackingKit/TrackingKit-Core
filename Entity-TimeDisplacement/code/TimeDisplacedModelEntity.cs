using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;
using Sandbox;

namespace DisplacedEntity
{
    [Library("Displaced Entity")]
    public partial class TimeDisplacedModelEntity : TrackingModelEntity
    {
        private TrackingModelEntity DisplacedEntity { get; set; } 

        public override void Spawn()
        {
            Tracker = new Tracker();

            WaitScuffedTime = 0f;

            Model = Model.Load("models/citizen/citizen.vmdl");

            SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

            base.Spawn();
        }

        TimeSince WaitScuffedTime { get; set; } = 0f;

        [GameEvent.Tick.Server]
        public void Tick()
        {
            if (WaitScuffedTime < 2f) return;

            var aimtick = Time.Tick - Game.TickRate * 1;

            // 1s delay.
            using (var scoped = Tracker.Scope(aimtick) )
            {
                

                if(DisplacedEntity == null)
                {
                    DisplacedEntity = new TrackingModelEntity
                    {
                        Model = Model.Load("models/citizen/citizen.vmdl"),
                        RenderColor = new Color(0, 0, 255, 0.5f)

                    };

                    DisplacedEntity.SetupPhysicsFromModel(PhysicsMotionType.Keyframed );

                    DisplacedEntity.EnableAllCollisions = false;
                }

                
                if(Tracker.GetKeyExists(nameof(Position)))
                    DisplacedEntity.Position = scoped.GetPropertyOrLast<Vector3>(nameof(Position));

                if (Tracker.GetKeyExists(nameof(Rotation)))
                    DisplacedEntity.Rotation = scoped.GetPropertyOrLast<Rotation>(nameof(Rotation));



                if (Tracker.GetKeyExists(nameof(Bones)))
                {



                    var displacedEntity = scoped.GetObject( this );


                    
                    var displacedEntityBones = displacedEntity.Bones;

                    Log.Info(displacedEntityBones != Bones);

                    DisplacedEntity.Bones = displacedEntityBones;

                    //displacedEntity.Position = Vector3.Backward;


                    /*
                    if( displacedEntityBones != Bones )
                    {
                        var localPos = displacedEntity.LocalPosition;
                        var localRot = displacedEntity.LocalRotation;

                        var rot = displacedEntity.Rotation;
                        var pos = displacedEntity.Position;
                        var scale = displacedEntity.Scale;

                        for (int i = 0; i < Bones.Count(); i++)
                        {
                            var tx = displacedEntityBones.ElementAt(i);




                            tx.Position = (tx.Position - localPos) * localRot * rot + pos;
                            tx.Rotation = rot * (localRot * tx.Rotation);
                            tx.Scale = scale;

                            DisplacedEntity.SetBoneTransform(i, tx);
                        }

                    }
                    */

                   
                    

                }



                //DisplacedEntity.Rotation = scoped.GetPropertyOrLast<Rotation>(nameof(Rotation));
            }



        }

    }
}
