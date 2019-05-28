namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BlockItem : MyObjectBuilder_PhysicalObject
    {
        [ProtoMember(13)]
        public SerializableDefinitionId BlockDefId;

        public override bool CanStack(MyObjectBuilder_PhysicalObject a)
        {
            MyObjectBuilder_BlockItem item = a as MyObjectBuilder_BlockItem;
            return ((item != null) ? ((item.BlockDefId.TypeId == this.BlockDefId.TypeId) && ((item.BlockDefId.SubtypeId == this.BlockDefId.SubtypeId) && (a.Flags == base.Flags))) : false);
        }

        public override bool CanStack(MyObjectBuilderType typeId, MyStringHash subtypeId, MyItemFlags flags) => 
            ((new MyDefinitionId(typeId, subtypeId) == this.BlockDefId) && (flags == base.Flags));

        public override MyDefinitionId GetObjectId() => 
            this.BlockDefId;
    }
}

