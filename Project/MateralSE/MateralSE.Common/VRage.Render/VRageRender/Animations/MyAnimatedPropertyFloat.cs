namespace VRageRender.Animations
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;

    public class MyAnimatedPropertyFloat : MyAnimatedProperty<float>
    {
        public MyAnimatedPropertyFloat()
        {
        }

        public MyAnimatedPropertyFloat(string name) : this(name, false, null)
        {
        }

        public MyAnimatedPropertyFloat(string name, bool interpolateAfterEnd, MyAnimatedProperty<float>.InterpolatorDelegate interpolator) : base(name, interpolateAfterEnd, interpolator)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedPropertyFloat targetProp = new MyAnimatedPropertyFloat(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        protected override bool EqualsValues(object value1, object value2) => 
            MyUtils.IsZero((float) (((float) value1) - ((float) value2)), 1E-05f);

        protected override void Init()
        {
            base.Interpolator = new MyAnimatedProperty<float>.InterpolatorDelegate(MyFloatInterpolator.Lerp);
            base.Init();
        }

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(((float) value).ToString(CultureInfo.InvariantCulture));
        }

        public override string ValueType =>
            "Float";
    }
}

