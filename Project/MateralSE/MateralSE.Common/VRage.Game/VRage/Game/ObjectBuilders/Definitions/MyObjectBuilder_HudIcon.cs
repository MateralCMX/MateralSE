namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game.GUI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_HudIcon : MyObjectBuilder_Base
    {
        public Vector2 Position;
        public MyGuiDrawAlignEnum? OriginAlign;
        public Vector2? Size;
        public MyStringHash Texture;
        public MyAlphaBlinkBehavior Blink;
    }
}

