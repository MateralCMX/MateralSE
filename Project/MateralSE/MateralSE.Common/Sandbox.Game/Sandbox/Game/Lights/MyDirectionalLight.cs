namespace Sandbox.Game.Lights
{
    using System;
    using VRageMath;

    internal class MyDirectionalLight
    {
        public Vector3 Direction;
        public Vector4 Color;
        public Vector3 BackColor;
        public Vector3 SpecularColor = Vector3.One;
        public float Intensity;
        public float BackIntensity;
        public bool LightOn;

        public void Start()
        {
            this.LightOn = true;
            this.Intensity = 1f;
            this.BackIntensity = 0.1f;
        }

        public void Start(Vector3 direction, Vector4 color, Vector3 backColor)
        {
            this.Start();
            this.Direction = direction;
            this.Color = color;
            this.BackColor = backColor;
        }
    }
}

