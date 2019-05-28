namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VariableSetterScriptNode : MyObjectBuilder_ScriptNode
    {
        public string VariableName = string.Empty;
        public string VariableValue = string.Empty;
        public int SequenceInputID = -1;
        public int SequenceOutputID = -1;
        public MyVariableIdentifier ValueInputID = MyVariableIdentifier.Default;
    }
}

