namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VisualScript : MyObjectBuilder_Base
    {
        [Nullable, ProtoMember(15)]
        public string Interface;
        [ProtoMember(0x12)]
        public List<string> DependencyFilePaths;
        [ProtoMember(0x15), DynamicObjectBuilder(false), XmlArrayItem("MyObjectBuilder_ScriptNode", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_ScriptNode>))]
        public List<MyObjectBuilder_ScriptNode> Nodes;
        [ProtoMember(0x1a)]
        public string Name;
    }
}

