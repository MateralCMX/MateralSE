namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TriggerScriptNode : MyObjectBuilder_ScriptNode
    {
        public string TriggerName = string.Empty;
        public int SequenceInputID = -1;
        public List<MyVariableIdentifier> InputIDs = new List<MyVariableIdentifier>();
        public List<string> InputNames = new List<string>();
        public List<string> InputTypes = new List<string>();
    }
}

