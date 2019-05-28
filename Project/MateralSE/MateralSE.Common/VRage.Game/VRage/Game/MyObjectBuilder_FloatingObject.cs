namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FloatingObject : MyObjectBuilder_EntityBase
    {
        [ProtoMember(13)]
        public MyObjectBuilder_InventoryItem Item;
        [ProtoMember(0x10), DefaultValue(0)]
        public int ModelVariant;
        [ProtoMember(20), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public string OreSubtypeId;

        public bool ShouldSerializeModelVariant() => 
            (this.ModelVariant != 0);
    }
}

