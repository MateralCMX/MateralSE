namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_PlanetPrefabDefinition), (Type) null)]
    public class MyPlanetPrefabDefinition : MyDefinitionBase
    {
        public MyObjectBuilder_Planet PlanetBuilder;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PlanetPrefabDefinition definition = builder as MyObjectBuilder_PlanetPrefabDefinition;
            this.PlanetBuilder = definition.PlanetBuilder;
        }
    }
}

