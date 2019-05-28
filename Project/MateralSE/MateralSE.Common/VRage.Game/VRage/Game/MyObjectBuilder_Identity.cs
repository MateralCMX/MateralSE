namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Identity : MyObjectBuilder_Base
    {
        [ProtoMember(0x19)]
        public long IdentityId;
        [ProtoMember(0x1c), Serialize(MyObjectFlags.DefaultZero)]
        public string DisplayName;
        [ProtoMember(0x20)]
        public long CharacterEntityId;
        [ProtoMember(0x23), Serialize(MyObjectFlags.DefaultZero)]
        public string Model;
        [ProtoMember(0x27)]
        public SerializableVector3? ColorMask;
        [ProtoMember(0x2b)]
        public int BlockLimitModifier;
        [ProtoMember(0x2e)]
        public DateTime LastLoginTime;
        [ProtoMember(0x31, IsRequired=false)]
        public HashSet<long> SavedCharacters;
        [ProtoMember(0x34)]
        public DateTime LastLogoutTime;
        [ProtoMember(0x37)]
        public List<long> RespawnShips;
        [ProtoMember(0x3a, IsRequired=false)]
        public Vector3D? LastDeathPosition;

        public bool ShouldSerializeColorMask() => 
            (this.ColorMask != null);

        public bool ShouldSerializePlayerId() => 
            false;

        [NoSerialize]
        public long PlayerId
        {
            get => 
                this.IdentityId;
            set => 
                (this.IdentityId = value);
        }
    }
}

