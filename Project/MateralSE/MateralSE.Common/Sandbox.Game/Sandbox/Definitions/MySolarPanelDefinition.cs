namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_SolarPanelDefinition), (Type) null)]
    public class MySolarPanelDefinition : MyPowerProducerDefinition
    {
        public Vector3 PanelOrientation;
        public bool IsTwoSided;
        public float PanelOffset;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SolarPanelDefinition definition = builder as MyObjectBuilder_SolarPanelDefinition;
            this.PanelOrientation = definition.PanelOrientation;
            this.IsTwoSided = definition.TwoSidedPanel;
            this.PanelOffset = definition.PanelOffset;
        }
    }
}

