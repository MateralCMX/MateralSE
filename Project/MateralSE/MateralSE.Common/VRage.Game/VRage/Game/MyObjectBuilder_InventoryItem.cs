namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_InventoryItem : MyObjectBuilder_Base
    {
        [ProtoMember(13), XmlElement("Amount")]
        public MyFixedPoint Amount = 1;
        [ProtoMember(0x11), XmlElement("Scale")]
        public float Scale = 1f;
        [ProtoMember(0x22), XmlElement(Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PhysicalObject>)), DynamicObjectBuilder(false), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_PhysicalObject Content;
        [ProtoMember(0x2a), XmlElement(Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_PhysicalObject>)), DynamicObjectBuilder(false), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_PhysicalObject PhysicalContent;
        [ProtoMember(0x30)]
        public uint ItemId;

        public bool ShouldSerializeContent() => 
            false;

        public bool ShouldSerializeObsolete_AmountDecimal() => 
            false;

        public bool ShouldSerializeScale() => 
            !(this.Scale == 1f);

        [XmlElement("AmountDecimal"), NoSerialize]
        public decimal Obsolete_AmountDecimal
        {
            get => 
                ((decimal) this.Amount);
            set => 
                (this.Amount = (MyFixedPoint) value);
        }
    }
}

