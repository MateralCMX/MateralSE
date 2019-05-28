namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("AnimationControllerDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationControllerDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(15), XmlArrayItem("Layer")]
        public MyObjectBuilder_AnimationLayer[] Layers;
        [ProtoMember(0x13), XmlArrayItem("StateMachine")]
        public MyObjectBuilder_AnimationSM[] StateMachines;
        [ProtoMember(0x17), XmlArrayItem("FootIkChain")]
        public MyObjectBuilder_AnimationFootIkChain[] FootIkChains;
        [ProtoMember(0x1b), XmlArrayItem("Bone")]
        public string[] IkIgnoredBones;

        public MyObjectBuilder_AnimationControllerDefinition()
        {
            base.Id.TypeId = typeof(MyObjectBuilder_AnimationControllerDefinition);
        }
    }
}

