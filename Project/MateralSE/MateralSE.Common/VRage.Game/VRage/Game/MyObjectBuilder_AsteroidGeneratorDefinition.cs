namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AsteroidGeneratorDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x11)]
        public int ObjectSizeMin;
        [ProtoMember(20)]
        public int ObjectSizeMax;
        [ProtoMember(0x17)]
        public int SubCells;
        [ProtoMember(0x1a)]
        public int ObjectMaxInCluster;
        [ProtoMember(0x1d)]
        public int ObjectMinDistanceInCluster;
        [ProtoMember(0x20)]
        public int ObjectMaxDistanceInClusterMin;
        [ProtoMember(0x23)]
        public int ObjectMaxDistanceInClusterMax;
        [ProtoMember(0x26)]
        public int ObjectSizeMinCluster;
        [ProtoMember(0x29)]
        public int ObjectSizeMaxCluster;
        [ProtoMember(0x2c)]
        public double ObjectDensityCluster;
        [ProtoMember(0x2f)]
        public bool ClusterDispersionAbsolute;
        [ProtoMember(50)]
        public bool AllowPartialClusterObjectOverlap;
        [ProtoMember(0x35)]
        public bool UseClusterDefAsAsteroid;
        [ProtoMember(0x38)]
        public bool RotateAsteroids;
        [ProtoMember(0x3b)]
        public bool UseLinearPowOfTwoSizeDistribution;
        [ProtoMember(0x3e)]
        public bool UseGeneratorSeed;
        [ProtoMember(0x41)]
        public bool UseClusterVariableSize;
        [ProtoMember(0x44)]
        public SerializableDictionary<MyObjectSeedType, double> SeedTypeProbability;
        [ProtoMember(0x47)]
        public SerializableDictionary<MyObjectSeedType, double> SeedClusterTypeProbability;
    }
}

