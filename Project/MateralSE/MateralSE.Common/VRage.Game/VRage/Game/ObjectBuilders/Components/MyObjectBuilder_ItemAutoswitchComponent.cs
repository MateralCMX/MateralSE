namespace VRage.Game.ObjectBuilders.Components
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ItemAutoswitchComponent : MyObjectBuilder_SessionComponent
    {
        [ProtoMember(12)]
        public SerializableDefinitionId? AutoswitchTargetDefinition;
    }
}

