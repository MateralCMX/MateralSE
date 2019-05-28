namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageMath;

    public class MyConstPropertyVector4 : MyConstProperty<Vector4>
    {
        public MyConstPropertyVector4()
        {
        }

        public MyConstPropertyVector4(string name) : base(name)
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
            MyConstPropertyVector4 targetProp = new MyConstPropertyVector4(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public static implicit operator Vector4(MyConstPropertyVector4 f) => 
            f.GetValue<Vector4>();

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

