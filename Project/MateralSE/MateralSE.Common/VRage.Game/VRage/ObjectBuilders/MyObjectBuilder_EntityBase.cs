namespace VRage.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null)]
    public class MyObjectBuilder_EntityBase : MyObjectBuilder_Base
    {
        [ProtoMember(0x18)]
        public long EntityId;
        [ProtoMember(0x1b)]
        public MyPersistentEntityFlags2 PersistentFlags;
        [ProtoMember(30), Serialize(MyObjectFlags.DefaultZero)]
        public string Name;
        [ProtoMember(0x22)]
        public MyPositionAndOrientation? PositionAndOrientation;
        [ProtoMember(0x2b), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_ComponentContainer ComponentContainer;
        [ProtoMember(0x34), DefaultValue((string) null), NoSerialize]
        public SerializableDefinitionId? EntityDefinitionId;

        public virtual void Remap(IMyRemapHelper remapHelper)
        {
            this.EntityId = remapHelper.RemapEntityId(this.EntityId);
        }

        public bool ShouldSerializeComponentContainer() => 
            ((this.ComponentContainer != null) && ((this.ComponentContainer.Components != null) && (this.ComponentContainer.Components.Count > 0)));

        public bool ShouldSerializeEntityDefinitionId() => 
            false;

        public bool ShouldSerializePositionAndOrientation() => 
            (this.PositionAndOrientation != null);
    }
}

