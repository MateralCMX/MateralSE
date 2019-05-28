namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlType("Transform")]
    public class MyObjectBuilder_WorldGeneratorPlayerStartingState_Transform : MyObjectBuilder_WorldGeneratorPlayerStartingState
    {
        [ProtoMember(0xab)]
        public MyPositionAndOrientation? Transform;
        [ProtoMember(0xaf), XmlAttribute]
        public bool JetpackEnabled;
        [ProtoMember(0xb2), XmlAttribute]
        public bool DampenersEnabled;

        public bool ShouldSerializeTransform() => 
            (this.Transform != null);
    }
}

