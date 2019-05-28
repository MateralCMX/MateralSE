namespace VRageRender.Animations
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class MyConstPropertyGenerationIndex : MyConstPropertyInt
    {
        public MyConstPropertyGenerationIndex()
        {
        }

        public MyConstPropertyGenerationIndex(string name) : base(name)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public override IMyConstProperty Duplicate()
        {
            MyConstPropertyGenerationIndex targetProp = new MyConstPropertyGenerationIndex(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(((int) value).ToString(CultureInfo.InvariantCulture));
        }

        public override string BaseValueType =>
            "GenerationIndex";
    }
}

