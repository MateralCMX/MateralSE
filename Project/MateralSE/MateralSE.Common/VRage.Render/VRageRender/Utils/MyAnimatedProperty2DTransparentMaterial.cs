namespace VRageRender.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRageRender;
    using VRageRender.Animations;

    public class MyAnimatedProperty2DTransparentMaterial : MyAnimatedProperty2D<MyAnimatedPropertyTransparentMaterial, MyTransparentMaterial, int>
    {
        public MyAnimatedProperty2DTransparentMaterial(string name) : this(name, null)
        {
        }

        public MyAnimatedProperty2DTransparentMaterial(string name, MyAnimatedProperty<MyTransparentMaterial>.InterpolatorDelegate interpolator) : base(name, interpolator)
        {
        }

        public override void ApplyVariance(ref MyTransparentMaterial interpolatedValue, ref int variance, float multiplier, out MyTransparentMaterial value)
        {
            value = interpolatedValue;
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            MyAnimatedPropertyTransparentMaterial material = new MyAnimatedPropertyTransparentMaterial(base.Name, base.m_interpolator2);
            material.Deserialize(reader);
            value = material;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedProperty2DTransparentMaterial targetProp = new MyAnimatedProperty2DTransparentMaterial(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public override string ValueType =>
            "String";

        public override string BaseValueType =>
            "MyTransparentMaterial";
    }
}

