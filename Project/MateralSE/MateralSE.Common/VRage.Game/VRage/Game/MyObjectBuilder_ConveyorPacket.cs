namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ConveyorPacket : MyObjectBuilder_Base
    {
        [ProtoMember(12)]
        public MyObjectBuilder_InventoryItem Item;
        [ProtoMember(15), DefaultValue(0)]
        public int LinePosition;

        public bool ShouldSerializeLinePosition() => 
            (this.LinePosition != 0);
    }
}

