namespace VRage.Game.ObjectBuilders.VisualScripting
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SwitchScriptNode : MyObjectBuilder_ScriptNode
    {
        public int SequenceInput = -1;
        public readonly List<OptionData> Options = new List<OptionData>();
        public MyVariableIdentifier ValueInput;
        public string NodeType = string.Empty;

        [StructLayout(LayoutKind.Sequential)]
        public struct OptionData
        {
            public string Option;
            public int SequenceOutput;
        }
    }
}

