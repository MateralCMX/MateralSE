namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GetterScriptNode : MyObjectBuilder_ScriptNode
    {
        public string BoundVariableName = string.Empty;
        public IdentifierList OutputIDs;
    }
}

