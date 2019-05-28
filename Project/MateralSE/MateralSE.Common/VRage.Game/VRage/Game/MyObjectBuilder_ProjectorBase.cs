namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public abstract class MyObjectBuilder_ProjectorBase : MyObjectBuilder_FunctionalBlock
    {
        [ProtoMember(0x11), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_CubeGrid ProjectedGrid;
        [ProtoMember(20)]
        public Vector3I ProjectionOffset;
        [ProtoMember(0x16)]
        public Vector3I ProjectionRotation;
        [ProtoMember(0x18)]
        public bool KeepProjection;
        [ProtoMember(0x1a)]
        public bool ShowOnlyBuildable;
        [ProtoMember(0x1c)]
        public bool InstantBuildingEnabled;
        [ProtoMember(30)]
        public int MaxNumberOfProjections = 5;
        [ProtoMember(0x20)]
        public int MaxNumberOfBlocks = 200;
        [ProtoMember(0x22)]
        public int ProjectionsRemaining;
        [ProtoMember(0x24)]
        public bool GetOwnershipFromProjector;
        [ProtoMember(0x26)]
        public float Scale = 1f;
        [ProtoMember(0x29), Serialize(MyObjectFlags.DefaultZero)]
        public List<MySerializedTextPanelData> TextPanels;

        protected MyObjectBuilder_ProjectorBase()
        {
        }

        public override void Remap(IMyRemapHelper remapHelper)
        {
            base.Remap(remapHelper);
            if (this.ProjectedGrid != null)
            {
                this.ProjectedGrid.Remap(remapHelper);
            }
        }
    }
}

