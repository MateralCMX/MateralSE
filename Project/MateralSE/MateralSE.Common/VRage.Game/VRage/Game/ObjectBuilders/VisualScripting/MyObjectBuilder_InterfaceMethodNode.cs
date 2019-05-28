namespace VRage.Game.ObjectBuilders.VisualScripting
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_InterfaceMethodNode : MyObjectBuilder_ScriptNode
    {
        public string MethodName;
        public List<int> SequenceOutputIDs = new List<int>();
        public List<IdentifierList> OutputIDs = new List<IdentifierList>();
        public List<string> OutputNames = new List<string>();
        public List<string> OuputTypes = new List<string>();
    }
}

