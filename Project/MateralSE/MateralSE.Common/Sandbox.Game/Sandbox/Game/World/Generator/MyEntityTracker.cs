namespace Sandbox.Game.World.Generator
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRageMath;

    public class MyEntityTracker
    {
        public BoundingSphereD BoundingVolume = new BoundingSphereD(Vector3D.PositiveInfinity, 0.0);

        public MyEntityTracker(MyEntity entity, double radius)
        {
            this.Entity = entity;
            this.Radius = radius;
        }

        public bool ShouldGenerate()
        {
            if (this.Entity.Closed || !this.Entity.Save)
            {
                return false;
            }
            return ((this.CurrentPosition - this.LastPosition).Length() > this.Tolerance);
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.Entity, ", ", this.BoundingVolume, ", ", this.Tolerance };
            return string.Concat(objArray1);
        }

        public void UpdateLastPosition()
        {
            this.LastPosition = this.CurrentPosition;
        }

        public MyEntity Entity { get; private set; }

        public Vector3D CurrentPosition =>
            this.Entity.PositionComp.WorldAABB.Center;

        public Vector3D LastPosition
        {
            get => 
                this.BoundingVolume.Center;
            private set => 
                (this.BoundingVolume.Center = value);
        }

        public double Radius
        {
            get => 
                this.BoundingVolume.Radius;
            set
            {
                this.Tolerance = MathHelper.Clamp((double) (value / 2.0), (double) 128.0, (double) 512.0);
                this.BoundingVolume.Radius = value + this.Tolerance;
            }
        }

        public double Tolerance { get; private set; }
    }
}

