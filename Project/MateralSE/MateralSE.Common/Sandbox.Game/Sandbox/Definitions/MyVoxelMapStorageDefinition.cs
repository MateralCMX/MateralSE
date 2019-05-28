namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_VoxelMapStorageDefinition), (Type) null)]
    public class MyVoxelMapStorageDefinition : MyDefinitionBase
    {
        public string StorageFile;
        public bool UseForProceduralRemovals;
        public bool UseForProceduralAdditions;
        public bool UseAsPrimaryProceduralAdditionShape;
        public float SpawnProbability;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_VoxelMapStorageDefinition definition = builder as MyObjectBuilder_VoxelMapStorageDefinition;
            this.StorageFile = definition.StorageFile;
            this.UseForProceduralRemovals = definition.UseForProceduralRemovals;
            this.UseForProceduralAdditions = definition.UseForProceduralAdditions;
            this.UseAsPrimaryProceduralAdditionShape = definition.UseAsPrimaryProceduralAdditionShape;
            this.SpawnProbability = definition.SpawnProbability;
        }
    }
}

