namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.RenderDirect.ActorComponents;
    using System;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentThrust : MyDebugRenderComponent
    {
        private MyThrust m_thrust;

        public MyDebugRenderComponentThrust(MyThrust thrust) : base(thrust)
        {
            this.m_thrust = thrust;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_THRUSTER_DAMAGE)
            {
                this.DebugDrawDamageArea();
            }
        }

        private void DebugDrawDamageArea()
        {
            if ((this.m_thrust.CurrentStrength != 0f) || MyFakes.INACTIVE_THRUSTER_DMG)
            {
                foreach (MyThrustFlameAnimator.FlameInfo info in this.m_thrust.Flames)
                {
                    MatrixD worldMatrix = this.m_thrust.WorldMatrix;
                    LineD damageCapsuleLine = this.m_thrust.GetDamageCapsuleLine(info, ref worldMatrix);
                    MyRenderProxy.DebugDrawCapsule(damageCapsuleLine.From, damageCapsuleLine.To, info.Radius * this.m_thrust.FlameDamageLengthScale, Color.Red, false, false, false);
                }
            }
        }
    }
}

