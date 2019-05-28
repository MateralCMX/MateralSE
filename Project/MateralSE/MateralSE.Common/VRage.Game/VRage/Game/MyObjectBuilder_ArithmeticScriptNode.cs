namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((System.Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ArithmeticScriptNode : MyObjectBuilder_ScriptNode
    {
        public List<MyVariableIdentifier> OutputNodeIDs = new List<MyVariableIdentifier>();
        public string Operation;
        public string Type;
        public MyVariableIdentifier InputAID = MyVariableIdentifier.Default;
        public MyVariableIdentifier InputBID = MyVariableIdentifier.Default;
        public string ValueA = string.Empty;
        public string ValueB = string.Empty;
    }
}

