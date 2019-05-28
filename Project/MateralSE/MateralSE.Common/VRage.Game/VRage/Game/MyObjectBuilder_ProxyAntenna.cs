namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ProxyAntenna : MyObjectBuilder_EntityBase
    {
        public bool HasReceiver;
        public bool IsLaser;
        public bool IsCharacter;
        public SerializableVector3D Position;
        public float BroadcastRadius;
        public List<MyObjectBuilder_HudEntityParams> HudParams;
        public long Owner;
        public MyOwnershipShareModeEnum Share;
        public long InfoEntityId;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string InfoName;
        public long AntennaEntityId;
        [Serialize(MyObjectFlags.DefaultZero)]
        public long? SuccessfullyContacting;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string StateText;
        public bool HasRemote;
        [Serialize(MyObjectFlags.DefaultZero)]
        public long? MainRemoteOwner;
        [Serialize(MyObjectFlags.DefaultZero)]
        public long? MainRemoteId;
        public MyOwnershipShareModeEnum MainRemoteSharing;
    }
}

