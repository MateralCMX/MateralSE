namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DurabilityComponentDefinition : MyObjectBuilder_ComponentDefinitionBase
    {
        [ProtoMember(0x18)]
        public float DefaultHitDamage = 0.01f;
        [ProtoMember(0x1b), XmlArrayItem("Hit")]
        public HitDefinition[] DefinedHits;
        [ProtoMember(30)]
        public string ParticleEffect;
        [ProtoMember(0x21)]
        public string SoundCue;
        [ProtoMember(0x24)]
        public float DamageOverTime;

        [ProtoContract]
        public class HitDefinition
        {
            [ProtoMember(0x10), DefaultValue((string) null), XmlAttribute]
            public string Action;
            [ProtoMember(0x12), DefaultValue((string) null), XmlAttribute]
            public string Material;
            [ProtoMember(20), XmlAttribute]
            public float Damage = 0.01f;
        }
    }
}

