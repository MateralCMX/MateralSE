namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DeviceBase : MyObjectBuilder_Base
    {
        [ProtoMember(11)]
        public uint? InventoryItemId;

        public bool ShouldSerializeInventoryItemId() => 
            (this.InventoryItemId != null);
    }
}

