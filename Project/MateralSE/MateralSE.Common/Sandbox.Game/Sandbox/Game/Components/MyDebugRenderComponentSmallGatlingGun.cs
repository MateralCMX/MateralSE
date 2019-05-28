namespace Sandbox.Game.Components
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Weapons;
    using System;
    using VRageMath;

    internal class MyDebugRenderComponentSmallGatlingGun : MyDebugRenderComponent
    {
        private MySmallGatlingGun m_gatlingGun;

        public MyDebugRenderComponentSmallGatlingGun(MySmallGatlingGun gatlingGun) : base(gatlingGun)
        {
            this.m_gatlingGun = gatlingGun;
        }

        public override void DebugDraw()
        {
            this.m_gatlingGun.ConveyorEndpoint.DebugDraw();
            MyResourceSinkComponent component = this.m_gatlingGun.Components.Get<MyResourceSinkComponent>();
            if (component != null)
            {
                component.DebugDraw((Matrix) this.m_gatlingGun.PositionComp.WorldMatrix);
            }
        }
    }
}

