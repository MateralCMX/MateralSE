namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_GravityGeneratorDefinition), (Type) null)]
    public class MyGravityGeneratorDefinition : MyGravityGeneratorBaseDefinition
    {
        public float RequiredPowerInput;
        public Vector3 MinFieldSize;
        public Vector3 MaxFieldSize;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GravityGeneratorDefinition definition = builder as MyObjectBuilder_GravityGeneratorDefinition;
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.MinFieldSize = (Vector3) definition.MinFieldSize;
            this.MaxFieldSize = (Vector3) definition.MaxFieldSize;
        }
    }
}

