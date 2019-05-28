namespace Sandbox.Definitions.GUI
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_HudDefinition), (Type) null)]
    public class MyHudDefinition : MyDefinitionBase
    {
        private MyObjectBuilder_ToolbarControlVisualStyle m_toolbar;
        private MyObjectBuilder_StatControls[] m_statControlses;
        private MyObjectBuilder_GravityIndicatorVisualStyle m_gravityIndicator;
        private MyObjectBuilder_CrosshairStyle m_crosshair;
        private Vector2I? m_optimalScreenRatio;
        private float? m_customUIScale;
        private MyStringHash? m_visorOverlayTexture;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_HudDefinition definition = builder as MyObjectBuilder_HudDefinition;
            this.m_toolbar = definition.Toolbar;
            this.m_statControlses = definition.StatControls;
            this.m_gravityIndicator = definition.GravityIndicator;
            this.m_crosshair = definition.Crosshair;
            this.m_optimalScreenRatio = definition.OptimalScreenRatio;
            this.m_customUIScale = definition.CustomUIScale;
            this.m_visorOverlayTexture = definition.VisorOverlayTexture;
        }

        public MyObjectBuilder_ToolbarControlVisualStyle Toolbar =>
            this.m_toolbar;

        public MyObjectBuilder_StatControls[] StatControls =>
            this.m_statControlses;

        public MyObjectBuilder_GravityIndicatorVisualStyle GravityIndicator =>
            this.m_gravityIndicator;

        public MyObjectBuilder_CrosshairStyle Crosshair =>
            this.m_crosshair;

        public Vector2I? OptimalScreenRatio =>
            this.m_optimalScreenRatio;

        public float? CustomUIScale =>
            this.m_customUIScale;

        public MyStringHash? VisorOverlayTexture =>
            this.m_visorOverlayTexture;
    }
}

