namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SessionComponent : MyObjectBuilder_Base
    {
        public bool ShouldSerializeDefinition() => 
            (this.Definition != null);

        public SerializableDefinitionId? Definition { get; set; }
    }
}

