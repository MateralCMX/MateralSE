namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_BattleDefinition), (Type) null)]
    public class MyBattleDefinition : MyDefinitionBase
    {
        public MyObjectBuilder_Toolbar DefaultToolbar;
        public MyDefinitionId[] SpawnBlocks;
        public float DefenderEntityDamage;
        public string[] DefaultBlueprints;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_BattleDefinition definition = builder as MyObjectBuilder_BattleDefinition;
            this.DefaultToolbar = definition.DefaultToolbar;
            this.DefenderEntityDamage = definition.DefenderEntityDamage;
            this.DefaultBlueprints = definition.DefaultBlueprints;
            if ((definition.SpawnBlocks != null) && (definition.SpawnBlocks.Length != 0))
            {
                this.SpawnBlocks = new MyDefinitionId[definition.SpawnBlocks.Length];
                for (int i = 0; i < definition.SpawnBlocks.Length; i++)
                {
                    this.SpawnBlocks[i] = definition.SpawnBlocks[i];
                }
            }
        }

        public void Merge(MyBattleDefinition src)
        {
            this.DefaultToolbar = src.DefaultToolbar;
            this.DefenderEntityDamage = src.DefenderEntityDamage;
            this.DefaultBlueprints = src.DefaultBlueprints;
            if ((src.SpawnBlocks != null) && (src.SpawnBlocks.Length != 0))
            {
                this.SpawnBlocks = new MyDefinitionId[src.SpawnBlocks.Length];
                for (int i = 0; i < src.SpawnBlocks.Length; i++)
                {
                    this.SpawnBlocks[i] = src.SpawnBlocks[i];
                }
            }
        }
    }
}

