namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TriggerPositionLeft : MyObjectBuilder_Trigger
    {
        [ProtoMember(12)]
        public Vector3D Pos;
        [ProtoMember(14)]
        public double Distance2;
    }
}

