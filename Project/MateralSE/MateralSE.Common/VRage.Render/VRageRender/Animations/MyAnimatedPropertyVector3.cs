namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageMath;

    public class MyAnimatedPropertyVector3 : MyAnimatedProperty<Vector3>
    {
        public MyAnimatedPropertyVector3()
        {
        }

        public MyAnimatedPropertyVector3(string name) : this(name, false, null)
        {
        }

        public MyAnimatedPropertyVector3(string name, bool interpolateAfterEnd, MyAnimatedProperty<Vector3>.InterpolatorDelegate interpolator) : base(name, interpolateAfterEnd, interpolator)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            Vector3 vector;
            MyUtils.DeserializeValue(reader, out vector);
            value = vector;
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedPropertyVector3 targetProp = new MyAnimatedPropertyVector3(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        protected override bool EqualsValues(object value1, object value2) => 
            MyUtils.IsZero(((Vector3) value1) - ((Vector3) value2), 1E-05f);

        protected override void Init()
        {
            base.Interpolator = new MyAnimatedProperty<Vector3>.InterpolatorDelegate(MyVector3Interpolator.Lerp);
            base.Init();
        }

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteElementString("X", ((Vector3) value).X.ToString());
            writer.WriteElementString("Y", ((Vector3) value).Y.ToString());
            writer.WriteElementString("Z", ((Vector3) value).Z.ToString());
        }

        public override string ValueType =>
            "Vector3";
    }
}

