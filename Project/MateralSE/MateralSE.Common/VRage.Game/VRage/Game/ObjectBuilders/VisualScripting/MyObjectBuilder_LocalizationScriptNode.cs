namespace VRage.Game.ObjectBuilders.VisualScripting
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_LocalizationScriptNode : MyObjectBuilder_ScriptNode
    {
        public string Context = string.Empty;
        public string MessageId = string.Empty;
        public ulong ResourceId = ulong.MaxValue;
        public List<MyVariableIdentifier> ParameterInputs = new List<MyVariableIdentifier>();
        public List<MyVariableIdentifier> ValueOutputs = new List<MyVariableIdentifier>();
    }
}

