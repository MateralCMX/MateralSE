namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ConveyorSorterDefinition : MyObjectBuilder_CubeBlockDefinition
    {
        [ProtoMember(13)]
        public string ResourceSinkGroup;
        [ProtoMember(0x10)]
        public float PowerInput = 0.001f;
        [ProtoMember(0x13)]
        public Vector3 InventorySize;
    }
}

