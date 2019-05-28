namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Inventory : MyObjectBuilder_InventoryBase
    {
        [ProtoMember(0x18)]
        public List<MyObjectBuilder_InventoryItem> Items = new List<MyObjectBuilder_InventoryItem>();
        [ProtoMember(0x1b)]
        public uint nextItemId;
        [ProtoMember(30), DefaultValue((string) null)]
        public MyFixedPoint? Volume;
        [ProtoMember(0x21), DefaultValue((string) null)]
        public MyFixedPoint? Mass;
        [ProtoMember(0x24), DefaultValue((string) null)]
        public int? MaxItemCount;
        [ProtoMember(40), DefaultValue((string) null)]
        public SerializableVector3? Size;
        [ProtoMember(0x2b), DefaultValue((string) null)]
        public MyInventoryFlags? InventoryFlags;
        [ProtoMember(0x2e)]
        public bool RemoveEntityOnEmpty;

        public override void Clear()
        {
            this.Items.Clear();
            this.nextItemId = 0;
            base.Clear();
        }

        public bool ShouldSerializeMaxItemCount() => 
            (this.MaxItemCount != null);
    }
}

