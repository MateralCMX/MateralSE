namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((System.Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_EmissiveColorStatePresetDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(14), XmlArrayItem("EmissiveState")]
        public EmissiveStateDefinition[] EmissiveStates;
    }
}

