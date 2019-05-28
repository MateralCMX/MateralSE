namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_EventScriptNode : MyObjectBuilder_ScriptNode
    {
        public string Name;
        public int SequenceOutputID = -1;
        public List<IdentifierList> OutputIDs = new List<IdentifierList>();
        public List<string> OutputNames = new List<string>();
        public List<string> OuputTypes = new List<string>();
    }
}

