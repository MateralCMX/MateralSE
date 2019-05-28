namespace Sandbox.Game.AI.Pathfinding
{
    using Havok;
    using ParallelTasks;
    using RecastDetour;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyExternalPathfinding : IMyPathfinding
    {
        private MyRecastOptions m_recastOptions;
        private List<MyRecastDetourPolygon> m_polygons = new List<MyRecastDetourPolygon>();
        private MyPolyMeshDetail m_polyMesh;
        private Vector3D m_meshCenter;
        private Vector3D m_currentCenter;
        private int m_meshMaxSize;
        private int m_singleTileSize;
        private int m_singleTileHeight;
        private int m_tileLineCount;
        private float m_border;
        private bool m_isNavmeshInitialized;
        private MyNavmeshOBBs m_navmeshOBBs;
        private MyRDWrapper rdWrapper;
        private List<MyRDPath> m_debugDrawPaths = new List<MyRDPath>();
        private List<BoundingBoxD> m_lastGroundMeshQuery = new List<BoundingBoxD>();
        private Dictionary<string, GeometryCenterPair> m_cachedGeometry = new Dictionary<string, GeometryCenterPair>();
        private bool drawMesh;
        private bool m_isNavmeshCreationRunning;
        private Vector3D? m_pathfindingDebugTarget;
        private List<MyNavmeshOBBs.OBBCoords> m_debugDrawIntersectedOBBs = new List<MyNavmeshOBBs.OBBCoords>();
        private List<MyFormatPositionColor> m_visualNavmesh = new List<MyFormatPositionColor>();
        private List<MyFormatPositionColor> m_newVisualNavmesh;
        private uint m_drawNavmeshID = uint.MaxValue;

        public void DebugDraw()
        {
            this.DebugDrawInternal();
            int count = this.m_debugDrawPaths.Count;
            int index = 0;
            while (index < count)
            {
                MyRDPath path = this.m_debugDrawPaths[index];
                if (!path.IsValid || path.PathCompleted)
                {
                    this.m_debugDrawPaths.RemoveAt(index);
                    count = this.m_debugDrawPaths.Count;
                    continue;
                }
                path.DebugDraw();
                index++;
            }
        }

        private unsafe void DebugDrawInternal()
        {
            if (this.m_navmeshOBBs != null)
            {
                this.m_navmeshOBBs.DebugDraw();
            }
            if (this.DrawNavmesh)
            {
                this.DrawPersistentDebugNavmesh(false);
            }
            if (this.DrawPhysicalMesh)
            {
                this.DebugDrawPhysicalShapes();
            }
            Vector3D position = MySession.Static.ControlledEntity.ControllerInfo.Controller.Player.GetPosition();
            double* numPtr1 = (double*) ref position.Y;
            numPtr1[0] += 2.4000000953674316;
            MyRenderProxy.DebugDrawText3D(position, $"X: {Math.Round(position.X, 2)}
Y: {Math.Round(position.Y, 2)}
Z: {Math.Round(position.Z, 2)}", Color.Red, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            if (this.m_lastGroundMeshQuery.Count > 0)
            {
                MyRenderProxy.DebugDrawSphere(this.m_lastGroundMeshQuery[0].Center, 1f, Color.Yellow, 1f, true, false, true, false);
                foreach (BoundingBoxD xd2 in this.m_lastGroundMeshQuery)
                {
                    MyRenderProxy.DebugDrawOBB(xd2.Matrix, Color.Yellow, 0f, true, false, true, false);
                }
                if (this.m_navmeshOBBs != null)
                {
                    float num;
                    float num2;
                    foreach (MyNavmeshOBBs.OBBCoords coords in this.m_debugDrawIntersectedOBBs)
                    {
                        MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(coords.OBB.Center, new Vector3(coords.OBB.HalfExtent.X, coords.OBB.HalfExtent.Y / 2.0, coords.OBB.HalfExtent.Z), coords.OBB.Orientation), Color.White, 0f, true, false, false);
                    }
                    MyOrientedBoundingBoxD obb = this.m_navmeshOBBs.GetOBB(0, 0).Value;
                    MyPlanet planet = this.GetPlanet(obb.Center);
                    Vector3* points = (Vector3*) stackalloc byte[(((IntPtr) 4) * sizeof(Vector3))];
                    GetMiddleOBBPoints(obb, ref points);
                    planet.Provider.Shape.GetBounds(points, 4, out num, out num2);
                    if (num.IsValid() && num2.IsValid())
                    {
                        Vector3D vectord2 = obb.Orientation.Up * num2;
                        MyRenderProxy.DebugDrawSphere(obb.Orientation.Up * num, 1f, Color.Blue, 0f, true, false, true, false);
                        MyRenderProxy.DebugDrawSphere(vectord2, 1f, Color.Blue, 0f, true, false, true, false);
                    }
                    DrawTerrainLimits(planet, obb);
                }
                MyRenderProxy.DebugDrawSphere(this.m_meshCenter, 2f, Color.Red, 0f, true, false, true, false);
            }
            if ((this.m_polygons != null) && (this.m_pathfindingDebugTarget != null))
            {
                Vector3D vectord3 = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(this.m_pathfindingDebugTarget.Value));
                MyRenderProxy.DebugDrawSphere(this.m_pathfindingDebugTarget.Value + (1.5 * vectord3), 0.2f, Color.Red, 0f, true, false, true, false);
            }
        }

        public void DebugDrawPhysicalShapes()
        {
            MyCubeGrid targetGrid = MyCubeGrid.GetTargetGrid();
            if (targetGrid != null)
            {
                List<MyCubeGrid> list = new List<MyCubeGrid>();
                foreach (MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node node in MyCubeGridGroups.Static.Logical.GetGroup(targetGrid).Nodes)
                {
                    list.Add(node.NodeData);
                }
                MatrixD.Invert(list[0].WorldMatrix);
                foreach (MyCubeGrid grid2 in list)
                {
                    if (MyPerGameSettings.Game == GameEnum.SE_GAME)
                    {
                        HkGridShape shape1 = new HkGridShape(grid2.GridSize, HkReferencePolicy.None);
                        MyCubeBlockCollector collector1 = new MyCubeBlockCollector();
                        collector1.Collect(grid2, new MyVoxelSegmentation(), MyVoxelSegmentationType.Simple, new Dictionary<Vector3I, HkMassElement>());
                        foreach (HkShape shape in collector1.Shapes)
                        {
                            this.DebugDrawShape("", shape, grid2.WorldMatrix);
                        }
                        continue;
                    }
                    foreach (MySlimBlock block in grid2.GetBlocks())
                    {
                        if (block.FatBlock != null)
                        {
                            if (block.FatBlock is MyCompoundCubeBlock)
                            {
                                foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                                {
                                    HkShape shape = block2.FatBlock.ModelCollision.HavokCollisionShapes[0];
                                    this.DebugDrawShape(block2.BlockDefinition.Id.SubtypeName, shape, block2.FatBlock.PositionComp.WorldMatrix);
                                }
                                continue;
                            }
                            if (block.FatBlock.ModelCollision.HavokCollisionShapes != null)
                            {
                                foreach (HkShape shape3 in block.FatBlock.ModelCollision.HavokCollisionShapes)
                                {
                                    this.DebugDrawShape(block.BlockDefinition.Id.SubtypeName, shape3, block.FatBlock.PositionComp.WorldMatrix);
                                }
                            }
                        }
                    }
                }
            }
        }

        private unsafe void DebugDrawShape(string blockName, HkShape shape, MatrixD worldMatrix)
        {
            float num = 1.05f;
            float num2 = 0.02f;
            if (MyPerGameSettings.Game == GameEnum.SE_GAME)
            {
                num2 = 0.1f;
            }
            switch (shape.ShapeType)
            {
                case HkShapeType.Box:
                    MyRenderProxy.DebugDrawOBB(MatrixD.CreateScale((((HkBoxShape) shape).HalfExtents * 2f) + new Vector3(num2)) * worldMatrix, Color.Red, 0f, true, false, true, false);
                    return;

                case HkShapeType.Capsule:
                case HkShapeType.TriSampledHeightFieldCollection:
                case HkShapeType.TriSampledHeightFieldBvTree:
                    break;

                case HkShapeType.ConvexVertices:
                {
                    GeometryCenterPair pair;
                    HkConvexVerticesShape shape7 = (HkConvexVerticesShape) shape;
                    if (!this.m_cachedGeometry.TryGetValue(blockName, out pair))
                    {
                        Vector3 vector;
                        HkGeometry geometry = new HkGeometry();
                        shape7.GetGeometry(geometry, out vector);
                        GeometryCenterPair pair1 = new GeometryCenterPair();
                        pair1.Geometry = geometry;
                        pair1.Center = vector;
                        pair = pair1;
                        if (!string.IsNullOrEmpty(blockName))
                        {
                            this.m_cachedGeometry.Add(blockName, pair);
                        }
                    }
                    Vector3D vectord = Vector3D.Transform(pair.Center, worldMatrix.GetOrientation());
                    MatrixD xd = worldMatrix;
                    xd = MatrixD.CreateScale((double) num) * xd;
                    MatrixD* xdPtr1 = (MatrixD*) ref xd;
                    xdPtr1.Translation -= vectord * (num - 1f);
                    this.DrawGeometry(pair.Geometry, xd, Color.Olive, false, false);
                    break;
                }
                case HkShapeType.List:
                {
                    HkShapeContainerIterator iterator = ((HkListShape) shape).GetIterator();
                    int num3 = 0;
                    while (iterator.IsValid)
                    {
                        num3++;
                        this.DebugDrawShape(blockName + num3, iterator.CurrentValue, worldMatrix);
                        iterator.Next();
                    }
                    return;
                }
                case HkShapeType.Mopp:
                {
                    HkMoppBvTreeShape shape4 = (HkMoppBvTreeShape) shape;
                    this.DebugDrawShape(blockName, (HkShape) shape4.ShapeCollection, worldMatrix);
                    return;
                }
                case HkShapeType.ConvexTranslate:
                {
                    HkConvexTranslateShape shape6 = (HkConvexTranslateShape) shape;
                    this.DebugDrawShape(blockName, (HkShape) shape6.ChildShape, Matrix.CreateTranslation(shape6.Translation) * worldMatrix);
                    return;
                }
                case HkShapeType.ConvexTransform:
                {
                    HkConvexTransformShape shape5 = (HkConvexTransformShape) shape;
                    this.DebugDrawShape(blockName, (HkShape) shape5.ChildShape, shape5.Transform * worldMatrix);
                    return;
                }
                default:
                    return;
            }
        }

        public void DrawGeometry(HkGeometry geometry, MatrixD worldMatrix, Color color, bool depthRead = false, bool shaded = false)
        {
            MyRenderMessageDebugDrawTriangles triangles = MyRenderProxy.PrepareDebugDrawTriangles();
            for (int i = 0; i < geometry.TriangleCount; i++)
            {
                int num2;
                int num3;
                int num4;
                int num5;
                geometry.GetTriangle(i, out num2, out num3, out num4, out num5);
                triangles.AddIndex(num2);
                triangles.AddIndex(num3);
                triangles.AddIndex(num4);
            }
            for (int j = 0; j < geometry.VertexCount; j++)
            {
                triangles.AddVertex(geometry.GetVertex(j));
            }
        }

        private void DrawPersistentDebugNavmesh(bool force)
        {
            if (this.m_newVisualNavmesh != null)
            {
                this.m_visualNavmesh = this.m_newVisualNavmesh;
                this.m_newVisualNavmesh = null;
                force = true;
            }
            if (force)
            {
                if (this.m_visualNavmesh.Count > 0)
                {
                    if (this.m_drawNavmeshID == uint.MaxValue)
                    {
                        this.m_drawNavmeshID = MyRenderProxy.DebugDrawMesh(this.m_visualNavmesh, MatrixD.Identity, true, true);
                    }
                    else
                    {
                        MyRenderProxy.DebugDrawUpdateMesh(this.m_drawNavmeshID, this.m_visualNavmesh, MatrixD.Identity, true, true);
                    }
                }
                else
                {
                    this.HidePersistentDebugNavmesh();
                }
            }
        }

        private static unsafe bool DrawTerrainLimits(MyPlanet planet, MyOrientedBoundingBoxD obb)
        {
            float num;
            float num2;
            Vector3* points = (Vector3*) stackalloc byte[(((IntPtr) 4) * sizeof(Vector3))];
            GetMiddleOBBPoints(obb, ref points);
            planet.Provider.Shape.GetBounds(points, 4, out num, out num2);
            if (!num.IsValid() || !num2.IsValid())
            {
                return false;
            }
            Vector3D vectord = obb.Orientation.Up * num;
            obb.Center = vectord + (((obb.Orientation.Up * num2) - vectord) * 0.5);
            obb.HalfExtent.Y = (num2 - num) * 0.5f;
            MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(obb.Center, obb.HalfExtent, obb.Orientation), Color.Blue, 0f, true, false, false);
            return true;
        }

        public IMyPath FindPathGlobal(Vector3D begin, IMyDestinationShape end, VRage.Game.Entity.MyEntity relativeEntity) => 
            null;

        private void GenerateDebugDrawPolygonNavmesh(MyPlanet planet, MyOrientedBoundingBoxD obb, List<MyFormatPositionColor> navmesh, int xCoord, int yCoord)
        {
            int num = 10;
            int num2 = 0;
            int num3 = 0x5f;
            int num4 = 10;
            using (List<MyRecastDetourPolygon>.Enumerator enumerator = this.m_polygons.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (Vector3 vector in enumerator.Current.Vertices)
                    {
                        Vector3D vectord = this.LocalNavmeshPositionToWorldPosition(obb, vector, this.m_meshCenter, Vector3D.Zero);
                        MyFormatPositionColor item = new MyFormatPositionColor {
                            Position = (Vector3) vectord,
                            Color = new Color(0, num + num2, 0)
                        };
                        navmesh.Add(item);
                    }
                    num2 = (num2 + num4) % num3;
                }
            }
        }

        private void GenerateTiles(List<MyNavmeshOBBs.OBBCoords> obbList)
        {
            MyPlanet planet = this.GetPlanet(this.m_meshCenter);
            foreach (MyNavmeshOBBs.OBBCoords coords in obbList)
            {
                MyOrientedBoundingBoxD oBB = coords.OBB;
                Vector3D pos = this.WorldPositionToLocalNavmeshPosition(oBB.Center, 0f);
                if (!this.rdWrapper.TileAlreadyGenerated(pos))
                {
                    List<Vector3D> list = new List<Vector3D>();
                    int num = list.Count / 3;
                    float[] numArray = new float[list.Count * 3];
                    int num2 = 0;
                    int index = 0;
                    while (true)
                    {
                        if (num2 >= list.Count)
                        {
                            int[] numArray2 = new int[num * 3];
                            int num4 = 0;
                            while (true)
                            {
                                if (num4 >= (num * 3))
                                {
                                    this.m_polygons.Clear();
                                    if (num > 0)
                                    {
                                        List<MyFormatPositionColor> navmesh = new List<MyFormatPositionColor>();
                                        this.GenerateDebugDrawPolygonNavmesh(planet, oBB, navmesh, coords.Coords.X, coords.Coords.Y);
                                        this.m_newVisualNavmesh = navmesh;
                                        Thread.Sleep(10);
                                    }
                                    break;
                                }
                                numArray2[num4] = num4;
                                num4++;
                            }
                            break;
                        }
                        numArray[index] = (float) list[num2].X;
                        numArray[index] = (float) list[num2].Y;
                        index = ((index + 1) + 1) + 1;
                        numArray[index] = (float) list[num2].Z;
                        num2++;
                    }
                }
            }
            this.m_isNavmeshCreationRunning = false;
        }

        private static unsafe Vector3* GetMiddleOBBPoints(MyOrientedBoundingBoxD obb, ref Vector3* points)
        {
            Vector3 vector = obb.Orientation.Right * ((float) obb.HalfExtent.X);
            Vector3 vector2 = obb.Orientation.Forward * ((float) obb.HalfExtent.Z);
            points = (Vector3*) ((obb.Center - vector) - vector2);
            points[sizeof(Vector3)] = (((Vector3) obb.Center) + vector) - vector2;
            points + (((IntPtr) 2) * sizeof(Vector3)) = (Vector3*) ((obb.Center + vector) + vector2);
            points + (((IntPtr) 3) * sizeof(Vector3)) = (Vector3*) ((obb.Center - vector) + vector2);
            return points;
        }

        public static Vector3D GetOBBCorner(MyOrientedBoundingBoxD obb, OBBCorner corner)
        {
            Vector3D[] corners = new Vector3D[8];
            obb.GetCorners(corners, 0);
            return corners[(int) corner];
        }

        public static List<Vector3D> GetOBBCorners(MyOrientedBoundingBoxD obb, List<OBBCorner> corners)
        {
            Vector3D[] vectordArray = new Vector3D[8];
            obb.GetCorners(vectordArray, 0);
            List<Vector3D> list = new List<Vector3D>();
            foreach (OBBCorner corner in corners)
            {
                list.Add(vectordArray[(int) corner]);
            }
            return list;
        }

        public IMyPathfindingLog GetPathfindingLog() => 
            null;

        public List<Vector3D> GetPathPoints(Vector3D initialPosition, Vector3D targetPosition)
        {
            List<Vector3D> list = new List<Vector3D>();
            if (!this.m_isNavmeshCreationRunning)
            {
                if (!this.m_isNavmeshInitialized)
                {
                    this.InitializeNavmesh(initialPosition);
                }
                Vector3D vectord = this.WorldPositionToLocalNavmeshPosition(initialPosition, 0.5f);
                Vector3D vectord2 = this.WorldPositionToLocalNavmeshPosition(targetPosition, 0.5f);
                List<Vector3> path = this.rdWrapper.GetPath((Vector3) vectord, (Vector3) vectord2);
                if (path.Count == 0)
                {
                    List<MyNavmeshOBBs.OBBCoords> intersectedOBB = this.m_navmeshOBBs.GetIntersectedOBB(new LineD(initialPosition, targetPosition));
                    this.StartNavmeshTileCreation(intersectedOBB);
                    this.m_debugDrawIntersectedOBBs = intersectedOBB;
                }
                else
                {
                    foreach (Vector3 vector in path)
                    {
                        list.Add(this.LocalPositionToWorldPosition(vector));
                    }
                }
            }
            return list;
        }

        private MyPlanet GetPlanet(Vector3D position)
        {
            int num = 100;
            BoundingBoxD box = new BoundingBoxD(position - (num * 0.5f), position + (num * 0.5f));
            return MyGamePruningStructure.GetClosestPlanet(ref box);
        }

        private void HidePersistentDebugNavmesh()
        {
            if (this.m_drawNavmeshID != uint.MaxValue)
            {
                MyRenderProxy.RemoveRenderObject(this.m_drawNavmeshID, MyRenderProxy.ObjectType.DebugDrawMesh, false);
                this.m_drawNavmeshID = uint.MaxValue;
            }
        }

        public void InitializeNavmesh(Vector3D center)
        {
            this.m_isNavmeshInitialized = true;
            float cellSize = 0.2f;
            this.m_singleTileSize = 20;
            this.m_tileLineCount = 50;
            this.m_singleTileHeight = 70;
            MyRecastOptions options1 = new MyRecastOptions();
            options1.cellHeight = 0.2f;
            options1.agentHeight = 1.5f;
            options1.agentRadius = 0.5f;
            options1.agentMaxClimb = 0.5f;
            options1.agentMaxSlope = 50f;
            options1.regionMinSize = 1f;
            options1.regionMergeSize = 10f;
            options1.edgeMaxLen = 50f;
            options1.edgeMaxError = 3f;
            options1.vertsPerPoly = 6f;
            options1.detailSampleDist = 6f;
            options1.detailSampleMaxError = 1f;
            options1.partitionType = 1;
            this.m_recastOptions = options1;
            float num2 = (this.m_singleTileSize * 0.5f) + (this.m_singleTileSize * ((float) Math.Floor((double) (this.m_tileLineCount * 0.5f))));
            float num3 = this.m_singleTileHeight * 0.5f;
            this.m_border = this.m_recastOptions.agentRadius + (3f * cellSize);
            float[] bMin = new float[] { -num2, -num3, -num2 };
            float[] bMax = new float[] { num2, num3, num2 };
            this.rdWrapper = new MyRDWrapper();
            this.rdWrapper.Init(cellSize, (float) this.m_singleTileSize, bMin, bMax);
            Vector3D forwardDirection = Vector3D.CalculatePerpendicularVector(-Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(center)));
            this.UnloadData();
            this.m_navmeshOBBs = new MyNavmeshOBBs(this.GetPlanet(center), center, forwardDirection, this.m_tileLineCount, this.m_singleTileSize, this.m_singleTileHeight);
            this.m_meshCenter = center;
            this.m_visualNavmesh.Clear();
        }

        private Vector3D LocalNavmeshPositionToWorldPosition(MyOrientedBoundingBoxD obb, Vector3D position, Vector3D center, Vector3D heightIncrease)
        {
            MatrixD matrix = this.LocalNavmeshPositionToWorldPositionTransform(obb, center);
            return (Vector3D.Transform(position, matrix) + this.m_meshCenter);
        }

        private MatrixD LocalNavmeshPositionToWorldPositionTransform(MyOrientedBoundingBoxD obb, Vector3D center)
        {
            Vector3D v = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(center));
            return MatrixD.CreateFromQuaternion(Quaternion.CreateFromForwardUp((Vector3) Vector3D.CalculatePerpendicularVector(v), (Vector3) v));
        }

        private Vector3D LocalPositionToWorldPosition(Vector3D position)
        {
            Vector3D center = position;
            if (this.m_navmeshOBBs != null)
            {
                center = this.m_meshCenter;
            }
            return this.LocalNavmeshPositionToWorldPosition(this.m_navmeshOBBs.CenterOBB, position, center, (Vector3D) (0.5 * -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(center))));
        }

        public bool ReachableUnderThreshold(Vector3D begin, IMyDestinationShape end, float thresholdDistance) => 
            true;

        public void SetTarget(Vector3D? target)
        {
            this.m_pathfindingDebugTarget = target;
        }

        public void StartNavmeshTileCreation(List<MyNavmeshOBBs.OBBCoords> obbList)
        {
            if (!this.m_isNavmeshCreationRunning)
            {
                this.m_isNavmeshCreationRunning = true;
                Parallel.Start(() => this.GenerateTiles(obbList));
            }
        }

        private void TestCliPerformance()
        {
            Stopwatch stopwatch = new Stopwatch();
            int num = 0x989680;
            stopwatch.Start();
            for (int i = 0; i < num; i++)
            {
                Test.SimpleTest(i);
            }
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
            for (int j = 0; j < num; j++)
            {
                MyRDWrapper.SimpleTestShallow(j);
            }
            stopwatch.Stop();
            long num5 = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
            for (int k = 0; k < num; k++)
            {
                MyRDWrapper.SimpleTestDeep(k);
            }
            stopwatch.Stop();
            long num6 = stopwatch.ElapsedMilliseconds;
        }

        public void UnloadData()
        {
            this.HidePersistentDebugNavmesh();
            this.m_visualNavmesh.Clear();
            if (this.m_newVisualNavmesh != null)
            {
                this.m_newVisualNavmesh.Clear();
            }
            this.m_newVisualNavmesh = null;
        }

        public void Update()
        {
        }

        private Vector3D WorldPositionToLocalNavmeshPosition(Vector3D position, float heightIncrease)
        {
            MyOrientedBoundingBoxD? oBB = this.m_navmeshOBBs.GetOBB(position);
            if (oBB != null)
            {
                MyOrientedBoundingBoxD local1 = oBB.Value;
            }
            else
            {
                Vector3D meshCenter = this.m_meshCenter;
            }
            Vector3D v = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(this.m_meshCenter));
            MatrixD matrix = MatrixD.CreateFromQuaternion(Quaternion.Inverse(Quaternion.CreateFromForwardUp((Vector3) Vector3D.CalculatePerpendicularVector(v), (Vector3) v)));
            return Vector3D.Transform((position - this.m_meshCenter) + (heightIncrease * v), matrix);
        }

        public bool DrawDebug { get; set; }

        public bool DrawPhysicalMesh { get; set; }

        public bool DrawNavmesh
        {
            get => 
                this.drawMesh;
            set
            {
                this.drawMesh = value;
                if (this.drawMesh)
                {
                    this.DrawPersistentDebugNavmesh(true);
                }
                else
                {
                    this.HidePersistentDebugNavmesh();
                }
            }
        }

        private class GeometryCenterPair
        {
            public HkGeometry Geometry { get; set; }

            public Vector3D Center { get; set; }
        }

        public enum OBBCorner
        {
            UpperFrontLeft,
            UpperBackLeft,
            LowerBackLeft,
            LowerFrontLeft,
            UpperFrontRight,
            UpperBackRight,
            LowerBackRight,
            LowerFrontRight
        }

        private class Test
        {
            public static int SimpleTest(int i) => 
                0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vector3D pos;
            public Color color;
        }
    }
}

