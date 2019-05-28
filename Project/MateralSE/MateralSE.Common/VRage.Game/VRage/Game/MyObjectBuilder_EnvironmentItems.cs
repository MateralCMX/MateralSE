namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers"), MyEnvironmentItems(typeof(MyObjectBuilder_EnvironmentItemDefinition))]
    public class MyObjectBuilder_EnvironmentItems : MyObjectBuilder_EntityBase
    {
        [XmlArrayItem("Item"), ProtoMember(0x29)]
        public MyOBEnvironmentItemData[] Items;
        [ProtoMember(0x2c)]
        public Vector3D CellsOffset;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MyOBEnvironmentItemData
        {
            [ProtoMember(0x21)]
            public MyPositionAndOrientation PositionAndOrientation;
            [ProtoMember(0x24)]
            public string SubtypeName;
        }
    }
}

