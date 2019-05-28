namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageMath;

    public class MyAnimatedPropertyVector4 : MyAnimatedProperty<Vector4>
    {
        public MyAnimatedPropertyVector4()
        {
        }

        public MyAnimatedPropertyVector4(string name) : this(name, null)
        {
        }

        public MyAnimatedPropertyVector4(string name, MyAnimatedProperty<Vector4>.InterpolatorDelegate interpolator) : base(name, false, interpolator)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            Vector4 vector;
            MyUtils.DeserializeValue(reader, out vector);
            value = vector;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedPropertyVector4 targetProp = new MyAnimatedPropertyVector4(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        protected override bool EqualsValues(object value1, object value2) => 
            MyUtils.IsZero(((Vector4) value1) - ((Vector4) value2));

        protected override void Init()
        {
            base.Interpolator = new MyAnimatedProperty<Vector4>.InterpolatorDelegate(MyVector4Interpolator.Lerp);
            base.Init();
        }

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteElementString("W", ((Vector4) value).W.ToString());
            writer.WriteElementString("X", ((Vector4) value).X.ToString());
            writer.WriteElementString("Y", ((Vector4) value).Y.ToString());
            writer.WriteElementString("Z", ((Vector4) value).Z.ToString());
        }

        public override string ValueType =>
            "Vector4";
    }
}

