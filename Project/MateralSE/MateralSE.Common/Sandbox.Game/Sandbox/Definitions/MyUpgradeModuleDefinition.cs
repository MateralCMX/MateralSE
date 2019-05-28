namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_UpgradeModuleDefinition), (Type) null)]
    public class MyUpgradeModuleDefinition : MyCubeBlockDefinition
    {
        public MyUpgradeModuleInfo[] Upgrades;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_UpgradeModuleDefinition definition = builder as MyObjectBuilder_UpgradeModuleDefinition;
            this.Upgrades = definition.Upgrades;
        }
    }
}

