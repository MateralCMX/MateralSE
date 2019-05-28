namespace Sandbox.Game.AI.Pathfinding
{
    using Havok;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Entity;
    using VRage.Game.Voxels;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    internal class MyNavigationInputMesh
    {
        private static IcoSphereMesh m_icosphereMesh = new IcoSphereMesh();
        private static CapsuleMesh m_capsuleMesh = new CapsuleMesh();
        [ThreadStatic]
        private static WorldVerticesInfo m_worldVerticesInfoPerThread;
        private static Dictionary<string, BoundingBoxD> m_cachedBoxes = new Dictionary<string, BoundingBoxD>();
        [ThreadStatic]
        private static List<HkShape> m_tmpShapes;
        private const int NAVMESH_LOD = 0;
        private Dictionary<Vector3I, MyIsoMesh> m_meshCache = new Dictionary<Vector3I, MyIsoMesh>(0x400, new Vector3I.EqualityComparer());
        private List<CacheInterval> m_invalidateMeshCacheCoord = new List<CacheInterval>();
        private List<CacheInterval> m_tmpInvalidCache = new List<CacheInterval>();
        private MyPlanet m_planet;
        private Vector3D m_center;
        private Quaternion rdWorldQuaternion;
        private MyRDPathfinding m_rdPathfinding;
        private List<GridInfo> m_lastGridsInfo = new List<GridInfo>();
        private List<CubeInfo> m_lastIntersectedGridsInfoCubes = new List<CubeInfo>();

        public MyNavigationInputMesh(MyRDPathfinding rdPathfinding, MyPlanet planet, Vector3D center)
        {
            this.m_rdPathfinding = rdPathfinding;
            this.m_planet = planet;
            this.m_center = center;
            Vector3 v = (Vector3) -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(this.m_center));
            Vector3 forward = Vector3.CalculatePerpendicularVector(v);
            this.rdWorldQuaternion = Quaternion.Inverse(Quaternion.CreateFromForwardUp(forward, v));
        }

        private unsafe void AddEntities(float border, Vector3D originPosition, MyOrientedBoundingBoxD obb, List<BoundingBoxD> boundingBoxes, List<MyVoxelMap> trackedEntities)
        {
            Vector3D* vectordPtr1 = (Vector3D*) ref obb.HalfExtent;
            vectordPtr1[0] += new Vector3D((double) border, 0.0, (double) border);
            BoundingBoxD aABB = obb.GetAABB();
            List<VRage.Game.Entity.MyEntity> result = new List<VRage.Game.Entity.MyEntity>();
            MyGamePruningStructure.GetAllEntitiesInBox(ref aABB, result, MyEntityQueryType.Both);
            if (result.Count<VRage.Game.Entity.MyEntity>(e => (e is MyCubeGrid)) > 0)
            {
                this.m_lastGridsInfo.Clear();
                this.m_lastIntersectedGridsInfoCubes.Clear();
            }
            foreach (VRage.Game.Entity.MyEntity entity in result)
            {
                using (entity.Pin())
                {
                    if (entity.MarkedForClose)
                    {
                        continue;
                    }
                    MyCubeGrid grid = entity as MyCubeGrid;
                    if (grid != null)
                    {
                        bool isStatic = grid.IsStatic;
                    }
                    MyVoxelMap item = entity as MyVoxelMap;
                    if (item != null)
                    {
                        trackedEntities.Add(item);
                        this.AddVoxelVertices(item, border, originPosition, obb, boundingBoxes);
                    }
                }
            }
        }

        private unsafe void AddGridVerticesInsideOBB(MyCubeGrid grid, MyOrientedBoundingBoxD obb)
        {
            BoundingBoxD aABB = obb.GetAABB();
            using (HashSet<MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node>.Enumerator enumerator = MyCubeGridGroups.Static.Logical.GetGroup(grid).Nodes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyCubeGrid nodeData = enumerator.Current.NodeData;
                    this.m_rdPathfinding.AddToTrackedGrids(nodeData);
                    MatrixD worldMatrix = nodeData.WorldMatrix;
                    MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                    xdPtr1.Translation -= this.m_center;
                    MatrixD xd3 = MatrixD.Transform(worldMatrix, this.rdWorldQuaternion);
                    if (MyPerGameSettings.Game == GameEnum.SE_GAME)
                    {
                        BoundingBoxD xd4 = aABB.TransformFast(nodeData.PositionComp.WorldMatrixNormalizedInv);
                        Vector3I vectori = new Vector3I((int) Math.Round(xd4.Min.X), (int) Math.Round(xd4.Min.Y), (int) Math.Round(xd4.Min.Z));
                        Vector3I vectori2 = new Vector3I((int) Math.Round(xd4.Max.X), (int) Math.Round(xd4.Max.Y), (int) Math.Round(xd4.Max.Z));
                        vectori = Vector3I.Min(vectori, vectori2);
                        vectori2 = Vector3I.Max(vectori, vectori2);
                        if (nodeData.Physics != null)
                        {
                            using (MyUtils.ReuseCollection<HkShape>(ref m_tmpShapes))
                            {
                                MyGridShape shape = nodeData.Physics.Shape;
                                using (MyGridShape.NativeShapeLock.AcquireSharedUsing())
                                {
                                    shape.GetShapesInInterval(vectori, vectori2, m_tmpShapes);
                                    foreach (HkShape shape2 in m_tmpShapes)
                                    {
                                        this.AddPhysicalShape(shape2, (Matrix) xd3);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddGround(float border, Vector3D originPosition, MyOrientedBoundingBoxD obb, List<BoundingBoxD> bbList)
        {
            if (this.SetTerrainLimits(ref obb))
            {
                this.AddVoxelMesh(this.m_planet, this.m_planet.Storage, this.m_meshCache, border, originPosition, obb, bbList);
            }
        }

        private unsafe void AddMeshTriangles(MyIsoMesh mesh, Vector3 offset, Matrix rotation, Matrix ownRotation)
        {
            for (int i = 0; i < mesh.TrianglesCount; i++)
            {
                ushort num2 = mesh.Triangles[i].V0;
                ushort num3 = mesh.Triangles[i].V1;
                ushort num4 = mesh.Triangles[i].V2;
                m_worldVertices.Triangles.Add(m_worldVertices.VerticesMaxValue + num4);
                m_worldVertices.Triangles.Add(m_worldVertices.VerticesMaxValue + num3);
                m_worldVertices.Triangles.Add(m_worldVertices.VerticesMaxValue + num2);
            }
            for (int j = 0; j < mesh.VerticesCount; j++)
            {
                Vector3 vector;
                mesh.GetUnpackedPosition(j, out vector);
                Vector3* vectorPtr1 = (Vector3*) ref vector;
                Vector3.Transform(ref (Vector3) ref vectorPtr1, ref ownRotation, out vector);
                vector -= offset;
                Vector3* vectorPtr2 = (Vector3*) ref vector;
                Vector3.Transform(ref (Vector3) ref vectorPtr2, ref rotation, out vector);
                m_worldVertices.Vertices.Add(vector);
            }
            WorldVerticesInfo worldVertices = m_worldVertices;
            worldVertices.VerticesMaxValue += mesh.VerticesCount;
        }

        private unsafe void AddPhysicalShape(HkShape shape, Matrix rdWorldMatrix)
        {
            switch (shape.ShapeType)
            {
                case HkShapeType.Sphere:
                {
                    HkSphereShape shape7 = (HkSphereShape) shape;
                    m_icosphereMesh.AddTrianglesToWorldVertices(rdWorldMatrix.Translation, shape7.Radius);
                    return;
                }
                case HkShapeType.Cylinder:
                case HkShapeType.Triangle:
                case HkShapeType.TriSampledHeightFieldCollection:
                case HkShapeType.TriSampledHeightFieldBvTree:
                    break;

                case HkShapeType.Box:
                {
                    HkBoxShape shape2 = (HkBoxShape) shape;
                    Vector3D min = new Vector3D((double) -shape2.HalfExtents.X, (double) -shape2.HalfExtents.Y, (double) -shape2.HalfExtents.Z);
                    Vector3D max = new Vector3D((double) shape2.HalfExtents.X, (double) shape2.HalfExtents.Y, (double) shape2.HalfExtents.Z);
                    BoundingBoxD bbox = new BoundingBoxD(min, max);
                    this.BoundingBoxToTranslatedTriangles(bbox, rdWorldMatrix);
                    return;
                }
                case HkShapeType.Capsule:
                    return;

                case HkShapeType.ConvexVertices:
                {
                    Vector3 vector;
                    HkConvexVerticesShape shape9 = (HkConvexVerticesShape) shape;
                    HkGeometry geometry = new HkGeometry();
                    shape9.GetGeometry(geometry, out vector);
                    int triangleIndex = 0;
                    while (true)
                    {
                        int num2;
                        int num3;
                        int num4;
                        int num5;
                        if (triangleIndex >= geometry.TriangleCount)
                        {
                            int vertexIndex = 0;
                            while (true)
                            {
                                if (vertexIndex >= geometry.VertexCount)
                                {
                                    WorldVerticesInfo worldVertices = m_worldVertices;
                                    worldVertices.VerticesMaxValue += geometry.VertexCount;
                                    break;
                                }
                                Vector3 vertex = geometry.GetVertex(vertexIndex);
                                Vector3* vectorPtr1 = (Vector3*) ref vertex;
                                Vector3.Transform(ref (Vector3) ref vectorPtr1, ref rdWorldMatrix, out vertex);
                                m_worldVertices.Vertices.Add(vertex);
                                vertexIndex++;
                            }
                            break;
                        }
                        geometry.GetTriangle(triangleIndex, out num2, out num3, out num4, out num5);
                        m_worldVertices.Triangles.Add(m_worldVertices.VerticesMaxValue + num2);
                        m_worldVertices.Triangles.Add(m_worldVertices.VerticesMaxValue + num3);
                        m_worldVertices.Triangles.Add(m_worldVertices.VerticesMaxValue + num4);
                        triangleIndex++;
                    }
                    break;
                }
                case HkShapeType.List:
                {
                    HkShapeContainerIterator iterator = ((HkListShape) shape).GetIterator();
                    while (iterator.IsValid)
                    {
                        this.AddPhysicalShape(iterator.CurrentValue, rdWorldMatrix);
                        iterator.Next();
                    }
                    return;
                }
                case HkShapeType.Mopp:
                {
                    HkMoppBvTreeShape shape4 = (HkMoppBvTreeShape) shape;
                    this.AddPhysicalShape((HkShape) shape4.ShapeCollection, rdWorldMatrix);
                    return;
                }
                case HkShapeType.ConvexTranslate:
                {
                    HkConvexTranslateShape shape6 = (HkConvexTranslateShape) shape;
                    Matrix matrix = Matrix.CreateTranslation(shape6.Translation);
                    this.AddPhysicalShape((HkShape) shape6.ChildShape, matrix * rdWorldMatrix);
                    return;
                }
                case HkShapeType.ConvexTransform:
                {
                    HkConvexTransformShape shape5 = (HkConvexTransformShape) shape;
                    this.AddPhysicalShape((HkShape) shape5.ChildShape, shape5.Transform * rdWorldMatrix);
                    return;
                }
                default:
                    return;
            }
        }

        private unsafe void AddVoxelMesh(MyVoxelBase voxelBase, IMyStorage storage, Dictionary<Vector3I, MyIsoMesh> cache, float border, Vector3D originPosition, MyOrientedBoundingBoxD obb, List<BoundingBoxD> bbList)
        {
            Vector3I vectori3;
            Vector3I vectori4;
            bool flag = cache != null;
            if (flag)
            {
                this.CheckCacheValidity();
            }
            Vector3D* vectordPtr1 = (Vector3D*) ref obb.HalfExtent;
            vectordPtr1[0] += new Vector3D((double) border, 0.0, (double) border);
            BoundingBoxD aABB = obb.GetAABB();
            int num = (int) Math.Round((double) (aABB.HalfExtents.Max() * 2.0));
            BoundingBoxD* xdPtr1 = (BoundingBoxD*) ref aABB;
            xdPtr1 = (BoundingBoxD*) new BoundingBoxD(aABB.Min, aABB.Min + num);
            ((BoundingBoxD*) ref aABB).Translate(obb.Center - aABB.Center);
            bbList.Add(new BoundingBoxD(aABB.Min, aABB.Max));
            aABB = aABB.TransformFast(voxelBase.PositionComp.WorldMatrixInvScaled);
            aABB.Translate(voxelBase.SizeInMetresHalf);
            Vector3I voxelCoord = Vector3I.Round(aABB.Min);
            Vector3I vectori2 = (Vector3I) (voxelCoord + num);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref voxelCoord, out vectori3);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref vectori2, out vectori4);
            MyOrientedBoundingBoxD xd2 = obb;
            xd2.Transform(voxelBase.PositionComp.WorldMatrixInvScaled);
            Vector3D* vectordPtr2 = (Vector3D*) ref xd2.Center;
            vectordPtr2[0] += voxelBase.SizeInMetresHalf;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori3, ref vectori4);
            MyCellCoord coord = new MyCellCoord {
                Lod = 0
            };
            int num2 = 0;
            Vector3 offset = (Vector3) (originPosition - voxelBase.PositionLeftBottomCorner);
            Vector3 up = -Vector3.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(originPosition));
            Matrix rotation = Matrix.CreateFromQuaternion(Quaternion.Inverse(Quaternion.CreateFromForwardUp(Vector3.CalculatePerpendicularVector(up), up)));
            Matrix orientation = (Matrix) voxelBase.PositionComp.WorldMatrix.GetOrientation();
            while (iterator.IsValid())
            {
                BoundingBox box;
                MyIsoMesh mesh;
                if (flag && cache.TryGetValue(iterator.Current, out mesh))
                {
                    if (mesh != null)
                    {
                        this.AddMeshTriangles(mesh, offset, rotation, orientation);
                    }
                    iterator.MoveNext();
                    continue;
                }
                coord.CoordInLod = iterator.Current;
                MyVoxelCoordSystems.GeometryCellCoordToLocalAABB(ref coord.CoordInLod, out box);
                if (!xd2.Intersects(ref box))
                {
                    num2++;
                    iterator.MoveNext();
                }
                else
                {
                    BoundingBoxD item = new BoundingBoxD(box.Min, box.Max).Translate(-voxelBase.SizeInMetresHalf);
                    bbList.Add(item);
                    Vector3I lodVoxelMin = (coord.CoordInLod * 8) - 1;
                    MyIsoMesh mesh2 = MyPrecalcComponent.IsoMesher.Precalc(storage, 0, lodVoxelMin, (Vector3I) (((lodVoxelMin + 8) + 1) + 1), MyStorageDataTypeFlags.Content, 0);
                    if (flag)
                    {
                        cache[iterator.Current] = mesh2;
                    }
                    if (mesh2 != null)
                    {
                        this.AddMeshTriangles(mesh2, offset, rotation, orientation);
                    }
                    iterator.MoveNext();
                }
            }
        }

        private void AddVoxelVertices(MyVoxelMap voxelMap, float border, Vector3D originPosition, MyOrientedBoundingBoxD obb, List<BoundingBoxD> bbList)
        {
            this.AddVoxelMesh(voxelMap, voxelMap.Storage, null, border, originPosition, obb, bbList);
        }

        private unsafe void BoundingBoxToTranslatedTriangles(BoundingBoxD bbox, Matrix worldMatrix)
        {
            Vector3 result = new Vector3(bbox.Min.X, bbox.Max.Y, bbox.Max.Z);
            Vector3 vector2 = new Vector3(bbox.Max.X, bbox.Max.Y, bbox.Max.Z);
            Vector3 vector3 = new Vector3(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);
            Vector3 vector4 = new Vector3(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            Vector3 vector5 = new Vector3(bbox.Min.X, bbox.Min.Y, bbox.Max.Z);
            Vector3 vector6 = new Vector3(bbox.Max.X, bbox.Min.Y, bbox.Max.Z);
            Vector3 vector7 = new Vector3(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            Vector3 vector8 = new Vector3(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            Vector3* vectorPtr1 = (Vector3*) ref result;
            Vector3.Transform(ref (Vector3) ref vectorPtr1, ref worldMatrix, out result);
            Vector3* vectorPtr2 = (Vector3*) ref vector2;
            Vector3.Transform(ref (Vector3) ref vectorPtr2, ref worldMatrix, out vector2);
            Vector3* vectorPtr3 = (Vector3*) ref vector3;
            Vector3.Transform(ref (Vector3) ref vectorPtr3, ref worldMatrix, out vector3);
            Vector3* vectorPtr4 = (Vector3*) ref vector4;
            Vector3.Transform(ref (Vector3) ref vectorPtr4, ref worldMatrix, out vector4);
            Vector3* vectorPtr5 = (Vector3*) ref vector5;
            Vector3.Transform(ref (Vector3) ref vectorPtr5, ref worldMatrix, out vector5);
            Vector3* vectorPtr6 = (Vector3*) ref vector6;
            Vector3.Transform(ref (Vector3) ref vectorPtr6, ref worldMatrix, out vector6);
            Vector3* vectorPtr7 = (Vector3*) ref vector7;
            Vector3.Transform(ref (Vector3) ref vectorPtr7, ref worldMatrix, out vector7);
            Vector3* vectorPtr8 = (Vector3*) ref vector8;
            Vector3.Transform(ref (Vector3) ref vectorPtr8, ref worldMatrix, out vector8);
            m_worldVertices.Vertices.Add(result);
            m_worldVertices.Vertices.Add(vector2);
            m_worldVertices.Vertices.Add(vector3);
            m_worldVertices.Vertices.Add(vector4);
            m_worldVertices.Vertices.Add(vector5);
            m_worldVertices.Vertices.Add(vector6);
            m_worldVertices.Vertices.Add(vector7);
            m_worldVertices.Vertices.Add(vector8);
            int verticesMaxValue = m_worldVertices.VerticesMaxValue;
            int item = m_worldVertices.VerticesMaxValue + 1;
            int num3 = m_worldVertices.VerticesMaxValue + 2;
            int num4 = m_worldVertices.VerticesMaxValue + 3;
            int num5 = m_worldVertices.VerticesMaxValue + 4;
            int num6 = m_worldVertices.VerticesMaxValue + 5;
            int num7 = m_worldVertices.VerticesMaxValue + 6;
            int num8 = m_worldVertices.VerticesMaxValue + 7;
            m_worldVertices.Triangles.Add(num4);
            m_worldVertices.Triangles.Add(num3);
            m_worldVertices.Triangles.Add(verticesMaxValue);
            m_worldVertices.Triangles.Add(verticesMaxValue);
            m_worldVertices.Triangles.Add(item);
            m_worldVertices.Triangles.Add(num4);
            m_worldVertices.Triangles.Add(num5);
            m_worldVertices.Triangles.Add(num7);
            m_worldVertices.Triangles.Add(num8);
            m_worldVertices.Triangles.Add(num8);
            m_worldVertices.Triangles.Add(num6);
            m_worldVertices.Triangles.Add(num5);
            m_worldVertices.Triangles.Add(num3);
            m_worldVertices.Triangles.Add(num8);
            m_worldVertices.Triangles.Add(num7);
            m_worldVertices.Triangles.Add(num3);
            m_worldVertices.Triangles.Add(num4);
            m_worldVertices.Triangles.Add(num8);
            m_worldVertices.Triangles.Add(verticesMaxValue);
            m_worldVertices.Triangles.Add(num5);
            m_worldVertices.Triangles.Add(num6);
            m_worldVertices.Triangles.Add(num6);
            m_worldVertices.Triangles.Add(item);
            m_worldVertices.Triangles.Add(verticesMaxValue);
            m_worldVertices.Triangles.Add(num7);
            m_worldVertices.Triangles.Add(num5);
            m_worldVertices.Triangles.Add(verticesMaxValue);
            m_worldVertices.Triangles.Add(verticesMaxValue);
            m_worldVertices.Triangles.Add(num3);
            m_worldVertices.Triangles.Add(num7);
            m_worldVertices.Triangles.Add(item);
            m_worldVertices.Triangles.Add(num6);
            m_worldVertices.Triangles.Add(num8);
            m_worldVertices.Triangles.Add(num8);
            m_worldVertices.Triangles.Add(num4);
            m_worldVertices.Triangles.Add(item);
            WorldVerticesInfo worldVertices = m_worldVertices;
            worldVertices.VerticesMaxValue += 8;
        }

        private void CheckCacheValidity()
        {
            if (this.m_invalidateMeshCacheCoord.Count > 0)
            {
                this.m_tmpInvalidCache.AddRange(this.m_invalidateMeshCacheCoord);
                this.m_invalidateMeshCacheCoord.Clear();
                foreach (CacheInterval interval in this.m_tmpInvalidCache)
                {
                    for (int i = 0; i < this.m_meshCache.Count; i++)
                    {
                        KeyValuePair<Vector3I, MyIsoMesh> pair = this.m_meshCache.ElementAt<KeyValuePair<Vector3I, MyIsoMesh>>(i);
                        Vector3I key = pair.Key;
                        if (((key.X >= interval.Min.X) && ((key.Y >= interval.Min.Y) && ((key.Z >= interval.Min.Z) && ((key.X <= interval.Max.X) && (key.Y <= interval.Max.Y))))) && (key.Z <= interval.Max.Z))
                        {
                            this.m_meshCache.Remove(key);
                            break;
                        }
                    }
                }
                this.m_tmpInvalidCache.Clear();
            }
        }

        public void Clear()
        {
            this.m_meshCache.Clear();
        }

        private void ClearWorldVertices()
        {
            m_worldVertices.Vertices.Clear();
            m_worldVertices.VerticesMaxValue = 0;
            m_worldVertices.Triangles.Clear();
        }

        public void DebugDraw()
        {
            foreach (GridInfo info in this.m_lastGridsInfo)
            {
                foreach (CubeInfo info2 in info.Cubes)
                {
                    if (this.m_lastIntersectedGridsInfoCubes.Contains(info2))
                    {
                        MyRenderProxy.DebugDrawAABB(info2.BoundingBox, Color.White, 1f, 1f, true, false, false);
                        continue;
                    }
                    MyRenderProxy.DebugDrawAABB(info2.BoundingBox, Color.Yellow, 1f, 1f, true, false, false);
                }
            }
        }

        private unsafe Vector3* GetMiddleOBBLocalPoints(MyOrientedBoundingBoxD obb, ref Vector3* points)
        {
            Vector3 vector = obb.Orientation.Right * ((float) obb.HalfExtent.X);
            Vector3 vector2 = obb.Orientation.Forward * ((float) obb.HalfExtent.Z);
            Vector3 vector3 = (Vector3) (obb.Center - this.m_planet.PositionComp.GetPosition());
            points = (Vector3*) ((vector3 - vector) - vector2);
            points[sizeof(Vector3)] = (vector3 + vector) - vector2;
            points + (((IntPtr) 2) * sizeof(Vector3)) = (Vector3*) ((vector3 + vector) + vector2);
            points + (((IntPtr) 3) * sizeof(Vector3)) = (Vector3*) ((vector3 - vector) + vector2);
            return points;
        }

        public WorldVerticesInfo GetWorldVertices(float border, Vector3D originPosition, MyOrientedBoundingBoxD obb, List<BoundingBoxD> boundingBoxes, List<MyVoxelMap> trackedEntities)
        {
            this.ClearWorldVertices();
            this.AddEntities(border, originPosition, obb, boundingBoxes, trackedEntities);
            this.AddGround(border, originPosition, obb, boundingBoxes);
            return m_worldVertices;
        }

        public void InvalidateCache(BoundingBoxD box)
        {
            Vector3I vectori3;
            Vector3I vectori4;
            Vector3I voxelCoord = new Vector3I(Vector3D.Transform(box.Min, this.m_planet.PositionComp.WorldMatrixInvScaled) + this.m_planet.SizeInMetresHalf);
            Vector3I vectori2 = new Vector3I(Vector3D.Transform(box.Max, this.m_planet.PositionComp.WorldMatrixInvScaled) + this.m_planet.SizeInMetresHalf);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref voxelCoord, out vectori3);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref vectori2, out vectori4);
            CacheInterval item = new CacheInterval {
                Min = vectori3,
                Max = vectori4
            };
            this.m_invalidateMeshCacheCoord.Add(item);
        }

        public void RefreshCache()
        {
            this.m_meshCache.Clear();
        }

        private unsafe bool SetTerrainLimits(ref MyOrientedBoundingBoxD obb)
        {
            float num;
            float num2;
            Vector3* points = (Vector3*) stackalloc byte[(((IntPtr) 4) * sizeof(Vector3))];
            this.GetMiddleOBBLocalPoints(obb, ref points);
            this.m_planet.Provider.Shape.GetBounds(points, 4, out num, out num2);
            if (!num.IsValid() || !num2.IsValid())
            {
                return false;
            }
            Vector3D vectord = (obb.Orientation.Up * num) + this.m_planet.PositionComp.GetPosition();
            obb.Center = (vectord + ((obb.Orientation.Up * num2) + this.m_planet.PositionComp.GetPosition())) * 0.5;
            float num4 = Math.Max((float) (num2 - num), (float) 1f);
            obb.HalfExtent.Y = num4 * 0.5f;
            return true;
        }

        private static WorldVerticesInfo m_worldVertices
        {
            get
            {
                if (m_worldVerticesInfoPerThread == null)
                {
                    m_worldVerticesInfoPerThread = new WorldVerticesInfo();
                }
                return m_worldVerticesInfoPerThread;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyNavigationInputMesh.<>c <>9 = new MyNavigationInputMesh.<>c();
            public static Func<VRage.Game.Entity.MyEntity, bool> <>9__30_0;

            internal bool <AddEntities>b__30_0(VRage.Game.Entity.MyEntity e) => 
                (e is MyCubeGrid);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CacheInterval
        {
            public Vector3I Min;
            public Vector3I Max;
        }

        public class CapsuleMesh
        {
            private const double PId2 = 1.5707963267948966;
            private const double PIm2 = 6.2831853071795862;
            private List<Vector3> m_verticeList = new List<Vector3>();
            private List<int> m_triangleList = new List<int>();
            private int N = 8;
            private float radius = 1f;
            private float height;

            public CapsuleMesh()
            {
                this.Create();
            }

            public void AddTrianglesToWorldVertices(Matrix transformMatrix, float radius, Line axisLine)
            {
                Matrix matrix = Matrix.CreateFromDir(axisLine.Direction);
                Vector3 translation = transformMatrix.Translation;
                transformMatrix.Translation = Vector3.Zero;
                int num = this.m_verticeList.Count / 2;
                Vector3 vector2 = new Vector3(0f, 0f, axisLine.Length * 0.5f);
                for (int i = 0; i < num; i++)
                {
                    MyNavigationInputMesh.m_worldVertices.Vertices.Add(Vector3.Transform((translation + (this.m_verticeList[i] * radius)) - vector2, matrix));
                }
                for (int j = num; j < this.m_verticeList.Count; j++)
                {
                    MyNavigationInputMesh.m_worldVertices.Vertices.Add(Vector3.Transform((translation + (this.m_verticeList[j] * radius)) + vector2, matrix));
                }
                foreach (int num4 in this.m_triangleList)
                {
                    MyNavigationInputMesh.m_worldVertices.Triangles.Add(MyNavigationInputMesh.m_worldVertices.VerticesMaxValue + num4);
                }
                MyNavigationInputMesh.WorldVerticesInfo worldVertices = MyNavigationInputMesh.m_worldVertices;
                worldVertices.VerticesMaxValue += this.m_verticeList.Count;
            }

            private void Create()
            {
                int num;
                double num7;
                double num8;
                int num2 = 0;
                while (num2 <= (this.N / 4))
                {
                    num = 0;
                    while (true)
                    {
                        if (num > this.N)
                        {
                            num2++;
                            break;
                        }
                        Vector3 item = new Vector3();
                        num7 = (num * 6.2831853071795862) / ((double) this.N);
                        num8 = -1.5707963267948966 + ((3.1415926535897931 * num2) / ((double) (this.N / 2)));
                        item.X = this.radius * ((float) (Math.Cos(num8) * Math.Cos(num7)));
                        item.Y = this.radius * ((float) (Math.Cos(num8) * Math.Sin(num7)));
                        item.Z = (this.radius * ((float) Math.Sin(num8))) - (this.height / 2f);
                        this.m_verticeList.Add(item);
                        num++;
                    }
                }
                num2 = this.N / 4;
                while (num2 <= (this.N / 2))
                {
                    num = 0;
                    while (true)
                    {
                        if (num > this.N)
                        {
                            num2++;
                            break;
                        }
                        Vector3 item = new Vector3();
                        num7 = (num * 6.2831853071795862) / ((double) this.N);
                        num8 = -1.5707963267948966 + ((3.1415926535897931 * num2) / ((double) (this.N / 2)));
                        item.X = this.radius * ((float) (Math.Cos(num8) * Math.Cos(num7)));
                        item.Y = this.radius * ((float) (Math.Cos(num8) * Math.Sin(num7)));
                        item.Z = (this.radius * ((float) Math.Sin(num8))) + (this.height / 2f);
                        this.m_verticeList.Add(item);
                        num++;
                    }
                }
                num2 = 0;
                while (num2 <= (this.N / 2))
                {
                    num = 0;
                    while (true)
                    {
                        if (num >= this.N)
                        {
                            num2++;
                            break;
                        }
                        int item = (num2 * (this.N + 1)) + num;
                        int num4 = (num2 * (this.N + 1)) + (num + 1);
                        int num5 = ((num2 + 1) * (this.N + 1)) + (num + 1);
                        int num6 = ((num2 + 1) * (this.N + 1)) + num;
                        this.m_triangleList.Add(item);
                        this.m_triangleList.Add(num4);
                        this.m_triangleList.Add(num5);
                        this.m_triangleList.Add(item);
                        this.m_triangleList.Add(num5);
                        this.m_triangleList.Add(num6);
                        num++;
                    }
                }
            }
        }

        public class CubeInfo
        {
            public int ID { get; set; }

            public BoundingBoxD BoundingBox { get; set; }

            public List<Vector3D> TriangleVertices { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GridInfo
        {
            public long ID { get; set; }
            public List<MyNavigationInputMesh.CubeInfo> Cubes { get; set; }
        }

        public class IcoSphereMesh
        {
            private const int RECURSION_LEVEL = 1;
            private int index;
            private Dictionary<long, int> middlePointIndexCache;
            private List<int> triangleIndices;
            private List<Vector3> positions;

            public IcoSphereMesh()
            {
                this.create();
            }

            public void AddTrianglesToWorldVertices(Vector3 center, float radius)
            {
                foreach (int num in this.triangleIndices)
                {
                    MyNavigationInputMesh.m_worldVertices.Triangles.Add(MyNavigationInputMesh.m_worldVertices.VerticesMaxValue + num);
                }
                foreach (Vector3 vector in this.positions)
                {
                    MyNavigationInputMesh.m_worldVertices.Vertices.Add(center + (vector * radius));
                }
                MyNavigationInputMesh.WorldVerticesInfo worldVertices = MyNavigationInputMesh.m_worldVertices;
                worldVertices.VerticesMaxValue += this.positions.Count;
            }

            private int addVertex(Vector3 p)
            {
                double num = Math.Sqrt((double) (((p.X * p.X) + (p.Y * p.Y)) + (p.Z * p.Z)));
                this.positions.Add(new Vector3(((double) p.X) / num, ((double) p.Y) / num, ((double) p.Z) / num));
                int index = this.index;
                this.index = index + 1;
                return index;
            }

            private void create()
            {
                this.middlePointIndexCache = new Dictionary<long, int>();
                this.triangleIndices = new List<int>();
                this.positions = new List<Vector3>();
                this.index = 0;
                double y = (1.0 + Math.Sqrt(5.0)) / 2.0;
                this.addVertex(new Vector3(-1.0, y, 0.0));
                this.addVertex(new Vector3(1.0, y, 0.0));
                this.addVertex(new Vector3(-1.0, -y, 0.0));
                this.addVertex(new Vector3(1.0, -y, 0.0));
                this.addVertex(new Vector3(0.0, -1.0, y));
                this.addVertex(new Vector3(0.0, 1.0, y));
                this.addVertex(new Vector3(0.0, -1.0, -y));
                this.addVertex(new Vector3(0.0, 1.0, -y));
                this.addVertex(new Vector3(y, 0.0, -1.0));
                this.addVertex(new Vector3(y, 0.0, 1.0));
                this.addVertex(new Vector3(-y, 0.0, -1.0));
                this.addVertex(new Vector3(-y, 0.0, 1.0));
                List<TriangleIndices> list = new List<TriangleIndices> {
                    new TriangleIndices(0, 11, 5),
                    new TriangleIndices(0, 5, 1),
                    new TriangleIndices(0, 1, 7),
                    new TriangleIndices(0, 7, 10),
                    new TriangleIndices(0, 10, 11),
                    new TriangleIndices(1, 5, 9),
                    new TriangleIndices(5, 11, 4),
                    new TriangleIndices(11, 10, 2),
                    new TriangleIndices(10, 7, 6),
                    new TriangleIndices(7, 1, 8),
                    new TriangleIndices(3, 9, 4),
                    new TriangleIndices(3, 4, 2),
                    new TriangleIndices(3, 2, 6),
                    new TriangleIndices(3, 6, 8),
                    new TriangleIndices(3, 8, 9),
                    new TriangleIndices(4, 9, 5),
                    new TriangleIndices(2, 4, 11),
                    new TriangleIndices(6, 2, 10),
                    new TriangleIndices(8, 6, 7),
                    new TriangleIndices(9, 8, 1)
                };
                for (int i = 0; i < 1; i++)
                {
                    List<TriangleIndices> list2 = new List<TriangleIndices>();
                    foreach (TriangleIndices indices in list)
                    {
                        int num3 = this.getMiddlePoint(indices.v1, indices.v2);
                        int num4 = this.getMiddlePoint(indices.v2, indices.v3);
                        int num5 = this.getMiddlePoint(indices.v3, indices.v1);
                        list2.Add(new TriangleIndices(indices.v1, num3, num5));
                        list2.Add(new TriangleIndices(indices.v2, num4, num3));
                        list2.Add(new TriangleIndices(indices.v3, num5, num4));
                        list2.Add(new TriangleIndices(num3, num4, num5));
                    }
                    list = list2;
                }
                foreach (TriangleIndices indices2 in list)
                {
                    this.triangleIndices.Add(indices2.v1);
                    this.triangleIndices.Add(indices2.v2);
                    this.triangleIndices.Add(indices2.v3);
                }
            }

            private int getMiddlePoint(int p1, int p2)
            {
                int num4;
                bool flag1 = p1 < p2;
                long num = flag1 ? ((long) p1) : ((long) p2);
                long num2 = flag1 ? ((long) p2) : ((long) p1);
                long key = (num << 0x20) + num2;
                if (this.middlePointIndexCache.TryGetValue(key, out num4))
                {
                    return num4;
                }
                Vector3 vector = this.positions[p1];
                Vector3 vector2 = this.positions[p2];
                Vector3 p = new Vector3(((double) (vector.X + vector2.X)) / 2.0, ((double) (vector.Y + vector2.Y)) / 2.0, ((double) (vector.Z + vector2.Z)) / 2.0);
                int num5 = this.addVertex(p);
                this.middlePointIndexCache.Add(key, num5);
                return num5;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct TriangleIndices
            {
                public int v1;
                public int v2;
                public int v3;
                public TriangleIndices(int v1, int v2, int v3)
                {
                    this.v1 = v1;
                    this.v2 = v2;
                    this.v3 = v3;
                }
            }
        }

        public class WorldVerticesInfo
        {
            public List<Vector3> Vertices = new List<Vector3>();
            public int VerticesMaxValue;
            public List<int> Triangles = new List<int>();
        }
    }
}

