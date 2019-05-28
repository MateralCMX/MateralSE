namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_InventoryBase : MyObjectBuilder_ComponentBase
    {
        [ProtoMember(13), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public string InventoryId;

        public virtual void Clear()
        {
        }
    }
}

