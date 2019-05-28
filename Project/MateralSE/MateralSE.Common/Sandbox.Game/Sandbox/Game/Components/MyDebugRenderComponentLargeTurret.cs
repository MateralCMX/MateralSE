namespace Sandbox.Game.Components
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Weapons;
    using System;
    using VRage.Game.Components;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentLargeTurret : MyDebugRenderComponent
    {
        private MyLargeTurretBase m_turretBase;

        public MyDebugRenderComponentLargeTurret(MyLargeTurretBase turretBase) : base(turretBase)
        {
            this.m_turretBase = turretBase;
        }

        public override void DebugDraw()
        {
            if (this.m_turretBase.Render.GetModel() != null)
            {
                BoundingSphere boundingSphere = this.m_turretBase.Render.GetModel().BoundingSphere;
            }
            Vector3 vector = new Vector3();
            switch (this.m_turretBase.GetStatus())
            {
                case MyLargeTurretBase.MyLargeShipGunStatus.MyWeaponStatus_Deactivated:
                    vector = Color.Green.ToVector3();
                    break;

                case MyLargeTurretBase.MyLargeShipGunStatus.MyWeaponStatus_Searching:
                    vector = Color.Red.ToVector3();
                    break;

                case MyLargeTurretBase.MyLargeShipGunStatus.MyWeaponStatus_Shooting:
                    vector = Color.White.ToVector3();
                    break;

                default:
                    break;
            }
            Color colorFrom = new Color(vector);
            Color colorTo = new Color(vector);
            if (this.m_turretBase.Target != null)
            {
                MyRenderProxy.DebugDrawLine3D(this.m_turretBase.Barrel.Entity.PositionComp.GetPosition(), this.m_turretBase.Target.PositionComp.GetPosition(), colorFrom, colorTo, false, false);
                MyRenderProxy.DebugDrawSphere(this.m_turretBase.Target.PositionComp.GetPosition(), this.m_turretBase.Target.PositionComp.LocalVolume.Radius, Color.White, 1f, false, false, true, false);
            }
            MyResourceSinkComponent component = this.m_turretBase.Components.Get<MyResourceSinkComponent>();
            if (component != null)
            {
                component.DebugDraw((Matrix) this.m_turretBase.PositionComp.WorldMatrix);
            }
            base.DebugDraw();
        }
    }
}

