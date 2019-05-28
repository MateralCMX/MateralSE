namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AmmoDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x58)]
        public AmmoBasicProperties BasicProperties;

        [ProtoContract]
        public class AmmoBasicProperties
        {
            [ProtoMember(0x4a)]
            public float DesiredSpeed;
            [ProtoMember(0x4c)]
            public float SpeedVariance;
            [ProtoMember(0x4e)]
            public float MaxTrajectory;
            [ProtoMember(80), DefaultValue(false)]
            public bool IsExplosive;
            [ProtoMember(0x52), DefaultValue((float) 0f)]
            public float BackkickForce;
            [ProtoMember(0x54)]
            public string PhysicalMaterial = "";
        }
    }
}

