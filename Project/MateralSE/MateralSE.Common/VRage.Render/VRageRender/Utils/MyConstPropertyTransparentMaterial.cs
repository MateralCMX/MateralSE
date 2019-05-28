namespace VRageRender.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageRender;
    using VRageRender.Animations;

    public class MyConstPropertyTransparentMaterial : MyConstProperty<MyTransparentMaterial>
    {
        public MyConstPropertyTransparentMaterial()
        {
        }

        public MyConstPropertyTransparentMaterial(string name) : base(name)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = MyTransparentMaterials.GetMaterial(MyStringId.GetOrCompute((string) value));
        }

        public override IMyConstProperty Duplicate()
        {
            MyConstPropertyTransparentMaterial targetProp = new MyConstPropertyTransparentMaterial(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        protected override void Init()
        {
            base.Init();
        }

        public override void SerializeValue(XmlWriter writer, object value)
        {
            writer.WriteValue(((MyTransparentMaterial) value).Id.String);
        }

        public override string ValueType =>
            "String";

        public override string BaseValueType =>
            "MyTransparentMaterial";
    }
}

