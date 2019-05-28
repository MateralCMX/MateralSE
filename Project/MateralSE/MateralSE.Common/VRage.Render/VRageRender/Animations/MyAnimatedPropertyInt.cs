namespace VRageRender.Animations
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyAnimatedPropertyInt : MyAnimatedProperty<int>
    {
        public MyAnimatedPropertyInt()
        {
        }

        public MyAnimatedPropertyInt(string name) : this(name, null)
        {
        }

        public MyAnimatedPropertyInt(string name, MyAnimatedProperty<int>.InterpolatorDelegate interpolator) : base(name, false, interpolator)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedPropertyInt targetProp = new MyAnimatedPropertyInt(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        protected override bool EqualsValues(object value1, object value2) => 
            (((int) value1) == ((int) value2));

        protected override void Init()
        {
            base.Interpolator = new MyAnimatedProperty<int>.InterpolatorDelegate(MyIntInterpolator.Lerp);
            base.Init();
        }

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(((int) value).ToString(CultureInfo.InvariantCulture));
        }

        public override string ValueType =>
            "Int";
    }
}

