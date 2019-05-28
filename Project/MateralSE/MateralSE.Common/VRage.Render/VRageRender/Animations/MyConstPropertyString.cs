namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyConstPropertyString : MyConstProperty<string>
    {
        public MyConstPropertyString()
        {
        }

        public MyConstPropertyString(string name) : base(name)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = value.ToString();
        }

        public override IMyConstProperty Duplicate()
        {
            MyConstPropertyString targetProp = new MyConstPropertyString(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public static implicit operator string(MyConstPropertyString f) => 
            f.GetValue<string>();

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue((string) value);
        }

        public override string ValueType =>
            "String";
    }
}

