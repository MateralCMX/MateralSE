namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ImageStatVisualStyle : MyObjectBuilder_StatVisualStyle
    {
        public MyStringHash Texture;
        public Vector4? ColorMask;
    }
}

