namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_GravityGeneratorBaseDefinition), (Type) null)]
    public class MyGravityGeneratorBaseDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float MinGravityAcceleration;
        public float MaxGravityAcceleration;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GravityGeneratorBaseDefinition definition = builder as MyObjectBuilder_GravityGeneratorBaseDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.MinGravityAcceleration = definition.MinGravityAcceleration;
            this.MaxGravityAcceleration = definition.MaxGravityAcceleration;
        }
    }
}

