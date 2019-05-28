namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CompositeTexture : MyObjectBuilder_Base
    {
        public MyStringHash LeftTop = MyStringHash.NullOrEmpty;
        public MyStringHash LeftCenter = MyStringHash.NullOrEmpty;
        public MyStringHash LeftBottom = MyStringHash.NullOrEmpty;
        public MyStringHash CenterTop = MyStringHash.NullOrEmpty;
        public MyStringHash Center = MyStringHash.NullOrEmpty;
        public MyStringHash CenterBottom = MyStringHash.NullOrEmpty;
        public MyStringHash RightTop = MyStringHash.NullOrEmpty;
        public MyStringHash RightCenter = MyStringHash.NullOrEmpty;
        public MyStringHash RightBottom = MyStringHash.NullOrEmpty;

        public virtual bool IsValid() => 
            ((this.LeftTop != MyStringHash.NullOrEmpty) || ((this.LeftTop != MyStringHash.NullOrEmpty) || ((this.LeftCenter != MyStringHash.NullOrEmpty) || ((this.LeftBottom != MyStringHash.NullOrEmpty) || ((this.CenterTop != MyStringHash.NullOrEmpty) || ((this.Center != MyStringHash.NullOrEmpty) || ((this.CenterBottom != MyStringHash.NullOrEmpty) || ((this.RightTop != MyStringHash.NullOrEmpty) || ((this.RightCenter != MyStringHash.NullOrEmpty) || (this.RightBottom != MyStringHash.NullOrEmpty))))))))));
    }
}

