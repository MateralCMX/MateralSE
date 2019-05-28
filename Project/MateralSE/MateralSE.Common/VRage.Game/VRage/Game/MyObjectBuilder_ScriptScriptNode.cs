namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ScriptScriptNode : MyObjectBuilder_ScriptNode
    {
        public string Name = string.Empty;
        public string Path;
        public int SequenceOutput = -1;
        public int SequenceInput = -1;
        public List<MyInputParameterSerializationData> Inputs = new List<MyInputParameterSerializationData>();
        public List<MyOutputParameterSerializationData> Outputs = new List<MyOutputParameterSerializationData>();
    }
}

