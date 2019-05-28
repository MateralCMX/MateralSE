namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CompositeBlueprintDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(12), XmlArrayItem("Blueprint")]
        public BlueprintItem[] Blueprints;
    }
}

