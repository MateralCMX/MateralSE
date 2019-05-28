namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_MechanicalConnectionBlockBaseDefinition), (Type) null)]
    public class MyMechanicalConnectionBlockBaseDefinition : MyCubeBlockDefinition
    {
        public string TopPart;
        public float SafetyDetach;
        public float SafetyDetachMin;
        public float SafetyDetachMax;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_MechanicalConnectionBlockBaseDefinition definition = builder as MyObjectBuilder_MechanicalConnectionBlockBaseDefinition;
            this.TopPart = definition.TopPart ?? definition.RotorPart;
            this.SafetyDetach = definition.SafetyDetach;
            this.SafetyDetachMin = definition.SafetyDetachMin;
            this.SafetyDetachMax = definition.SafetyDetachMax;
        }
    }
}

