namespace Sandbox.Game.World
{
    using System;
    using VRageMath;

    internal class MyGodRaysProperties
    {
        public bool Enabled;
        public float Density = 0.34f;
        public float Weight = 1.27f;
        public float Decay = 0.97f;
        public float Exposition = 0.077f;

        public MyGodRaysProperties InterpolateWith(MyGodRaysProperties otherProperties, float interpolator)
        {
            MyGodRaysProperties properties1 = new MyGodRaysProperties();
            properties1.Density = MathHelper.Lerp(this.Density, otherProperties.Density, interpolator);
            properties1.Weight = MathHelper.Lerp(this.Weight, otherProperties.Weight, interpolator);
            properties1.Decay = MathHelper.Lerp(this.Decay, otherProperties.Decay, interpolator);
            properties1.Exposition = MathHelper.Lerp(this.Exposition, otherProperties.Exposition, interpolator);
            properties1.Enabled = MathHelper.Lerp(this.Enabled ? ((float) 1) : ((float) 0), otherProperties.Enabled ? ((float) 1) : ((float) 0), interpolator) > 0.5f;
            return properties1;
        }
    }
}

