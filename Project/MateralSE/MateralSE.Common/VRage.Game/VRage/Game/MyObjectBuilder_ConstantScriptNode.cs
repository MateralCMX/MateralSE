namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((System.Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ConstantScriptNode : MyObjectBuilder_ScriptNode
    {
        public string Value = string.Empty;
        public string Type = string.Empty;
        public IdentifierList OutputIds;
        public Vector3D Vector;
    }
}

