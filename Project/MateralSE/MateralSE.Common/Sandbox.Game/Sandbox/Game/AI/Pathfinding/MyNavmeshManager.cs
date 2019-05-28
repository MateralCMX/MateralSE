namespace Sandbox.Game.AI.Pathfinding
{
    using ParallelTasks;
    using RecastDetour;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyNavmeshManager
    {
        private static MyRandom ran = new MyRandom(0);
        public Color m_debugColor;
        private const float RECAST_CELL_SIZE = 0.2f;
        private const int MAX_TILES_TO_GENERATE = 7;
        private const int MAX_TICKS_WITHOUT_HEARTBEAT = 0x1388;
        private int m_ticksAfterLastPathRequest;
        private int m_tileSize;
        private int m_tileHeight;
        private int m_tileLineCount;
        private float m_border;
        private float m_heightCoordTransformationIncrease;
        private bool m_allTilesGenerated;
        private bool m_isManagerAlive = true;
        private MyNavmeshOBBs m_navmeshOBBs;
        private MyRecastOptions m_recastOptions;
        private MyNavigationInputMesh m_navInputMesh;
        private HashSet<MyNavmeshOBBs.OBBCoords> m_obbCoordsToUpdate = new HashSet<MyNavmeshOBBs.OBBCoords>(new OBBCoordComparer());
        private HashSet<Vector2I> m_coordsAlreadyGenerated = new HashSet<Vector2I>(new CoordComparer());
        private Dictionary<Vector2I, List<MyFormatPositionColor>> m_obbCoordsPolygons = new Dictionary<Vector2I, List<MyFormatPositionColor>>();
        private Dictionary<Vector2I, List<MyFormatPositionColor>> m_newObbCoordsPolygons = new Dictionary<Vector2I, List<MyFormatPositionColor>>();
        private bool m_navmeshTileGenerationRunning;
        private MyRDWrapper m_rdWrapper;
        private MyOrientedBoundingBoxD m_extendedBaseOBB;
        private List<MyVoxelMap> m_tmpTrackedVoxelMaps = new List<MyVoxelMap>();
        private Dictionary<long, MyVoxelMap> m_trackedVoxelMaps = new Dictionary<long, MyVoxelMap>();
        private int?[][] m_debugTileSize;
        private bool m_drawMesh;
        private bool m_updateDrawMesh;
        private List<MyRecastDetourPolygon> m_polygons = new List<MyRecastDetourPolygon>();
        private List<BoundingBoxD> m_groundCaptureAABBs = new List<BoundingBoxD>();
        private uint m_drawNavmeshID = uint.MaxValue;

        public MyNavmeshManager(MyRDPathfinding rdPathfinding, Vector3D center, Vector3D forwardDirection, int tileSize, int tileHeight, int tileLineCount, MyRecastOptions recastOptions)
        {
            Vector3 vector = new Vector3(ran.NextFloat(), ran.NextFloat(), ran.NextFloat());
            vector -= Math.Min(vector.X, Math.Min(vector.Y, vector.Z));
            vector /= Math.Max(vector.X, Math.Max(vector.Y, vector.Z));
            this.m_debugColor = new Color(vector);
            this.m_tileSize = tileSize;
            this.m_tileHeight = tileHeight;
            this.m_tileLineCount = tileLineCount;
            this.Planet = this.GetPlanet(center);
            this.m_heightCoordTransformationIncrease = 0.5f;
            float cellSize = 0.2f;
            this.m_recastOptions = recastOptions;
            float num2 = (this.m_tileSize * 0.5f) + (this.m_tileSize * ((float) Math.Floor((double) (this.m_tileLineCount * 0.5f))));
            float num3 = this.m_tileHeight * 0.5f;
            this.m_border = this.m_recastOptions.agentRadius + (3f * cellSize);
            float[] bMin = new float[] { -num2, -num3, -num2 };
            float[] bMax = new float[] { num2, num3, num2 };
            this.m_rdWrapper = new MyRDWrapper();
            this.m_rdWrapper.Init(cellSize, (float) this.m_tileSize, bMin, bMax);
            Vector3D vectord = Vector3D.CalculatePerpendicularVector(-Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(center)));
            this.m_navmeshOBBs = new MyNavmeshOBBs(this.Planet, center, vectord, this.m_tileLineCount, this.m_tileSize, this.m_tileHeight);
            this.m_debugTileSize = new int?[this.m_tileLineCount][];
            for (int i = 0; i < this.m_tileLineCount; i++)
            {
                this.m_debugTileSize[i] = new int?[this.m_tileLineCount];
            }
            this.m_extendedBaseOBB = new MyOrientedBoundingBoxD(this.m_navmeshOBBs.BaseOBB.Center, new Vector3D(this.m_navmeshOBBs.BaseOBB.HalfExtent.X, (double) this.m_tileHeight, this.m_navmeshOBBs.BaseOBB.HalfExtent.Z), this.m_navmeshOBBs.BaseOBB.Orientation);
            this.m_navInputMesh = new MyNavigationInputMesh(rdPathfinding, this.Planet, center);
        }

        private bool AddTileToGeneration(MyNavmeshOBBs.OBBCoords obbCoord) => 
            (!this.m_coordsAlreadyGenerated.Contains(obbCoord.Coords) && this.m_obbCoordsToUpdate.Add(obbCoord));

        private bool CheckManagerHeartbeat()
        {
            if (!this.m_isManagerAlive)
            {
                return false;
            }
            this.m_ticksAfterLastPathRequest++;
            this.m_isManagerAlive = this.m_ticksAfterLastPathRequest < 0x1388;
            if (!this.m_isManagerAlive)
            {
                this.UnloadData();
            }
            return this.m_isManagerAlive;
        }

        public bool ContainsPosition(Vector3D position)
        {
            LineD line = new LineD(this.Planet.PositionComp.WorldAABB.Center, position);
            return (this.m_navmeshOBBs.BaseOBB.Intersects(ref line) != null);
        }

        public void DebugDraw()
        {
            this.m_navmeshOBBs.DebugDraw();
            this.m_navInputMesh.DebugDraw();
            MyRenderProxy.DebugDrawOBB(this.m_extendedBaseOBB, Color.White, 0f, true, false, false);
            using (List<BoundingBoxD>.Enumerator enumerator = this.m_groundCaptureAABBs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyRenderProxy.DebugDrawAABB(enumerator.Current, Color.Yellow, 1f, 1f, true, false, false);
                }
            }
        }

        private void DrawPersistentDebugNavmesh()
        {
            foreach (KeyValuePair<Vector2I, List<MyFormatPositionColor>> pair in this.m_newObbCoordsPolygons)
            {
                if (this.m_newObbCoordsPolygons[pair.Key] == null)
                {
                    this.m_obbCoordsPolygons.Remove(pair.Key);
                    continue;
                }
                this.m_obbCoordsPolygons[pair.Key] = pair.Value;
            }
            this.m_newObbCoordsPolygons.Clear();
            if (this.m_obbCoordsPolygons.Count > 0)
            {
                List<MyFormatPositionColor> vertices = new List<MyFormatPositionColor>();
                foreach (List<MyFormatPositionColor> list2 in this.m_obbCoordsPolygons.Values)
                {
                    int num = 0;
                    while (num < list2.Count)
                    {
                        vertices.Add(list2[num]);
                    }
                }
                if (this.m_drawNavmeshID == uint.MaxValue)
                {
                    this.m_drawNavmeshID = MyRenderProxy.DebugDrawMesh(vertices, MatrixD.Identity, true, true);
                }
                else
                {
                    MyRenderProxy.DebugDrawUpdateMesh(this.m_drawNavmeshID, vertices, MatrixD.Identity, true, true);
                }
            }
        }

        private void GenerateDebugDrawPolygonNavmesh(MyPlanet planet, List<MyRecastDetourPolygon> polygons, MyOrientedBoundingBoxD centerOBB, Vector2I coords)
        {
            if (polygons != null)
            {
                List<MyFormatPositionColor> list = new List<MyFormatPositionColor>();
                int num = 10;
                int num2 = 0;
                int num3 = 0x5f;
                int num4 = 10;
                using (List<MyRecastDetourPolygon>.Enumerator enumerator = polygons.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (Vector3 vector in enumerator.Current.Vertices)
                        {
                            MyFormatPositionColor item = new MyFormatPositionColor {
                                Position = (Vector3) this.LocalNavmeshPositionToWorldPosition(centerOBB, vector, this.Center, Vector3D.Zero),
                                Color = new Color(0, num + num2, 0)
                            };
                            list.Add(item);
                        }
                        num2 = (num2 + num4) % num3;
                    }
                }
                if (list.Count > 0)
                {
                    this.m_newObbCoordsPolygons[coords] = list;
                }
            }
        }

        private void GenerateDebugTileDataSize(Vector3 center, int xCoord, int yCoord)
        {
            int tileDataSize = this.m_rdWrapper.GetTileDataSize(center, 0);
            this.m_debugTileSize[xCoord][yCoord] = new int?(tileDataSize);
        }

        private void GenerateNextQueuedTile()
        {
            if (!this.m_navmeshTileGenerationRunning && this.TilesAreWaitingGeneration)
            {
                this.m_navmeshTileGenerationRunning = true;
                MyNavmeshOBBs.OBBCoords obb = this.m_obbCoordsToUpdate.First<MyNavmeshOBBs.OBBCoords>();
                this.m_obbCoordsToUpdate.Remove(obb);
                this.m_coordsAlreadyGenerated.Add(obb.Coords);
                Parallel.Start(() => this.GenerateTile(obb));
            }
        }

        private unsafe void GenerateTile(MyNavmeshOBBs.OBBCoords obbCoord)
        {
            MyOrientedBoundingBoxD oBB = obbCoord.OBB;
            Vector3 pos = (Vector3) this.WorldPositionToLocalNavmeshPosition(oBB.Center, 0f);
            List<BoundingBoxD> boundingBoxes = new List<BoundingBoxD>();
            MyNavigationInputMesh.WorldVerticesInfo info = this.m_navInputMesh.GetWorldVertices(this.m_border, this.Center, oBB, boundingBoxes, this.m_tmpTrackedVoxelMaps);
            this.m_groundCaptureAABBs = boundingBoxes;
            foreach (MyVoxelMap map in this.m_tmpTrackedVoxelMaps)
            {
                if (!this.m_trackedVoxelMaps.ContainsKey(map.EntityId))
                {
                    map.RangeChanged += new MyVoxelBase.StorageChanged(this.VoxelMapRangeChanged);
                    this.m_trackedVoxelMaps.Add(map.EntityId, map);
                }
            }
            this.m_tmpTrackedVoxelMaps.Clear();
            if (info.Triangles.Count <= 0)
            {
                this.m_newObbCoordsPolygons[obbCoord.Coords] = null;
            }
            else
            {
                Vector3* vectorPtr;
                Vector3[] pinned vectorArray;
                int* numPtr2;
                int[] pinned numArray;
                if (((vectorArray = info.Vertices.GetInternalArray<Vector3>()) == null) || (vectorArray.Length == 0))
                {
                    vectorPtr = null;
                }
                else
                {
                    vectorPtr = vectorArray;
                }
                float* vertices = (float*) vectorPtr;
                if (((numArray = info.Triangles.GetInternalArray<int>()) == null) || (numArray.Length == 0))
                {
                    numPtr2 = null;
                }
                else
                {
                    numPtr2 = numArray;
                }
                this.m_rdWrapper.CreateNavmeshTile(pos, ref this.m_recastOptions, ref this.m_polygons, obbCoord.Coords.X, obbCoord.Coords.Y, 0, vertices, info.Vertices.Count, numPtr2, info.Triangles.Count / 3);
                numArray = null;
                vectorArray = null;
                this.GenerateDebugDrawPolygonNavmesh(this.Planet, this.m_polygons, this.m_navmeshOBBs.CenterOBB, obbCoord.Coords);
                this.GenerateDebugTileDataSize(pos, obbCoord.Coords.X, obbCoord.Coords.Y);
                if (this.m_polygons != null)
                {
                    this.m_polygons.Clear();
                    this.m_updateDrawMesh = true;
                }
            }
            this.m_navmeshTileGenerationRunning = false;
        }

        private unsafe Vector3D GetBorderPoint(Vector3D startingPoint, Vector3D outsidePoint)
        {
            LineD line = new LineD(startingPoint, outsidePoint);
            double? nullable = this.m_extendedBaseOBB.Intersects(ref line);
            if (nullable == null)
            {
                return outsidePoint;
            }
            line.Length = nullable.Value - 1.0;
            LineD* edPtr1 = (LineD*) ref line;
            edPtr1->To = startingPoint + (line.Direction * nullable.Value);
            return line.To;
        }

        private double GetPathDistance(List<Vector3D> path)
        {
            double num = 0.0;
            for (int i = 0; i < (path.Count - 1); i++)
            {
                num += Vector3D.Distance(path[i], path[i + 1]);
            }
            return num;
        }

        public bool GetPathPoints(Vector3D initialPosition, Vector3D targetPosition, out List<Vector3D> path, out bool noTilesToGenerate)
        {
            this.Heartbeat();
            bool flag = false;
            noTilesToGenerate = true;
            path = new List<Vector3D>();
            if (!this.m_allTilesGenerated)
            {
                int num;
                this.TilesToGenerateInternal(initialPosition, targetPosition, out num);
                noTilesToGenerate = num == 0;
            }
            Vector3D vectord = this.WorldPositionToLocalNavmeshPosition(initialPosition, this.m_heightCoordTransformationIncrease);
            Vector3D position = targetPosition;
            bool flag2 = !this.ContainsPosition(targetPosition);
            if (flag2)
            {
                position = this.GetBorderPoint(initialPosition, targetPosition);
                position = this.GetPositionAtDistanceFromPlanetCenter(position, (initialPosition - this.Planet.PositionComp.WorldAABB.Center).Length());
            }
            Vector3D vectord3 = this.WorldPositionToLocalNavmeshPosition(position, this.m_heightCoordTransformationIncrease);
            List<Vector3> list = this.m_rdWrapper.GetPath((Vector3) vectord, (Vector3) vectord3);
            if (list.Count > 0)
            {
                foreach (Vector3 vector in list)
                {
                    path.Add(this.LocalPositionToWorldPosition(vector));
                }
                Vector3D vectord5 = path.Last<Vector3D>();
                bool flag1 = (position - vectord5).Length() <= 0.25;
                flag = flag1 && !flag2;
                bool local1 = flag1;
                if (local1)
                {
                    if (flag2)
                    {
                        path.RemoveAt(path.Count - 1);
                        path.Add(targetPosition);
                    }
                    else if (noTilesToGenerate && (this.GetPathDistance(path) > (3.0 * Vector3D.Distance(initialPosition, targetPosition))))
                    {
                        noTilesToGenerate = !this.TryGenerateTilesAroundPosition(initialPosition);
                    }
                }
                if ((!local1 && !this.m_allTilesGenerated) & noTilesToGenerate)
                {
                    noTilesToGenerate = !this.TryGenerateTilesAroundPosition(vectord5);
                }
            }
            return flag;
        }

        private MyPlanet GetPlanet(Vector3D position)
        {
            int num = 200;
            BoundingBoxD box = new BoundingBoxD(position - (num * 0.5f), position + (num * 0.5f));
            return MyGamePruningStructure.GetClosestPlanet(ref box);
        }

        private Vector3D GetPositionAtDistanceFromPlanetCenter(Vector3D position, double distance)
        {
            (position - this.Planet.PositionComp.WorldAABB.Center).Length();
            return ((-Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(position)) * distance) + this.Planet.PositionComp.WorldAABB.Center);
        }

        private void Heartbeat()
        {
            this.m_ticksAfterLastPathRequest = 0;
        }

        private void HidePersistentDebugNavmesh()
        {
            if (this.m_drawNavmeshID != uint.MaxValue)
            {
                MyRenderProxy.RemoveRenderObject(this.m_drawNavmeshID, MyRenderProxy.ObjectType.DebugDrawMesh, false);
                this.m_drawNavmeshID = uint.MaxValue;
            }
        }

        private bool Intersects(BoundingBoxD obb) => 
            this.m_extendedBaseOBB.Intersects(ref obb);

        public bool InvalidateArea(BoundingBoxD areaAABB)
        {
            bool flag3;
            bool flag = false;
            if (!this.Intersects(areaAABB))
            {
                return flag;
            }
            bool flag2 = false;
            int coordX = 0;
            goto TR_0013;
        TR_0003:
            if (flag)
            {
                this.m_updateDrawMesh = true;
            }
            return flag;
        TR_0013:
            while (true)
            {
                if (coordX < this.m_tileLineCount)
                {
                    flag3 = false;
                    bool flag4 = false;
                    for (int i = 0; i < this.m_tileLineCount; i++)
                    {
                        MyOrientedBoundingBoxD? oBB = this.m_navmeshOBBs.GetOBB(coordX, i);
                        MyOrientedBoundingBoxD xd = oBB.Value;
                        if (!xd.Intersects(ref areaAABB))
                        {
                            if (flag4)
                            {
                                break;
                            }
                        }
                        else
                        {
                            Vector2I item = new Vector2I(coordX, i);
                            flag3 = flag4 = true;
                            if (this.m_coordsAlreadyGenerated.Remove(item))
                            {
                                flag = true;
                                this.m_allTilesGenerated = false;
                                this.m_newObbCoordsPolygons[item] = null;
                                this.m_navInputMesh.InvalidateCache(areaAABB);
                            }
                        }
                    }
                }
                else
                {
                    goto TR_0003;
                }
                break;
            }
            if (!flag3)
            {
                if (flag2)
                {
                    goto TR_0003;
                }
            }
            else
            {
                flag2 = true;
            }
            coordX++;
            goto TR_0013;
        }

        private Vector3D LocalNavmeshPositionToWorldPosition(MyOrientedBoundingBoxD obb, Vector3D position, Vector3D center, Vector3D heightIncrease)
        {
            MatrixD matrix = this.LocalNavmeshPositionToWorldPositionTransform(obb, center);
            return ((Vector3D.Transform(position, matrix) + this.Center) + heightIncrease);
        }

        private MatrixD LocalNavmeshPositionToWorldPositionTransform(MyOrientedBoundingBoxD obb, Vector3D center)
        {
            Vector3D v = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(center));
            return MatrixD.CreateFromQuaternion(Quaternion.CreateFromForwardUp((Vector3) Vector3D.CalculatePerpendicularVector(v), (Vector3) v));
        }

        private Vector3D LocalPositionToWorldPosition(Vector3D position)
        {
            Vector3D worldPoint = position;
            if (this.m_navmeshOBBs != null)
            {
                worldPoint = this.Center;
            }
            Vector3D vectord2 = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(worldPoint));
            return this.LocalNavmeshPositionToWorldPosition(this.m_navmeshOBBs.CenterOBB, position, worldPoint, (Vector3D) (-this.m_heightCoordTransformationIncrease * vectord2));
        }

        private void TestCliPerformance()
        {
            Stopwatch stopwatch = new Stopwatch();
            int num = 0x989680;
            stopwatch.Start();
            long num2 = 0L;
            for (int i = 0; i < num; i++)
            {
                num2 += Test.SimpleTest(i);
            }
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
            num2 = 0L;
            for (int j = 0; j < num; j++)
            {
                num2 += MyRDWrapper.SimpleTestShallow(j);
            }
            stopwatch.Stop();
            long num6 = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
            num2 = 0L;
            for (int k = 0; k < num; k++)
            {
                num2 += MyRDWrapper.SimpleTestDeep(k);
            }
            stopwatch.Stop();
            long num7 = stopwatch.ElapsedMilliseconds;
        }

        public void TilesToGenerate(Vector3D initialPosition, Vector3D targetPosition)
        {
            int num;
            this.TilesToGenerateInternal(initialPosition, targetPosition, out num);
        }

        private List<MyNavmeshOBBs.OBBCoords> TilesToGenerateInternal(Vector3D initialPosition, Vector3D targetPosition, out int tilesAddedToGeneration)
        {
            tilesAddedToGeneration = 0;
            List<MyNavmeshOBBs.OBBCoords> intersectedOBB = this.m_navmeshOBBs.GetIntersectedOBB(new LineD(initialPosition, targetPosition));
            foreach (MyNavmeshOBBs.OBBCoords coords in intersectedOBB)
            {
                if (this.AddTileToGeneration(coords))
                {
                    tilesAddedToGeneration++;
                    if (tilesAddedToGeneration == 7)
                    {
                        break;
                    }
                }
            }
            return intersectedOBB;
        }

        private bool TryGenerateNeighbourTiles(MyNavmeshOBBs.OBBCoords obbCoord, int radius = 1)
        {
            int num = 0;
            bool flag = false;
            int num2 = -radius;
            while (num2 <= radius)
            {
                int num1;
                if ((num2 == -radius) || (num2 == radius))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = 2 * radius;
                }
                int num3 = num1;
                int num4 = -radius;
                while (true)
                {
                    Vector2I vectori;
                    if (num4 > radius)
                    {
                        num2++;
                        break;
                    }
                    vectori.X = obbCoord.Coords.X + num4;
                    vectori.Y = obbCoord.Coords.Y + num2;
                    MyNavmeshOBBs.OBBCoords? oBBCoord = this.m_navmeshOBBs.GetOBBCoord(vectori.X, vectori.Y);
                    if (oBBCoord != null)
                    {
                        flag = true;
                        if (this.AddTileToGeneration(oBBCoord.Value) && ((num + 1) >= 7))
                        {
                            return true;
                        }
                    }
                    num4 += num3;
                }
            }
            if (num > 0)
            {
                return true;
            }
            this.m_allTilesGenerated = !flag;
            return (!this.m_allTilesGenerated ? this.TryGenerateNeighbourTiles(obbCoord, radius + 1) : false);
        }

        private bool TryGenerateTilesAroundPosition(Vector3D position)
        {
            MyNavmeshOBBs.OBBCoords? oBBCoord = this.m_navmeshOBBs.GetOBBCoord(position);
            return ((oBBCoord != null) && this.TryGenerateNeighbourTiles(oBBCoord.Value, 1));
        }

        public void UnloadData()
        {
            this.m_isManagerAlive = false;
            foreach (KeyValuePair<long, MyVoxelMap> pair in this.m_trackedVoxelMaps)
            {
                pair.Value.RangeChanged -= new MyVoxelBase.StorageChanged(this.VoxelMapRangeChanged);
            }
            this.m_trackedVoxelMaps.Clear();
            this.m_rdWrapper.Clear();
            this.m_rdWrapper = null;
            this.m_navInputMesh.Clear();
            this.m_navInputMesh = null;
            this.m_navmeshOBBs.Clear();
            this.m_navmeshOBBs = null;
            this.m_obbCoordsToUpdate.Clear();
            this.m_obbCoordsToUpdate = null;
            this.m_coordsAlreadyGenerated.Clear();
            this.m_coordsAlreadyGenerated = null;
            this.m_obbCoordsPolygons.Clear();
            this.m_obbCoordsPolygons = null;
            this.m_newObbCoordsPolygons.Clear();
            this.m_newObbCoordsPolygons = null;
            this.m_polygons.Clear();
            this.m_polygons = null;
        }

        public bool Update()
        {
            if (!this.CheckManagerHeartbeat())
            {
                return false;
            }
            this.GenerateNextQueuedTile();
            if (this.m_updateDrawMesh)
            {
                this.m_updateDrawMesh = false;
                this.UpdatePersistentDebugNavmesh();
            }
            return true;
        }

        private void UpdatePersistentDebugNavmesh()
        {
            this.DrawNavmesh = this.DrawNavmesh;
        }

        private void VoxelMapRangeChanged(MyVoxelBase storage, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
        {
            BoundingBoxD areaAABB = MyRDPathfinding.GetVoxelAreaAABB(storage, minVoxelChanged, maxVoxelChanged);
            this.InvalidateArea(areaAABB);
        }

        private Vector3D WorldPositionToLocalNavmeshPosition(Vector3D position, float heightIncrease)
        {
            Vector3D v = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(this.Center));
            MatrixD matrix = MatrixD.CreateFromQuaternion(Quaternion.Inverse(Quaternion.CreateFromForwardUp((Vector3) Vector3D.CalculatePerpendicularVector(v), (Vector3) v)));
            return Vector3D.Transform((position - this.Center) + (heightIncrease * v), matrix);
        }

        public Vector3D Center =>
            this.m_navmeshOBBs.CenterOBB.Center;

        public MyOrientedBoundingBoxD CenterOBB =>
            this.m_navmeshOBBs.CenterOBB;

        public MyPlanet Planet { get; private set; }

        public bool TilesAreWaitingGeneration =>
            (this.m_obbCoordsToUpdate.Count > 0);

        public bool DrawNavmesh
        {
            get => 
                this.m_drawMesh;
            set
            {
                this.m_drawMesh = value;
                if (this.m_drawMesh)
                {
                    this.DrawPersistentDebugNavmesh();
                }
                else
                {
                    this.HidePersistentDebugNavmesh();
                }
            }
        }

        public class CoordComparer : IEqualityComparer<Vector2I>
        {
            public bool Equals(Vector2I a, Vector2I b) => 
                ((a.X == b.X) && (a.Y == b.Y));

            public int GetHashCode(Vector2I point) => 
                (point.X.ToString() + point.Y.ToString()).GetHashCode();
        }

        public class OBBCoordComparer : IEqualityComparer<MyNavmeshOBBs.OBBCoords>
        {
            public bool Equals(MyNavmeshOBBs.OBBCoords a, MyNavmeshOBBs.OBBCoords b) => 
                ((a.Coords.X == b.Coords.X) && (a.Coords.Y == b.Coords.Y));

            public int GetHashCode(MyNavmeshOBBs.OBBCoords point) => 
                (point.Coords.X.ToString() + point.Coords.Y.ToString()).GetHashCode();
        }

        private class Test
        {
            public static int SimpleTest(int i)
            {
                i++;
                return i;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vector3D pos;
            public Color color;
        }
    }
}

