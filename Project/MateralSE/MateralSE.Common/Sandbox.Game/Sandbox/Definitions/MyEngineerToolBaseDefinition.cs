namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_EngineerToolBaseDefinition), (Type) null)]
    public class MyEngineerToolBaseDefinition : MyHandItemDefinition
    {
        public float SpeedMultiplier;
        public float DistanceMultiplier;
        public string Flare;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_EngineerToolBaseDefinition objectBuilder = (MyObjectBuilder_EngineerToolBaseDefinition) base.GetObjectBuilder();
            objectBuilder.SpeedMultiplier = this.SpeedMultiplier;
            objectBuilder.DistanceMultiplier = this.DistanceMultiplier;
            objectBuilder.Flare = this.Flare;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_EngineerToolBaseDefinition definition = builder as MyObjectBuilder_EngineerToolBaseDefinition;
            this.SpeedMultiplier = definition.SpeedMultiplier;
            this.DistanceMultiplier = definition.DistanceMultiplier;
            this.Flare = definition.Flare;
        }
    }
}

