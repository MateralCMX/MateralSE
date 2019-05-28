namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;

    public class MyAnimatedProperty2DInt : MyAnimatedProperty2D<MyAnimatedPropertyInt, int, int>
    {
        public MyAnimatedProperty2DInt(string name) : this(name, null)
        {
        }

        public MyAnimatedProperty2DInt(string name, MyAnimatedProperty<int>.InterpolatorDelegate interpolator) : base(name, interpolator)
        {
        }

        public override void ApplyVariance(ref int interpolatedValue, ref int variance, float multiplier, out int value)
        {
            if ((variance != 0) || (multiplier != 1f))
            {
                interpolatedValue = (int) (MyUtils.GetRandomInt(interpolatedValue - variance, interpolatedValue + variance) * multiplier);
            }
            value = interpolatedValue;
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            MyAnimatedPropertyInt num = new MyAnimatedPropertyInt(base.Name, base.m_interpolator2);
            num.Deserialize(reader);
            value = num;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedProperty2DInt targetProp = new MyAnimatedProperty2DInt(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public override string ValueType =>
            "Int";
    }
}

