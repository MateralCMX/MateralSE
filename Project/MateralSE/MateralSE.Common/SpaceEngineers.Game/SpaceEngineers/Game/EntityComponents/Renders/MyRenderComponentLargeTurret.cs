namespace SpaceEngineers.Game.EntityComponents.Renders
{
    using Sandbox.Game.Components;
    using Sandbox.Game.Weapons;
    using System;

    internal class MyRenderComponentLargeTurret : MyRenderComponentCubeBlock
    {
        private MyLargeTurretBase m_turretBase;

        public override void Draw()
        {
            if (this.m_turretBase.IsWorking)
            {
                base.Draw();
                if (this.m_turretBase.Barrel != null)
                {
                    this.m_turretBase.Barrel.Draw();
                }
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_turretBase = base.Container.Entity as MyLargeTurretBase;
        }
    }
}

