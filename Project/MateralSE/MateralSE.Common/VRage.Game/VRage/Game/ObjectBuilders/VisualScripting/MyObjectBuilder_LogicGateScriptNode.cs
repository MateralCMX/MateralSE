namespace VRage.Game.ObjectBuilders.VisualScripting
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_LogicGateScriptNode : MyObjectBuilder_ScriptNode
    {
        public List<MyVariableIdentifier> ValueInputs = new List<MyVariableIdentifier>();
        public List<MyVariableIdentifier> ValueOutputs = new List<MyVariableIdentifier>();
        public LogicOperation Operation = LogicOperation.NOT;

        public enum LogicOperation
        {
            AND,
            OR,
            XOR,
            NAND,
            NOR,
            NOT
        }
    }
}

