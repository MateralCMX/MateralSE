namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_WelderDefinition), (Type) null)]
    internal class MyWelderDefinition : MyEngineerToolBaseDefinition
    {
        public string FlameEffect;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_WelderDefinition objectBuilder = (MyObjectBuilder_WelderDefinition) base.GetObjectBuilder();
            objectBuilder.FlameEffect = this.FlameEffect;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WelderDefinition definition = builder as MyObjectBuilder_WelderDefinition;
            this.FlameEffect = definition.FlameEffect;
        }
    }
}

