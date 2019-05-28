namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CubeGrid : MyObjectBuilder_EntityBase
    {
        [ProtoMember(0x11)]
        public MyCubeSize GridSizeEnum;
        [ProtoMember(0x13), DynamicItem(typeof(MyObjectBuilderDynamicSerializer), true), XmlArrayItem("MyObjectBuilder_CubeBlock", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_CubeBlock>))]
        public List<MyObjectBuilder_CubeBlock> CubeBlocks = new List<MyObjectBuilder_CubeBlock>();
        [ProtoMember(0x18), DefaultValue(false)]
        public bool IsStatic;
        [ProtoMember(0x1b), DefaultValue(false)]
        public bool IsUnsupportedStation;
        [ProtoMember(30), Serialize(MyObjectFlags.DefaultZero)]
        public List<BoneInfo> Skeleton;
        [ProtoMember(0x23), Serialize(MyObjectFlags.DefaultZero)]
        public SerializableVector3 LinearVelocity;
        [ProtoMember(40), Serialize(MyObjectFlags.DefaultZero)]
        public SerializableVector3 AngularVelocity;
        [ProtoMember(0x2d), Serialize(MyObjectFlags.DefaultZero)]
        public SerializableVector3I? XMirroxPlane;
        [ProtoMember(50), Serialize(MyObjectFlags.DefaultZero)]
        public SerializableVector3I? YMirroxPlane;
        [ProtoMember(0x37), Serialize(MyObjectFlags.DefaultZero)]
        public SerializableVector3I? ZMirroxPlane;
        [ProtoMember(60), DefaultValue(false)]
        public bool XMirroxOdd;
        [ProtoMember(0x3f), DefaultValue(false)]
        public bool YMirroxOdd;
        [ProtoMember(0x42), DefaultValue(false)]
        public bool ZMirroxOdd;
        [ProtoMember(0x45), DefaultValue(true)]
        public bool DampenersEnabled = true;
        [ProtoMember(0x48), DefaultValue(false), Obsolete]
        public bool UsePositionForSpawn;
        [ProtoMember(0x4b), DefaultValue((float) 0.3f), Obsolete]
        public float PlanetSpawnHeightRatio = 0.3f;
        [ProtoMember(0x4e), DefaultValue((float) 500f), Obsolete]
        public float SpawnRangeMin = 500f;
        [ProtoMember(0x51), DefaultValue((float) 650f), Obsolete]
        public float SpawnRangeMax = 650f;
        [ProtoMember(0x54), Serialize(MyObjectFlags.DefaultZero)]
        public List<MyObjectBuilder_ConveyorLine> ConveyorLines = new List<MyObjectBuilder_ConveyorLine>();
        [ProtoMember(0x59)]
        public List<MyObjectBuilder_BlockGroup> BlockGroups = new List<MyObjectBuilder_BlockGroup>();
        [ProtoMember(0x5d), DefaultValue(false)]
        public bool Handbrake;
        [ProtoMember(0x60), Serialize(MyObjectFlags.DefaultZero)]
        public string DisplayName;
        [ProtoMember(100), Serialize(MyObjectFlags.DefaultZero)]
        public float[] OxygenAmount;
        [ProtoMember(0x68)]
        public bool DestructibleBlocks = true;
        [ProtoMember(0x75), Serialize(MyObjectFlags.DefaultZero)]
        public Vector3D? JumpDriveDirection;
        [ProtoMember(0x7a), Serialize(MyObjectFlags.DefaultZero)]
        public float? JumpRemainingTime;
        [ProtoMember(0x7f), DefaultValue(true)]
        public bool CreatePhysics = true;
        [ProtoMember(0x83), DefaultValue(true)]
        public bool EnableSmallToLargeConnections = true;
        [ProtoMember(0x87)]
        public bool IsRespawnGrid;
        [ProtoMember(0x8a), DefaultValue(-1)]
        public int playedTime = -1;
        [ProtoMember(0x8e), DefaultValue((float) 1f)]
        public float GridGeneralDamageModifier = 1f;
        [ProtoMember(0x92)]
        public long LocalCoordSys;
        [ProtoMember(0x95), DefaultValue(true)]
        public bool Editable = true;
        [ProtoMember(0x99), Serialize(MyObjectFlags.DefaultZero)]
        public List<long> TargetingTargets = new List<long>();
        [ProtoMember(0x9d), DefaultValue(false)]
        public bool TargetingWhitelist;
        [ProtoMember(0xa2, IsRequired=false), DefaultValue(true)]
        public bool IsPowered = true;
        [ProtoMember(0xa6, IsRequired=false), Serialize(MyObjectFlags.DefaultZero)]
        public OxygenRoom[] OxygenRooms;

        public override void Remap(IMyRemapHelper remapHelper)
        {
            base.Remap(remapHelper);
            using (List<MyObjectBuilder_CubeBlock>.Enumerator enumerator = this.CubeBlocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Remap(remapHelper);
                }
            }
        }

        public bool ShouldSerializeAngularVelocity() => 
            (this.AngularVelocity != new SerializableVector3(0f, 0f, 0f));

        public bool ShouldSerializeBlockGroups() => 
            ((this.BlockGroups != null) && (this.BlockGroups.Count != 0));

        public bool ShouldSerializeConveyorLines() => 
            ((this.ConveyorLines != null) && (this.ConveyorLines.Count != 0));

        public bool ShouldSerializeJumpDriveDirection() => 
            (this.JumpDriveDirection != null);

        public bool ShouldSerializeJumpRemainingTime() => 
            (this.JumpRemainingTime != null);

        public bool ShouldSerializeLinearVelocity() => 
            (this.LinearVelocity != new SerializableVector3(0f, 0f, 0f));

        public bool ShouldSerializeSkeleton() => 
            ((this.Skeleton != null) && (this.Skeleton.Count != 0));

        public bool ShouldSerializeXMirroxPlane() => 
            (this.XMirroxPlane != null);

        public bool ShouldSerializeYMirroxPlane() => 
            (this.YMirroxPlane != null);

        public bool ShouldSerializeZMirroxPlane() => 
            (this.ZMirroxPlane != null);
    }
}

