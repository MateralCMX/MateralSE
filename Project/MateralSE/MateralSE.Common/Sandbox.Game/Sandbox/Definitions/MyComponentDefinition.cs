namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ComponentDefinition), (Type) null)]
    public class MyComponentDefinition : MyPhysicalItemDefinition
    {
        public int MaxIntegrity;
        public float DropProbability;
        public float DeconstructionEfficiency;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ComponentDefinition definition = builder as MyObjectBuilder_ComponentDefinition;
            this.MaxIntegrity = definition.MaxIntegrity;
            this.DropProbability = definition.DropProbability;
            this.DeconstructionEfficiency = definition.DeconstructionEfficiency;
        }
    }
}

