namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ShipDrillDefinition), (Type) null)]
    public class MyShipDrillDefinition : MyShipToolDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float CutOutOffset;
        public float CutOutRadius;
        public Vector3D ParticleOffset;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ShipDrillDefinition definition = builder as MyObjectBuilder_ShipDrillDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.CutOutOffset = definition.CutOutOffset;
            this.CutOutRadius = definition.CutOutRadius;
            this.ParticleOffset = definition.ParticleOffset;
        }
    }
}

