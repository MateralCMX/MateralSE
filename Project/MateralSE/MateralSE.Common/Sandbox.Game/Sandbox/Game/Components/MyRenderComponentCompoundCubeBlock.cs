namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRageMath;
    using VRageRender;

    internal class MyRenderComponentCompoundCubeBlock : MyRenderComponentCubeBlock
    {
        public override void AddRenderObjects()
        {
            this.InvalidateRenderObjects();
        }

        public override void InvalidateRenderObjects()
        {
            base.InvalidateRenderObjects();
            foreach (MySlimBlock block in (base.m_cubeBlock as MyCompoundCubeBlock).GetBlocks())
            {
                if (block.FatBlock == null)
                {
                    continue;
                }
                if ((block.FatBlock.Render.Visible || block.FatBlock.Render.CastShadows) && (block.FatBlock.InScene && block.FatBlock.InvalidateOnMove))
                {
                    uint[] renderObjectIDs = block.FatBlock.Render.RenderObjectIDs;
                    for (int i = 0; i < renderObjectIDs.Length; i++)
                    {
                        MatrixD worldMatrix = block.FatBlock.WorldMatrix;
                        BoundingBox? aabb = null;
                        Matrix? localMatrix = null;
                        MyRenderProxy.UpdateRenderObject(renderObjectIDs[i], new MatrixD?(worldMatrix), aabb, -1, localMatrix);
                    }
                }
            }
        }
    }
}

