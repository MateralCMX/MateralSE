namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageMath;

    public class MyConstPropertyVector3 : MyConstProperty<Vector3>
    {
        public MyConstPropertyVector3()
        {
        }

        public MyConstPropertyVector3(string name) : base(name)
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
            MyConstPropertyVector3 targetProp = new MyConstPropertyVector3(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        public static implicit operator Vector3(MyConstPropertyVector3 f) => 
            f.GetValue<Vector3>();

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

