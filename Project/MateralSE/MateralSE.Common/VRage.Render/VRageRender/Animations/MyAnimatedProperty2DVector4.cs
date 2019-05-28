namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageMath;

    public class MyAnimatedProperty2DVector4 : MyAnimatedProperty2D<MyAnimatedPropertyVector4, Vector4, float>
    {
        public MyAnimatedProperty2DVector4(string name) : this(name, null)
        {
        }

        public MyAnimatedProperty2DVector4(string name, MyAnimatedProperty<Vector4>.InterpolatorDelegate interpolator) : base(name, null)
        {
        }

        public override void ApplyVariance(ref Vector4 interpolatedValue, ref float variance, float multiplier, out Vector4 value)
        {
            float randomFloat = MyUtils.GetRandomFloat(1f - variance, 1f + variance);
            value.X = interpolatedValue.X * randomFloat;
            value.Y = interpolatedValue.Y * randomFloat;
            value.Z = interpolatedValue.Z * randomFloat;
            value.W = interpolatedValue.W;
            value.X = MathHelper.Clamp(value.X, 0f, 1f);
            value.Y = MathHelper.Clamp(value.Y, 0f, 1f);
            value.Z = MathHelper.Clamp(value.Z, 0f, 1f);
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            MyAnimatedPropertyVector4 vector = new MyAnimatedPropertyVector4(base.Name, base.m_interpolator2);
            vector.Deserialize(reader);
            value = vector;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedProperty2DVector4 targetProp = new MyAnimatedProperty2DVector4(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public override string ValueType =>
            "Vector4";
    }
}

