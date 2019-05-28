namespace Sandbox.Game.World
{
    using System;
    using VRageMath;

    internal class MyImpostorProperties
    {
        public bool Enabled = true;
        public int ImpostorType;
        public int? Material;
        public int ImpostorsCount;
        public float MinDistance;
        public float MaxDistance;
        public float MinRadius;
        public float MaxRadius;
        public Vector4 AnimationSpeed;
        public Vector3 Color;
        public float Intensity;
        public float Contrast;

        public float Radius
        {
            get => 
                this.MaxRadius;
            set
            {
                this.MinRadius = value;
                this.MaxRadius = value;
            }
        }

        public float Anim1
        {
            get => 
                this.AnimationSpeed.X;
            set => 
                (this.AnimationSpeed.X = value);
        }

        public float Anim2
        {
            get => 
                this.AnimationSpeed.Y;
            set => 
                (this.AnimationSpeed.Y = value);
        }

        public float Anim3
        {
            get => 
                this.AnimationSpeed.Z;
            set => 
                (this.AnimationSpeed.Z = value);
        }

        public float Anim4
        {
            get => 
                this.AnimationSpeed.W;
            set => 
                (this.AnimationSpeed.W = value);
        }
    }
}

