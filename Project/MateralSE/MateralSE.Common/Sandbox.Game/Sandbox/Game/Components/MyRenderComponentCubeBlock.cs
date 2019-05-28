namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRageMath;

    public class MyRenderComponentCubeBlock : MyRenderComponent
    {
        protected MyCubeBlock m_cubeBlock;

        public override void AddRenderObjects()
        {
            this.CalculateBlockDepthBias(this.m_cubeBlock);
            base.AddRenderObjects();
            this.UpdateGridParent();
        }

        public override void InvalidateRenderObjects()
        {
            this.m_cubeBlock.InvalidateOnMove = false;
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_cubeBlock = base.Container.Entity as MyCubeBlock;
            this.NeedsDraw = false;
            base.NeedsDrawFromParent = false;
            base.NeedForDrawFromParentChanged = (Action) Delegate.Combine(base.NeedForDrawFromParentChanged, new Action(this.OnNeedForDrawFromParentChanged));
        }

        private void OnNeedForDrawFromParentChanged()
        {
            if (((this.m_cubeBlock.SlimBlock != null) && (this.m_cubeBlock.CubeGrid != null)) && (this.m_cubeBlock.CubeGrid.BlocksForDraw.Contains(this.m_cubeBlock) != base.NeedsDrawFromParent))
            {
                if (base.NeedsDrawFromParent)
                {
                    this.m_cubeBlock.CubeGrid.BlocksForDraw.Add(this.m_cubeBlock);
                }
                else
                {
                    this.m_cubeBlock.CubeGrid.BlocksForDraw.Remove(this.m_cubeBlock);
                }
                this.m_cubeBlock.Render.SetVisibilityUpdates(base.NeedsDrawFromParent);
                this.m_cubeBlock.CubeGrid.MarkForDraw();
            }
        }

        protected void UpdateGridParent()
        {
            if (MyFakes.MANUAL_CULL_OBJECTS)
            {
                MyCubeGridRenderCell orAddCell = this.m_cubeBlock.CubeGrid.RenderData.GetOrAddCell((Vector3) (this.m_cubeBlock.Position * this.m_cubeBlock.CubeGrid.GridSize), true);
                if (orAddCell.ParentCullObject == uint.MaxValue)
                {
                    orAddCell.RebuildInstanceParts(this.GetRenderFlags());
                }
                for (int i = 0; i < base.m_renderObjectIDs.Length; i++)
                {
                    base.SetParent(i, orAddCell.ParentCullObject, new Matrix?(base.Entity.PositionComp.LocalMatrix));
                }
            }
        }
    }
}

