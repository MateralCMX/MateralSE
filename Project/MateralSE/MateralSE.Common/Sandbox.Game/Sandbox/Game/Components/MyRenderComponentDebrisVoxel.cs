namespace Sandbox.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;

    internal class MyRenderComponentDebrisVoxel : MyRenderComponent
    {
        public override void AddRenderObjects()
        {
            if (base.m_renderObjectIDs[0] == uint.MaxValue)
            {
                this.SetRenderObjectID(0, MyRenderProxy.CreateRenderVoxelDebris("Voxel debris", base.Model.AssetName, base.Container.Entity.PositionComp.WorldMatrix, this.TexCoordOffset, this.TexCoordScale, 1f, this.VoxelMaterialIndex, base.FadeIn));
            }
        }

        public float TexCoordOffset { get; set; }

        public float TexCoordScale { get; set; }

        public byte VoxelMaterialIndex { get; set; }
    }
}

