namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;

    public class MyAnimatedProperty2DFloat : MyAnimatedProperty2D<MyAnimatedPropertyFloat, float, float>
    {
        public MyAnimatedProperty2DFloat(string name) : this(name, null)
        {
        }

        public MyAnimatedProperty2DFloat(string name, MyAnimatedProperty<float>.InterpolatorDelegate interpolator) : base(name, interpolator)
        {
        }

        public override void ApplyVariance(ref float interpolatedValue, ref float variance, float multiplier, out float value)
        {
            if ((variance != 0f) || (multiplier != 1f))
            {
                interpolatedValue = MyUtils.GetRandomFloat(interpolatedValue - variance, interpolatedValue + variance) * multiplier;
            }
            value = interpolatedValue;
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            MyAnimatedPropertyFloat num = new MyAnimatedPropertyFloat(base.Name, false, base.m_interpolator2);
            num.Deserialize(reader);
            value = num;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedProperty2DFloat targetProp = new MyAnimatedProperty2DFloat(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public override string ValueType =>
            "Float";
    }
}

