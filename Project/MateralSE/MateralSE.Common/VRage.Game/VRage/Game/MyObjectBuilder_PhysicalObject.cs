namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PhysicalObject : MyObjectBuilder_Base
    {
        [ProtoMember(15), DefaultValue(0)]
        public MyItemFlags Flags;
        [ProtoMember(0x15), DefaultValue((string) null)]
        public float? DurabilityHP;

        public MyObjectBuilder_PhysicalObject() : this(MyItemFlags.None)
        {
        }

        public MyObjectBuilder_PhysicalObject(MyItemFlags flags)
        {
            this.Flags = flags;
        }

        public virtual bool CanStack(MyObjectBuilder_PhysicalObject a) => 
            ((a != null) ? this.CanStack(a.TypeId, a.SubtypeId, a.Flags) : false);

        public virtual bool CanStack(MyObjectBuilderType typeId, MyStringHash subtypeId, MyItemFlags flags) => 
            ((flags == this.Flags) && ((typeId == base.TypeId) && (subtypeId == base.SubtypeId)));

        public virtual MyDefinitionId GetObjectId() => 
            this.GetId();

        public bool ShouldSerializeDurabilityHP() => 
            (this.DurabilityHP != null);
    }
}

