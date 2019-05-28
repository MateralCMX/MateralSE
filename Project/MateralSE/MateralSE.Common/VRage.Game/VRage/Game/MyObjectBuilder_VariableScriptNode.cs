namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VariableScriptNode : MyObjectBuilder_ScriptNode
    {
        public string VariableName = "Default";
        public string VariableType = string.Empty;
        public string VariableValue = string.Empty;
        public List<MyVariableIdentifier> OutputNodeIds = new List<MyVariableIdentifier>();
        public Vector3D Vector;
        public List<MyVariableIdentifier> OutputNodeIdsX = new List<MyVariableIdentifier>();
        public List<MyVariableIdentifier> OutputNodeIdsY = new List<MyVariableIdentifier>();
        public List<MyVariableIdentifier> OutputNodeIdsZ = new List<MyVariableIdentifier>();
    }
}

