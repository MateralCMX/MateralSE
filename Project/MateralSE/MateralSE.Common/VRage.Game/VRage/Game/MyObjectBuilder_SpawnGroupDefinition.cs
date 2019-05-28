namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SpawnGroupDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x39), DefaultValue((float) 1f)]
        public float Frequency = 1f;
        [ProtoMember(60), XmlArrayItem("Prefab")]
        public SpawnGroupPrefab[] Prefabs;
        [ProtoMember(0x40), XmlArrayItem("Voxel")]
        public SpawnGroupVoxel[] Voxels;
        [ProtoMember(0x44), DefaultValue(false)]
        public bool IsEncounter;
        [ProtoMember(0x47), DefaultValue(false)]
        public bool IsPirate;
        [ProtoMember(0x4a), DefaultValue(false)]
        public bool IsCargoShip;
        [ProtoMember(0x4d), DefaultValue(false)]
        public bool ReactorsOn;

        [ProtoContract]
        public class SpawnGroupPrefab
        {
            [XmlAttribute, ProtoMember(0x12)]
            public string SubtypeId;
            [ProtoMember(0x15)]
            public Vector3 Position;
            [ProtoMember(0x18), DefaultValue("")]
            public string BeaconText = "";
            [ProtoMember(0x1b), DefaultValue((float) 10f)]
            public float Speed = 10f;
            [ProtoMember(30), DefaultValue(false)]
            public bool PlaceToGridOrigin;
            [ProtoMember(0x21)]
            public bool ResetOwnership = true;
            [ProtoMember(0x24)]
            public string Behaviour;
            [ProtoMember(0x27)]
            public float BehaviourActivationDistance = 1000f;
        }

        [ProtoContract]
        public class SpawnGroupVoxel
        {
            [XmlAttribute, ProtoMember(0x2f)]
            public string StorageName;
            [ProtoMember(50)]
            public Vector3 Offset;
            [ProtoMember(0x35, IsRequired=false)]
            public bool CenterOffset;
        }
    }
}

