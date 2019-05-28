namespace Sandbox.Common.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("VR.PhysicalModelCollectionDefinition")]
    public class MyObjectBuilder_PhysicalModelCollectionDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x20), XmlArrayItem("Item")]
        public MyPhysicalModelItem[] Items;
    }
}

