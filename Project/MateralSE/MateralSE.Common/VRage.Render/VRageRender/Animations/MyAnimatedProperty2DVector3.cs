namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageMath;

    public class MyAnimatedProperty2DVector3 : MyAnimatedProperty2D<MyAnimatedPropertyVector3, Vector3, Vector3>
    {
        public MyAnimatedProperty2DVector3(string name) : this(name, null)
        {
        }

        public MyAnimatedProperty2DVector3(string name, MyAnimatedProperty<Vector3>.InterpolatorDelegate interpolator) : base(name, interpolator)
        {
        }

        public override void ApplyVariance(ref Vector3 interpolatedValue, ref Vector3 variance, float multiplier, out Vector3 value)
        {
            if ((variance != Vector3.Zero) || (multiplier != 1f))
            {
                value.X = MyUtils.GetRandomFloat(interpolatedValue.X - variance.X, interpolatedValue.X + variance.X) * multiplier;
                value.Y = MyUtils.GetRandomFloat(interpolatedValue.Y - variance.Y, interpolatedValue.Y + variance.Y) * multiplier;
                value.Z = MyUtils.GetRandomFloat(interpolatedValue.Z - variance.Z, interpolatedValue.Z + variance.Z) * multiplier;
            }
            value = interpolatedValue;
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            MyAnimatedPropertyVector3 vector = new MyAnimatedPropertyVector3(base.Name, false, base.m_interpolator2);
            vector.Deserialize(reader);
            value = vector;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedProperty2DVector3 targetProp = new MyAnimatedProperty2DVector3(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public override string ValueType =>
            "Vector3";
    }
}

