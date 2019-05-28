namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_PirateAntennaDefinition), (Type) null)]
    public class MyPirateAntennaDefinition : MyDefinitionBase
    {
        public string Name;
        public float SpawnDistance;
        public int SpawnTimeMs;
        public int FirstSpawnTimeMs;
        public int MaxDrones;
        public MyDiscreteSampler<MySpawnGroupDefinition> SpawnGroupSampler;
        private List<string> m_spawnGroups;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PirateAntennaDefinition definition = builder as MyObjectBuilder_PirateAntennaDefinition;
            this.Name = definition.Name;
            this.SpawnDistance = definition.SpawnDistance;
            this.SpawnTimeMs = definition.SpawnTimeMs;
            this.FirstSpawnTimeMs = definition.FirstSpawnTimeMs;
            this.MaxDrones = definition.MaxDrones;
            this.m_spawnGroups = new List<string>();
            foreach (string str in definition.SpawnGroups)
            {
                this.m_spawnGroups.Add(str);
            }
        }

        public void Postprocess()
        {
            List<MySpawnGroupDefinition> values = new List<MySpawnGroupDefinition>();
            List<float> densities = new List<float>();
            foreach (string str in this.m_spawnGroups)
            {
                MySpawnGroupDefinition definition = null;
                MyDefinitionId defId = new MyDefinitionId(typeof(MyObjectBuilder_SpawnGroupDefinition), str);
                MyDefinitionManager.Static.TryGetDefinition<MySpawnGroupDefinition>(defId, out definition);
                if (definition != null)
                {
                    values.Add(definition);
                    densities.Add(definition.Frequency);
                }
            }
            this.m_spawnGroups = null;
            if (densities.Count != 0)
            {
                this.SpawnGroupSampler = new MyDiscreteSampler<MySpawnGroupDefinition>(values, densities);
            }
        }
    }
}

