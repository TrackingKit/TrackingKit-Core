using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;
using Sandbox;

namespace Tracking.Tests
{
    /*
    #if DEBUG

    // Helps determine if I messed up somehow can useful for other cases.
    public static class BonesBodyTest
    {
        // Just using TrackingModelEntity has got bones property on it.
        public static TrackingModelEntity Original { get; set; }

        public static TrackingModelEntity ParentTest { get; set; }


        private static bool IsActive => Original != null;


        [ConCmd.Server("timeDisplacement_unitTest_bones")]
        public static void Toggle()
        {
            bool shouldRemove = Original != null;

            Log.Info(shouldRemove);

            if (shouldRemove)
            {
                Original?.Delete();

                Original = null;

                ParentTest?.Delete();

                ParentTest = null;
            }
            else
            {
                Original = new TrackingModelEntity
                {
                    Model = Model.Load("models/citizen/citizen.vmdl"),
                    Name = "Child",
                    RenderColor = new Color(0, 0, 0, 0.5f),
                };

                Original.SetupPhysicsFromModel(PhysicsMotionType.Dynamic, true);
                //Original.EnableAllCollisions = false;

                ParentTest = new TrackingModelEntity
                {
                    Model = Model.Load("models/sbox_props/bin/rubbish_food_box.vmdl"),
                    Position = Vector3.Up * 100f,
                    Name = "Parent",

                };

                ParentTest.SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

                Original.EnableAllCollisions = false;


                Original.SetParent(ParentTest);
                Original.LocalPosition = Vector3.Zero;

            }

        }

        [ConVar.Server("debug_body_show")] private static bool ShowBody { get; set; } = false;

        [ConVar.Server("debug_bones_show")] private static bool ShowBones { get; set; } = false;


        [GameEvent.Physics.PostStep]
        public static void PostStep()
        {
            if (!Game.IsServer || !IsActive) return;





            //Original.Bones = ParentTest.Bones;

            // Origin:
            DebugOverlay.Sphere(Vector3.Zero, 1, Color.Orange);



            if (ShowBody)
            {
                Log.Info("body");

                // TODO: Do local transform world?
                foreach (var bodyPart in Original.LocalBodyParts)
                    DebugOverlay.Sphere(bodyPart.ToLocal(Original.Parent.Transform).Position, 1, Color.Blue);
                
                
                foreach (var bodyPart in Original.BodyParts)
                    DebugOverlay.Sphere(bodyPart.Position, 2, Color.Red);
                
            }

            if (ShowBones)
            {
                Log.Info("bones");

                foreach (var bone in Original.LocalBones)
                    DebugOverlay.Sphere(bone.Position, 1, Color.Blue);

                foreach (var bone in Original.Bones)
                    DebugOverlay.Sphere(bone.Position, 2, Color.Red);
            }






        }



    }
    #endif
    */
}
