namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_MissileLauncherDefinition), (Type) null)]
    public class MyMissileLauncherDefinition : MyCubeBlockDefinition
    {
        public string ProjectileMissile;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_MissileLauncherDefinition definition = (MyObjectBuilder_MissileLauncherDefinition) builder;
            this.ProjectileMissile = definition.ProjectileMissile;
        }
    }
}

