namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponentCubeBlock : MyDebugRenderComponent
    {
        private MyCubeBlock m_cubeBlock;

        public MyDebugRenderComponentCubeBlock(MyCubeBlock cubeBlock) : base(cubeBlock)
        {
            this.m_cubeBlock = cubeBlock;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CUBE_BLOCK_AABBS)
            {
                Color red = Color.Red;
                Color green = Color.Green;
                Vector3I center = this.m_cubeBlock.BlockDefinition.Center;
                Vector3 max = (this.m_cubeBlock.Max * this.m_cubeBlock.CubeGrid.GridSize) + new Vector3(this.m_cubeBlock.CubeGrid.GridSize / 2f);
                BoundingBoxD localbox = new BoundingBoxD((Vector3D) ((this.m_cubeBlock.Min * this.m_cubeBlock.CubeGrid.GridSize) - new Vector3(this.m_cubeBlock.CubeGrid.GridSize / 2f)), max);
                MyStringId? faceMaterial = null;
                faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref this.m_cubeBlock.CubeGrid.WorldMatrix, ref localbox, ref red, MySimpleObjectRasterizer.Wireframe, 1, 0.01f, faceMaterial, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
            }
        }
    }
}

