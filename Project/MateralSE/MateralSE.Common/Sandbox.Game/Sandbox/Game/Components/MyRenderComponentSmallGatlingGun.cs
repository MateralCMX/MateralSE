namespace Sandbox.Game.Components
{
    using Sandbox.Game.Weapons;
    using System;

    internal class MyRenderComponentSmallGatlingGun : MyRenderComponentCubeBlock
    {
        private MySmallGatlingGun m_gatlingGun;

        public override void Draw()
        {
            base.Draw();
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_gatlingGun = base.Container.Entity as MySmallGatlingGun;
        }
    }
}

