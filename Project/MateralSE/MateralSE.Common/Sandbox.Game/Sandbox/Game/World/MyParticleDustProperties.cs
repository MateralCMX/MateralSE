namespace Sandbox.Game.World
{
    using System;
    using VRageMath;

    internal class MyParticleDustProperties
    {
        public bool Enabled;
        public float DustBillboardRadius = 3f;
        public float DustFieldCountInDirectionHalf = 5f;
        public float DistanceBetween = 180f;
        public float AnimSpeed = 0.004f;
        public VRageMath.Color Color = VRageMath.Color.White;
        public int Texture;

        public MyParticleDustProperties InterpolateWith(MyParticleDustProperties otherProperties, float interpolator)
        {
            MyParticleDustProperties properties1 = new MyParticleDustProperties();
            properties1.DustFieldCountInDirectionHalf = MathHelper.Lerp(this.DustFieldCountInDirectionHalf, otherProperties.DustFieldCountInDirectionHalf, interpolator);
            properties1.DistanceBetween = MathHelper.Lerp(this.DistanceBetween, otherProperties.DistanceBetween, interpolator);
            properties1.AnimSpeed = MathHelper.Lerp(this.AnimSpeed, otherProperties.AnimSpeed, interpolator);
            properties1.Color = VRageMath.Color.Lerp(this.Color, otherProperties.Color, interpolator);
            properties1.Enabled = MathHelper.Lerp(this.Enabled ? ((float) 1) : ((float) 0), otherProperties.Enabled ? ((float) 1) : ((float) 0), interpolator) > 0.5f;
            MyParticleDustProperties local3 = properties1;
            MyParticleDustProperties local4 = properties1;
            local4.DustBillboardRadius = (interpolator <= 0.5f) ? this.DustBillboardRadius : otherProperties.DustBillboardRadius;
            float local1 = (float) local4;
            float local2 = (float) local4;
            local2.Texture = (interpolator <= 0.5f) ? this.Texture : otherProperties.Texture;
            return (MyParticleDustProperties) local2;
        }
    }
}

