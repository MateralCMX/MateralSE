namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TextStatVisualStyle : MyObjectBuilder_StatVisualStyle
    {
        public string Text;
        public float Scale;
        public string Font;
        public Vector4? ColorMask;
        public MyGuiDrawAlignEnum? TextAlign;
    }
}

