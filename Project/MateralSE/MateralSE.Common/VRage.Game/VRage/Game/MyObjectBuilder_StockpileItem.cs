namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_StockpileItem : MyObjectBuilder_Base
    {
        [ProtoMember(12)]
        public int Amount;
        [ProtoMember(15), DynamicObjectBuilder(false), Nullable]
        public MyObjectBuilder_PhysicalObject PhysicalContent;
    }
}

