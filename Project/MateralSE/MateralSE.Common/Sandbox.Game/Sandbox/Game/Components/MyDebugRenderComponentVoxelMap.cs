namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using System;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentVoxelMap : MyDebugRenderComponent
    {
        private MyVoxelBase m_voxelMap;

        public MyDebugRenderComponentVoxelMap(MyVoxelBase voxelMap) : base(voxelMap)
        {
            this.m_voxelMap = voxelMap;
        }

        public override void DebugDraw()
        {
            Vector3D positionLeftBottomCorner = this.m_voxelMap.PositionLeftBottomCorner;
            if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MAP_AABB)
            {
                MyRenderProxy.DebugDrawAABB(this.m_voxelMap.PositionComp.WorldAABB, Color.White, 0.2f, 1f, true, false, false);
                MyRenderProxy.DebugDrawLine3D(positionLeftBottomCorner, positionLeftBottomCorner + new Vector3(1f, 0f, 0f), Color.Red, Color.Red, true, false);
                MyRenderProxy.DebugDrawLine3D(positionLeftBottomCorner, positionLeftBottomCorner + new Vector3(0f, 1f, 0f), Color.Green, Color.Green, true, false);
                MyRenderProxy.DebugDrawLine3D(positionLeftBottomCorner, positionLeftBottomCorner + new Vector3(0f, 0f, 1f), Color.Blue, Color.Blue, true, false);
                MyRenderProxy.DebugDrawAxis(this.m_voxelMap.PositionComp.WorldMatrix, 2f, false, false, false);
                MyRenderProxy.DebugDrawSphere(this.m_voxelMap.PositionComp.GetPosition(), 1f, Color.OrangeRed, 1f, false, false, true, false);
            }
        }
    }
}

