namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_VoxelMapCollectionDefinition), (Type) null)]
    public class MyVoxelMapCollectionDefinition : MyDefinitionBase
    {
        public MyDiscreteSampler<MyDefinitionId> StorageFiles;
        public MyStringHash Modifier;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_VoxelMapCollectionDefinition definition = builder as MyObjectBuilder_VoxelMapCollectionDefinition;
            if (definition != null)
            {
                List<MyDefinitionId> values = new List<MyDefinitionId>();
                List<float> densities = new List<float>();
                for (int i = 0; i < definition.StorageDefs.Length; i++)
                {
                    MyObjectBuilder_VoxelMapCollectionDefinition.VoxelMapStorage storage = definition.StorageDefs[i];
                    values.Add(new MyDefinitionId(typeof(MyObjectBuilder_VoxelMapStorageDefinition), storage.Storage));
                    densities.Add(storage.Probability);
                }
                this.StorageFiles = new MyDiscreteSampler<MyDefinitionId>(values, densities);
                this.Modifier = MyStringHash.GetOrCompute(definition.Modifier);
            }
        }
    }
}

