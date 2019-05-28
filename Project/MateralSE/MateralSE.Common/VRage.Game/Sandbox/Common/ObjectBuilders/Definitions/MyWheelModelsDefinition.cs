namespace Sandbox.Common.ObjectBuilders.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_WheelModelsDefinition), (Type) null)]
    public class MyWheelModelsDefinition : MyDefinitionBase
    {
        public string AlternativeModel;
        public float AngularVelocityThreshold;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_WheelModelsDefinition definition = (MyObjectBuilder_WheelModelsDefinition) builder;
            this.AlternativeModel = definition.AlternativeModel;
            this.AngularVelocityThreshold = definition.AngularVelocityThreshold;
        }
    }
}

