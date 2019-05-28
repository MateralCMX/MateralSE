namespace VRage.ObjectBuilders.Voxels
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.ObjectBuilders.Definitions.Components;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VoxelPostprocessingDecimate : MyObjectBuilder_VoxelPostprocessing
    {
        [ProtoMember(0x22), Description("Set of lod range settings pairs.")]
        public List<Settings> LodSettings;

        [ProtoContract]
        public class Settings
        {
            [XmlAttribute, Description("Minimum lod level these settings apply. Subsequent sets must have strictly ascending lods.")]
            public int FromLod;
            [Description("The minimum angle to be considered a feature edge. Value is In Radians")]
            public float FeatureAngle;
            [Description("Distance threshold for an edge vertex to be discarded.")]
            public float EdgeThreshold;
            [Description("Distance threshold for an internal vertex to be discarded.")]
            public float PlaneThreshold;
            [Description("Weather edge vertices should be considered or not for removal.")]
            public bool IgnoreEdges = true;
        }
    }
}

