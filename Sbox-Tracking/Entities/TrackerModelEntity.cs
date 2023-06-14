using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public class TrackerModelEntity : ModelEntity
    {
        protected T GetProperty<T>(string propertyName, T propertyValue)
        {
            if (Tracker?.IsScoped ?? false)
            {
                if (Tracker.KeyExistsInTracker(propertyName))
                {
                    return Tracker.GetPropertyOrLast<T>(propertyName);
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

        #region Entity

        // Issue: This should be overridable?
        public new float Health
        {
            get => GetProperty(nameof(Health), base.Health);
            set => SetProperty(nameof(Health), value, v => base.Health = v);
        }

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

    }
}
