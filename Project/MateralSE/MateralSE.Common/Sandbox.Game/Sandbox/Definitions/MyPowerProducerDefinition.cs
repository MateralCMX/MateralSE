namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_PowerProducerDefinition), (Type) null)]
    public class MyPowerProducerDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSourceGroup;
        public float MaxPowerOutput;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PowerProducerDefinition definition = builder as MyObjectBuilder_PowerProducerDefinition;
            if (definition != null)
            {
                this.ResourceSourceGroup = MyStringHash.GetOrCompute(definition.ResourceSourceGroup);
                this.MaxPowerOutput = definition.MaxPowerOutput;
            }
        }
    }
}

