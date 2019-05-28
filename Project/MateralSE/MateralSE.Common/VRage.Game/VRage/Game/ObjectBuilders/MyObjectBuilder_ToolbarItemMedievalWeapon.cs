namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ToolbarItemMedievalWeapon : MyObjectBuilder_ToolbarItemWeapon
    {
        [ProtoMember(0x10)]
        public uint? ItemId;
    }
}

