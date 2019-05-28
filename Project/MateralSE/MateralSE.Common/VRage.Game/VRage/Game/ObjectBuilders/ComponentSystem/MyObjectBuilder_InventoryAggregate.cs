namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_InventoryAggregate : MyObjectBuilder_InventoryBase
    {
        [ProtoMember(15), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero), DynamicNullableObjectBuilderItem(false), XmlArrayItem("MyObjectBuilder_InventoryBase", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_InventoryBase>))]
        public List<MyObjectBuilder_InventoryBase> Inventories;

        public override void Clear()
        {
            using (List<MyObjectBuilder_InventoryBase>.Enumerator enumerator = this.Inventories.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Clear();
                }
            }
        }
    }
}

