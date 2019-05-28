namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ModStorageComponentDefinition : MyObjectBuilder_ComponentDefinitionBase
    {
        [ProtoMember(13), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultValueOrEmpty | MyObjectFlags.DefaultZero)]
        public Guid[] RegisteredStorageGuids;
    }
}

