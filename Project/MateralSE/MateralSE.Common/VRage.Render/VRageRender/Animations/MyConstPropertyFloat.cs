namespace VRageRender.Animations
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyConstPropertyFloat : MyConstProperty<float>
    {
        public MyConstPropertyFloat()
        {
        }

        public MyConstPropertyFloat(string name) : base(name)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        public override IMyConstProperty Duplicate()
        {
            MyConstPropertyFloat targetProp = new MyConstPropertyFloat(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public static implicit operator float(MyConstPropertyFloat f) => 
            f.GetValue<float>();

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(((float) value).ToString(CultureInfo.InvariantCulture));
        }

        public override string ValueType =>
            "Float";
    }
}

