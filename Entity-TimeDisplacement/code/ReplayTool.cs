using Sandbox.Components;
using Sandbox.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace Sandbox
{
    [Library("tool_replay", Title = "Replay", Description = "Allows to replay objects in time.", Group = "fun")]
    public class ReplayTool : BaseTool
    {
        public ReplayTool() => Event.Register(this);


        protected List<ModelEntity> Selected { get; set; } = new();
        public enum ToolMode { Select, Rewind }
        protected ToolMode Mode { get; set; } = ToolMode.Select;


        // New properties
        protected float RewindRate { get; set; } = 0.2f;
        protected float AccumulatedRewindAmount { get; set; } = 0f;
        protected int TargetTick => (int)(Time.Tick - AccumulatedRewindAmount);


        public override void Simulate()
        {
            if (Game.IsClient) return;

            if (Input.Pressed("reload"))
            {
                Mode = Mode == ToolMode.Select ? ToolMode.Rewind : ToolMode.Select;
                Log.Info($"Selected mode: {Mode}");
            }


            if (Mode == ToolMode.Select)
                SelectInputHandle();
            else if (Mode == ToolMode.Rewind)
                RewindInputHandle();

            base.Simulate();
        }

        protected void SelectInputHandle()
        {
            bool pressed = Input.Pressed("attack1") || Input.Pressed("attack2");

            if (!Game.IsServer || !pressed)
                return;

            var tr = DoTrace();

            if (!tr.Hit || !tr.Entity.IsValid())
                return;

            if (tr.Entity is not ModelEntity modelEnt)
                return;

            if (Input.Pressed("attack1"))
            {
                Selected.Add(modelEnt);
                Log.Info($"Selected: {modelEnt}");
            }
            else if (Input.Pressed("attack2") && Selected.Contains(modelEnt))
            {
                Selected.Remove(modelEnt);
                Log.Info($"Unselected: {modelEnt}");
            }

            tr.Entity.Components.GetOrCreate<TrackingEntityComponent>();
        }

        protected void RewindInputHandle()
        {
            bool rewindPress = Input.Down("attack2");
            bool forwardPress = Input.Down("attack1");
            bool noInput = !rewindPress && !forwardPress;

            if (rewindPress)
            {
                AccumulatedRewindAmount += RewindRate;
            }
            else if (forwardPress)
            {
                AccumulatedRewindAmount = Math.Max(AccumulatedRewindAmount - RewindRate, 0);
            }

            if (noInput)
            {
                RewindRate = 0;
            }
            else
            {
                RewindRate = 0.2f; // Or your desired rewind speed
            }
        }




        [GameEvent.Tick.Server]
        protected void Tick()
        {



            TimeLogic();
        }


        public void TimeLogic()
        {

            bool inThePast = AccumulatedRewindAmount > 0;


            DebugOverlay.ScreenText($"Rewind Rate: {RewindRate} | Target Tick: {TargetTick} | InPast: {inThePast}", new Vector2(100, 100));
            DebugOverlay.ScreenText($"Displacement from actual tick: {Time.Tick - TargetTick}  ", new Vector2(100, 120) );

            //Log.Info($"Rewind Rate: {RewindRate} | Target Tick: {TargetTick} | InPast: {inThePast}");

            if (inThePast && RewindRate != 0)
            {
                foreach (var entity in Selected)
                {
                    var tracker = TrackerSystem.Get(entity);

                    if (tracker == null)
                        return;

                    using (var scope = tracker.ScopeByTicks()) /* TargetTick - (int)Math.Abs(RewindRate), TargetTick + (int)Math.Abs(RewindRate))*/
                    {
                        if (RewindRate <= 0)
                        {
                            entity.Position = scope.GetOrPreviousOrDefault(nameof(Entity.Position), TargetTick, entity.Position);
                            entity.Rotation = scope.GetOrPreviousOrDefault(nameof(Entity.Rotation), TargetTick, entity.Rotation);
                        }
                        else
                        {
                            entity.Position = scope.GetOrNextOrDefault(nameof(Entity.Position), TargetTick, entity.Position);
                            entity.Rotation = scope.GetOrNextOrDefault(nameof(Entity.Rotation), TargetTick, entity.Rotation);
                        }
                    }
                }
            }
            else
            {
                Log.Info("In present");
            }
        }
    }
}
