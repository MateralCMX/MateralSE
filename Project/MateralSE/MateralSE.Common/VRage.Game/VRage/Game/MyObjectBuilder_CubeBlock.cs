namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CubeBlock : MyObjectBuilder_Base
    {
        [ProtoMember(0x13), DefaultValue(0), Serialize(MyObjectFlags.DefaultZero)]
        public long EntityId;
        [ProtoMember(0x19), Serialize(MyObjectFlags.DefaultZero)]
        public string Name;
        [ProtoMember(0x1d), Serialize(MyPrimitiveFlags.Variant, Kind=MySerializeKind.Item)]
        public SerializableVector3I Min = new SerializableVector3I(0, 0, 0);
        private SerializableQuaternion m_orientation;
        [ProtoMember(0x38), DefaultValue((float) 1f), Serialize(MyPrimitiveFlags.FixedPoint16 | MyPrimitiveFlags.Normalized)]
        public float IntegrityPercent = 1f;
        [ProtoMember(60), DefaultValue((float) 1f), Serialize(MyPrimitiveFlags.FixedPoint16 | MyPrimitiveFlags.Normalized)]
        public float BuildPercent = 1f;
        [ProtoMember(0x40)]
        public SerializableBlockOrientation BlockOrientation = SerializableBlockOrientation.Identity;
        [ProtoMember(0x44), DefaultValue((string) null), NoSerialize]
        public MyObjectBuilder_Inventory ConstructionInventory;
        [ProtoMember(0x49)]
        public SerializableVector3 ColorMaskHSV = new SerializableVector3(0f, -1f, 0f);
        [ProtoMember(0x4d)]
        public MyStringHash SkinSubtypeId;
        [ProtoMember(0x67), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_ConstructionStockpile ConstructionStockpile;
        [ProtoMember(0x6c), DefaultValue(0), Serialize(MyObjectFlags.DefaultZero)]
        public long Owner;
        [ProtoMember(0x70), DefaultValue(0), Serialize(MyObjectFlags.DefaultZero)]
        public long BuiltBy;
        [ProtoMember(0x7a), DefaultValue(0)]
        public MyOwnershipShareModeEnum ShareMode;
        [ProtoMember(0x7d), DefaultValue(0), NoSerialize]
        public float DeformationRatio;
        [XmlArrayItem("SubBlock"), ProtoMember(0x8f), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public MySubBlockId[] SubBlocks;
        [ProtoMember(0x93), DefaultValue(0), Serialize(MyObjectFlags.DefaultZero)]
        public int MultiBlockId;
        [ProtoMember(0x98), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public SerializableDefinitionId? MultiBlockDefinition;
        [ProtoMember(0x9d), DefaultValue(-1), Serialize]
        public int MultiBlockIndex = -1;
        [ProtoMember(0xa1), DefaultValue((float) 1f), Serialize]
        public float BlockGeneralDamageModifier = 1f;
        [ProtoMember(0xa5), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_ComponentContainer ComponentContainer;

        public virtual void Remap(IMyRemapHelper remapHelper)
        {
            if (this.EntityId != 0)
            {
                this.EntityId = remapHelper.RemapEntityId(this.EntityId);
            }
            if (this.SubBlocks != null)
            {
                for (int i = 0; i < this.SubBlocks.Length; i++)
                {
                    if (this.SubBlocks[i].SubGridId != 0)
                    {
                        this.SubBlocks[i].SubGridId = remapHelper.RemapEntityId(this.SubBlocks[i].SubGridId);
                    }
                }
            }
            if ((this.MultiBlockId != 0) && (this.MultiBlockDefinition != null))
            {
                this.MultiBlockId = remapHelper.RemapGroupId("MultiBlockId", this.MultiBlockId);
            }
        }

        public virtual void SetupForProjector()
        {
            this.Owner = 0L;
            this.ShareMode = MyOwnershipShareModeEnum.None;
        }

        public bool ShouldSerializeBlockOrientation() => 
            (this.BlockOrientation != SerializableBlockOrientation.Identity);

        public bool ShouldSerializeColorMaskHSV() => 
            (this.ColorMaskHSV != new SerializableVector3(0f, -1f, 0f));

        public bool ShouldSerializeComponentContainer() => 
            ((this.ComponentContainer != null) && ((this.ComponentContainer.Components != null) && (this.ComponentContainer.Components.Count > 0)));

        public bool ShouldSerializeConstructionInventory() => 
            false;

        public bool ShouldSerializeConstructionStockpile() => 
            (this.ConstructionStockpile != null);

        public bool ShouldSerializeEntityId() => 
            (this.EntityId != 0L);

        public bool ShouldSerializeMin() => 
            (this.Min != new SerializableVector3I(0, 0, 0));

        public bool ShouldSerializeMultiBlockDefinition() => 
            ((this.MultiBlockId != 0) && (this.MultiBlockDefinition != null));

        public bool ShouldSerializeMultiBlockId() => 
            (this.MultiBlockId != 0);

        public bool ShouldSerializeOrientation() => 
            false;

        public bool ShouldSerializeSkinSubtypeId() => 
            (this.SkinSubtypeId != MyStringHash.NullOrEmpty);

        public static MyObjectBuilder_CubeBlock Upgrade(MyObjectBuilder_CubeBlock cubeBlock, MyObjectBuilderType newType, string newSubType)
        {
            MyObjectBuilder_CubeBlock block = MyObjectBuilderSerializer.CreateNewObject(newType, newSubType) as MyObjectBuilder_CubeBlock;
            if (block == null)
            {
                return null;
            }
            block.EntityId = cubeBlock.EntityId;
            block.Min = cubeBlock.Min;
            block.m_orientation = cubeBlock.m_orientation;
            block.IntegrityPercent = cubeBlock.IntegrityPercent;
            block.BuildPercent = cubeBlock.BuildPercent;
            block.BlockOrientation = cubeBlock.BlockOrientation;
            block.ConstructionInventory = cubeBlock.ConstructionInventory;
            block.ColorMaskHSV = cubeBlock.ColorMaskHSV;
            return block;
        }

        [NoSerialize]
        public SerializableQuaternion Orientation
        {
            get => 
                this.m_orientation;
            set
            {
                this.m_orientation = MyUtils.IsZero((Quaternion) value, 1E-05f) ? Quaternion.Identity : value;
                this.BlockOrientation = new SerializableBlockOrientation(Base6Directions.GetForward((Quaternion) this.m_orientation), Base6Directions.GetUp((Quaternion) this.m_orientation));
            }
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MySubBlockId
        {
            [ProtoMember(0x84)]
            public long SubGridId;
            [ProtoMember(0x87)]
            public string SubGridName;
            [ProtoMember(0x8a)]
            public SerializableVector3I SubBlockPosition;
        }
    }
}

