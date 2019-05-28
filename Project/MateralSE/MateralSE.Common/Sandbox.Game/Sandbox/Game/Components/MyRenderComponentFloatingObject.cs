namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Utils;
    using VRageRender;

    internal class MyRenderComponentFloatingObject : MyRenderComponent
    {
        private MyFloatingObject m_floatingObject;

        public override void AddRenderObjects()
        {
            if (this.m_floatingObject.VoxelMaterial == null)
            {
                base.AddRenderObjects();
            }
            else if (base.m_renderObjectIDs[0] == uint.MaxValue)
            {
                this.SetRenderObjectID(0, MyRenderProxy.CreateRenderVoxelDebris("Voxel debris", base.Model.AssetName, base.Container.Entity.PositionComp.WorldMatrix, 5f, 8f, MyUtils.GetRandomFloat(0f, 2f), this.m_floatingObject.VoxelMaterial.Index, base.FadeIn));
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_floatingObject = base.Container.Entity as MyFloatingObject;
        }
    }
}

