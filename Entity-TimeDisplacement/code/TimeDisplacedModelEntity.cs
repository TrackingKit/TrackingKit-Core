using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;
using Sandbox;

namespace DisplacedEntity
{
    /*
    [Library("Displaced Entity"), Spawnable]
    public partial class TimeDisplacedModelEntity : TrackingModelEntity
    {
        private TrackingModelEntity DisplacedEntity { get; set; } 

        public override void Spawn()
        {
            Tracker = new Tracker();

            WaitScuffedTime = 0f;

            Model = Model.Load("models/citizen/citizen.vmdl");

           // Model = Cloud.Model("garry.beachball");


            SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

            base.Spawn();
        }

        TimeSince WaitScuffedTime { get; set; } = 0f;




        [GameEvent.Physics.PostStep(Priority = int.MaxValue)]
        public void PostStep()
        {

            if (WaitScuffedTime < 2f) return;

            if (Tracker == null) return;

            var aimtick = Time.Tick - Game.TickRate * 2;




            // 1s delay.
            using (var scoped = Tracker.Scope(aimtick) )
            {
                
                

                if(DisplacedEntity == null)
                {
                    DisplacedEntity = new TrackingModelEntity
                    {
                        Model = Model.Load("models/citizen/citizen.vmdl"),
                        //Model = Cloud.Model("garry.beachball"),
                        RenderColor = new Color(0, 0, 255, 0.5f),
                    };

                    DisplacedEntity.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
                    DisplacedEntity.EnableAllCollisions = false;



                    // IMPORTANT: if you aren't using PhysicsGroup (Multiple physicbodies) to track the entity body then use this
                    //DisplacedEntity.PhysicsEnabled = false;
                   
                }




                //Log.Info(LocalBodyParts.ElementAt(3).Position - DisplacedEntity.LocalBodyParts.ElementAt(3).Position);


                //foreach (var part in LocalBodyParts)
                //    DebugOverlay.Sphere(part.Position, 1, Color.Blue);




                //foreach (var part in DisplacedEntity.LocalBodyParts)
                //    DebugOverlay.Sphere(part.Position, 1, Color.Red);


                //DisplacedEntity.Bones = Bones;
                


                //Log.Info(LocalBodyParts.ElementAt(1).Rotation - DisplacedEntity.LocalBodyParts.ElementAt(1).Rotation);

                if (Tracker.GetKeyExists(nameof(BodyParts)))
                {
                    
                    var scopedOriginalEntityLocalBodyParts = scoped.GetPropertyOrLast<IEnumerable<Transform>>(nameof(BodyParts));


                    //Log.Info("hi");

                     DisplacedEntity.BodyParts = scopedOriginalEntityLocalBodyParts;


                }
                else
                {
                        /*
                        for (int i = 0; i < DisplacedEntity.PhysicsGroup.BodyCount; i++)
                        {

                            var replicatedBody = DisplacedEntity.PhysicsGroup.GetBody(i);

                            var originalBody = PhysicsGroup.GetBody(i);


                            replicatedBody.Position = originalBody.Position;
                            replicatedBody.Rotation = originalBody.Rotation;
                            replicatedBody.BodyType = originalBody.BodyType;
                        }
                        */
                }




                //DisplacedEntity.EnableDrawing = (DisplacedEntity.LocalBodyParts == LocalBodyParts);

                /*

                if (Tracker.GetKeyExists(nameof(Position)))
                    DisplacedEntity.Position = scoped.GetPropertyOrLast<Vector3>(nameof(Position));
                else
                    DisplacedEntity.Position = Position;


                if (Tracker.GetKeyExists(nameof(Rotation)))
                    DisplacedEntity.Rotation = scoped.GetPropertyOrLast<Rotation>(nameof(Rotation));
                else
                    DisplacedEntity.Rotation = Rotation;


                */

            }



        }

    }*/

}
