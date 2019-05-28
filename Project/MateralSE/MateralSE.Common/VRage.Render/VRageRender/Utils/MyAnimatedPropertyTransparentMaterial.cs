namespace VRageRender.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using VRage.Utils;
    using VRageRender;
    using VRageRender.Animations;

    public class MyAnimatedPropertyTransparentMaterial : MyAnimatedProperty<MyTransparentMaterial>
    {
        public MyAnimatedPropertyTransparentMaterial()
        {
        }

        public MyAnimatedPropertyTransparentMaterial(string name) : this(name, null)
        {
        }

        public MyAnimatedPropertyTransparentMaterial(string name, MyAnimatedProperty<MyTransparentMaterial>.InterpolatorDelegate interpolator) : base(name, false, interpolator)
        {
        }

        public override void DeserializeValue(XmlReader reader, out object value)
        {
            base.DeserializeValue(reader, out value);
            value = MyTransparentMaterials.GetMaterial(MyStringId.GetOrCompute((string) value));
        }

        public override IMyConstProperty Duplicate()
        {
            MyAnimatedPropertyTransparentMaterial targetProp = new MyAnimatedPropertyTransparentMaterial(base.Name);
            this.Duplicate(targetProp);
            return targetProp;
        }

        protected override bool EqualsValues(object value1, object value2) => 
            (((MyTransparentMaterial) value1).Id.String == ((MyTransparentMaterial) value2).Id.String);

        protected override void Init()
        {
            base.Interpolator = new MyAnimatedProperty<MyTransparentMaterial>.InterpolatorDelegate(MyTransparentMaterialInterpolator.Switch);
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

