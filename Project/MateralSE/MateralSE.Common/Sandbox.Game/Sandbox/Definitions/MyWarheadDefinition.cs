namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_WarheadDefinition), (Type) null)]
    public class MyWarheadDefinition : MyCubeBlockDefinition
    {
        public float ExplosionRadius;
        public float WarheadExplosionDamage;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WarheadDefinition definition = (MyObjectBuilder_WarheadDefinition) builder;
            this.ExplosionRadius = definition.ExplosionRadius;
            this.WarheadExplosionDamage = definition.WarheadExplosionDamage;
        }
    }
}

