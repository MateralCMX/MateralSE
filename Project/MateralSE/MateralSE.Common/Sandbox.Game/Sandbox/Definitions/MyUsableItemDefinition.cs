namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_UsableItemDefinition), (Type) null)]
    public class MyUsableItemDefinition : MyPhysicalItemDefinition
    {
        public string UseSound;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_UsableItemDefinition definition = builder as MyObjectBuilder_UsableItemDefinition;
            this.UseSound = definition.UseSound;
        }
    }
}

