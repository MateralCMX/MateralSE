namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Player : MyObjectBuilder_Base
    {
        [ProtoMember(0x21), Serialize(MyObjectFlags.DefaultZero)]
        public string DisplayName;
        [ProtoMember(0x25)]
        public long IdentityId;
        [ProtoMember(40)]
        public bool Connected;
        [ProtoMember(0x2b)]
        public bool ForceRealPlayer;
        [ProtoMember(0x2e), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_Toolbar Toolbar;
        [ProtoMember(50), Serialize(MyObjectFlags.DefaultZero)]
        public List<CameraControllerSettings> EntityCameraData;
        [ProtoMember(0x36), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public List<Vector3> BuildColorSlots;
        [NoSerialize]
        public ulong SteamID;
        [NoSerialize]
        private SerializableDictionary<long, CameraControllerSettings> m_cameraData;
        [NoSerialize]
        public long PlayerEntity;
        [NoSerialize]
        public string PlayerModel;
        [NoSerialize]
        public long PlayerId;
        [NoSerialize]
        public long LastActivity;

        public bool ShouldSerializeBuildColorSlots() => 
            (this.BuildColorSlots != null);

        public bool ShouldSerializeCameraData() => 
            false;

        public bool ShouldSerializeLastActivity() => 
            false;

        public bool ShouldSerializePlayerEntity() => 
            false;

        public bool ShouldSerializePlayerId() => 
            false;

        public bool ShouldSerializePlayerModel() => 
            false;

        public bool ShouldSerializeSteamID() => 
            false;

        [NoSerialize]
        public SerializableDictionary<long, CameraControllerSettings> CameraData
        {
            get => 
                this.m_cameraData;
            set => 
                (this.m_cameraData = value);
        }
    }
}

