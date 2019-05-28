namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_DropContainerDefinition), (Type) null)]
    public class MyDropContainerDefinition : MyDefinitionBase
    {
        public MyPrefabDefinition Prefab;
        public MyContainerSpawnRules SpawnRules;
        public float Priority;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_DropContainerDefinition definition = builder as MyObjectBuilder_DropContainerDefinition;
            this.SpawnRules = definition.SpawnRules;
            this.Prefab = MyDefinitionManager.Static.GetPrefabDefinition(definition.Prefab);
            this.Priority = definition.Priority;
        }
    }
}

