namespace Sandbox.Definitions
{
    using System;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AsteroidGeneratorDefinition), (Type) null)]
    public class MyAsteroidGeneratorDefinition : MyDefinitionBase
    {
        public int Version;
        public int ObjectSizeMin;
        public int ObjectSizeMax;
        public int SubcellSize;
        public int SubCells;
        public int ObjectMaxInCluster;
        public int ObjectMinDistanceInCluster;
        public int ObjectMaxDistanceInClusterMin;
        public int ObjectMaxDistanceInClusterMax;
        public int ObjectSizeMinCluster;
        public int ObjectSizeMaxCluster;
        public double ObjectDensityCluster;
        public bool ClusterDispersionAbsolute;
        public bool AllowPartialClusterObjectOverlap;
        public bool UseClusterDefAsAsteroid;
        public bool RotateAsteroids;
        public bool UseLinearPowOfTwoSizeDistribution;
        public bool UseGeneratorSeed;
        public bool UseClusterVariableSize;
        public DictionaryReader<MyObjectSeedType, double> SeedTypeProbability;
        public DictionaryReader<MyObjectSeedType, double> SeedClusterTypeProbability;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AsteroidGeneratorDefinition definition = (MyObjectBuilder_AsteroidGeneratorDefinition) builder;
            this.Version = int.Parse(definition.Id.SubtypeId);
            this.SubCells = definition.SubCells;
            this.ObjectSizeMax = definition.ObjectSizeMax;
            this.SubcellSize = 0x1000 + (definition.ObjectSizeMax * 2);
            this.ObjectSizeMin = definition.ObjectSizeMin;
            this.RotateAsteroids = definition.RotateAsteroids;
            this.UseGeneratorSeed = definition.UseGeneratorSeed;
            this.ObjectMaxInCluster = definition.ObjectMaxInCluster;
            this.ObjectDensityCluster = definition.ObjectDensityCluster;
            this.ObjectSizeMaxCluster = definition.ObjectSizeMaxCluster;
            this.ObjectSizeMinCluster = definition.ObjectSizeMinCluster;
            this.UseClusterVariableSize = definition.UseClusterVariableSize;
            this.UseClusterDefAsAsteroid = definition.UseClusterDefAsAsteroid;
            this.ClusterDispersionAbsolute = definition.ClusterDispersionAbsolute;
            this.ObjectMinDistanceInCluster = definition.ObjectMinDistanceInCluster;
            this.ObjectMaxDistanceInClusterMax = definition.ObjectMaxDistanceInClusterMax;
            this.ObjectMaxDistanceInClusterMin = definition.ObjectMaxDistanceInClusterMin;
            this.AllowPartialClusterObjectOverlap = definition.AllowPartialClusterObjectOverlap;
            this.UseLinearPowOfTwoSizeDistribution = definition.UseLinearPowOfTwoSizeDistribution;
            this.SeedTypeProbability = definition.SeedTypeProbability.Dictionary;
            this.SeedClusterTypeProbability = definition.SeedClusterTypeProbability.Dictionary;
        }
    }
}

