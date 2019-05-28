namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_InventoryBagEntity : MyObjectBuilder_EntityBase
    {
        [ProtoMember(11)]
        public SerializableVector3 LinearVelocity;
        [ProtoMember(14)]
        public SerializableVector3 AngularVelocity;
        [ProtoMember(0x11)]
        public float Mass = 5f;
        [ProtoMember(20)]
        public long OwnerIdentityId;
    }
}

