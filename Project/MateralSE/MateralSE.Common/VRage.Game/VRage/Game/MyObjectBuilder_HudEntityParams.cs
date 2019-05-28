namespace VRage.Game
{
    using System;
    using System.Xml.Serialization;
    using VRage.Game.Gui;
    using VRage.ObjectBuilders;
    using VRageMath;

    [XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_HudEntityParams : MyObjectBuilder_Base
    {
        public Vector3D Position;
        public long EntityId;
        public string Text;
        public MyHudIndicatorFlagsEnum FlagsEnum;
        public long Owner;
        public MyOwnershipShareModeEnum Share;
        public float BlinkingTime;
    }
}

