namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_OutputScriptNode : MyObjectBuilder_ScriptNode
    {
        public int SequenceInputID = -1;
        public List<MyInputParameterSerializationData> Inputs = new List<MyInputParameterSerializationData>();
    }
}

