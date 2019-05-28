namespace SpaceEngineers.Game.EntityComponents.DebugRenders
{
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using SpaceEngineers.Game.EntityComponents.GameLogic;
    using System;
    using VRage.Game.Components;

    public class MyDebugRenderComponentSolarPanel : MyDebugRenderComponent
    {
        private MyTerminalBlock m_solarBlock;
        private MySolarGameLogicComponent m_solarComponent;

        public MyDebugRenderComponentSolarPanel(MyTerminalBlock solarBlock) : base(solarBlock)
        {
            MyGameLogicComponent component;
            this.m_solarBlock = solarBlock;
            if (this.m_solarBlock.Components.TryGet<MyGameLogicComponent>(out component))
            {
                this.m_solarComponent = component as MySolarGameLogicComponent;
            }
            MySolarGameLogicComponent solarComponent = this.m_solarComponent;
        }

        public override void DebugDraw()
        {
        }
    }
}

