namespace Sandbox.Game.Components
{
    using Sandbox.Game.EntityComponents;
    using System;
    using VRage.ModAPI;
    using VRageMath;

    public class MyDebugRenderComponentDrawPowerSource : MyDebugRenderComponent
    {
        private readonly MyResourceSourceComponent m_source;
        private IMyEntity m_entity;

        public MyDebugRenderComponentDrawPowerSource(MyResourceSourceComponent source, IMyEntity entity) : base(null)
        {
            this.m_source = source;
            this.m_entity = entity;
        }

        public override void DebugDraw()
        {
            this.m_source.DebugDraw((Matrix) this.m_entity.PositionComp.WorldMatrix);
        }
    }
}

