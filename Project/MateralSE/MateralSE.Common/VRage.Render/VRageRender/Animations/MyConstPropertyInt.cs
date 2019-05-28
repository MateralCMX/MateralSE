namespace VRageRender.Animations
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyConstPropertyInt : MyConstProperty<int>
    {
        public MyConstPropertyInt()
        {
        }

        public MyConstPropertyInt(string name) : base(name)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public override IMyConstProperty Duplicate()
        {
            MyConstPropertyInt targetProp = new MyConstPropertyInt(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public static implicit operator int(MyConstPropertyInt f) => 
            f.GetValue<int>();

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(((int) value).ToString(CultureInfo.InvariantCulture));
        }

        public override string ValueType =>
            "Int";
    }
}

