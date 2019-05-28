namespace VRage.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TreeDefinition : MyObjectBuilder_EnvironmentItemDefinition
    {
        [ProtoMember(13)]
        public float BranchesStartHeight;
        [ProtoMember(0x10)]
        public float HitPoints = 100f;
        [ProtoMember(0x13)]
        public string CutEffect;
        [ProtoMember(0x16)]
        public string FallSound;
        [ProtoMember(0x19)]
        public string BreakSound;
    }
}

