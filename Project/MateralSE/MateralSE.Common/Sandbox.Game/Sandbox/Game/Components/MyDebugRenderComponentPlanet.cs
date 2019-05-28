namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using VRage;
    using VRage.Game.Components;
    using VRage.Game.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentPlanet : MyDebugRenderComponent
    {
        private MyPlanet m_planet;

        public MyDebugRenderComponentPlanet(MyPlanet voxelMap) : base(voxelMap)
        {
            this.m_planet = voxelMap;
        }

        public override void DebugDraw()
        {
            Vector3D positionLeftBottomCorner = this.m_planet.PositionLeftBottomCorner;
            if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_MAP_AABB)
            {
                this.m_planet.Components.Get<MyPlanetEnvironmentComponent>().DebugDraw();
                this.m_planet.DebugDrawPhysics();
                MyRenderProxy.DebugDrawAABB(this.m_planet.PositionComp.WorldAABB, Color.White, 1f, 1f, true, false, false);
                MyRenderProxy.DebugDrawLine3D(positionLeftBottomCorner, positionLeftBottomCorner + new Vector3(1f, 0f, 0f), Color.Red, Color.Red, true, false);
                MyRenderProxy.DebugDrawLine3D(positionLeftBottomCorner, positionLeftBottomCorner + new Vector3(0f, 1f, 0f), Color.Green, Color.Green, true, false);
                MyRenderProxy.DebugDrawLine3D(positionLeftBottomCorner, positionLeftBottomCorner + new Vector3(0f, 0f, 1f), Color.Blue, Color.Blue, true, false);
                MyRenderProxy.DebugDrawAxis(this.m_planet.PositionComp.WorldMatrix, 2f, false, false, false);
                MyRenderProxy.DebugDrawSphere(this.m_planet.PositionComp.GetPosition(), 1f, Color.OrangeRed, 1f, false, false, true, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_GEOMETRY_CELL)
            {
                MyIntersectionResultLineTriangleEx? nullable;
                MyCamera mainCamera = MySector.MainCamera;
                LineD line = new LineD(mainCamera.Position, mainCamera.Position + (25f * mainCamera.ForwardVector));
                if (this.m_planet.GetIntersectionWithLine(ref line, out nullable, IntersectionFlags.ALL_TRIANGLES))
                {
                    Vector3I vectori;
                    Vector3I vectori2;
                    BoundingBoxD xd2;
                    MyTriangle_Vertices inputTriangle = nullable.Value.Triangle.InputTriangle;
                    MyRenderProxy.DebugDrawTriangle(inputTriangle.Vertex0 + positionLeftBottomCorner, inputTriangle.Vertex1 + positionLeftBottomCorner, inputTriangle.Vertex2 + positionLeftBottomCorner, Color.Red, true, false, false);
                    Vector3D intersectionPointInWorldSpace = nullable.Value.IntersectionPointInWorldSpace;
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(positionLeftBottomCorner, ref intersectionPointInWorldSpace, out vectori2);
                    MyVoxelCoordSystems.VoxelCoordToWorldAABB(positionLeftBottomCorner, ref vectori2, out xd2);
                    MyRenderProxy.DebugDrawAABB(xd2, Vector3.UnitY, 1f, 1f, true, false, false);
                    MyVoxelCoordSystems.WorldPositionToGeometryCellCoord(positionLeftBottomCorner, ref intersectionPointInWorldSpace, out vectori);
                    MyVoxelCoordSystems.GeometryCellCoordToWorldAABB(positionLeftBottomCorner, ref vectori, out xd2);
                    MyRenderProxy.DebugDrawAABB(xd2, Vector3.UnitZ, 1f, 1f, true, false, false);
                }
            }
        }
    }
}

