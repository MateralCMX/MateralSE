namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((System.Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CastScriptNode : MyObjectBuilder_ScriptNode
    {
        public string Type;
        public int SequenceInputID = -1;
        public int SequenceOuputID = -1;
        public MyVariableIdentifier InputID = MyVariableIdentifier.Default;
        public List<MyVariableIdentifier> OuputIDs = new List<MyVariableIdentifier>();
    }
}

