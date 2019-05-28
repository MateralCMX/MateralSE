namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyConstPropertyBool : MyConstProperty<bool>
    {
        public MyConstPropertyBool()
        {
        }

        public MyConstPropertyBool(string name) : base(name)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = Convert.ToBoolean(value);
        }

        public override IMyConstProperty Duplicate()
        {
            MyConstPropertyBool targetProp = new MyConstPropertyBool(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public static implicit operator bool(MyConstPropertyBool f) => 
            ((f == null) ? false : f.GetValue<bool>());

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(value.ToString().ToLower());
        }

        public override string ValueType =>
            "Bool";
    }
}

