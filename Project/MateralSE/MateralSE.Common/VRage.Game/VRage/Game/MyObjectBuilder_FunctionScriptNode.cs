namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((System.Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FunctionScriptNode : MyObjectBuilder_ScriptNode
    {
        public int Version;
        public string DeclaringType = string.Empty;
        public string Type = string.Empty;
        public string ExtOfType = string.Empty;
        public int SequenceInputID = -1;
        public int SequenceOutputID = -1;
        public MyVariableIdentifier InstanceInputID = MyVariableIdentifier.Default;
        public List<MyVariableIdentifier> InputParameterIDs = new List<MyVariableIdentifier>();
        public List<IdentifierList> OutputParametersIDs = new List<IdentifierList>();
        public List<MyParameterValue> InputParameterValues = new List<MyParameterValue>();
    }
}

