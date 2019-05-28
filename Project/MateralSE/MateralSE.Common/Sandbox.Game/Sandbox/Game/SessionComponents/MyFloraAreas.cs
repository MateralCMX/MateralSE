namespace Sandbox.Game.SessionComponents
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.EnvironmentItems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyFloraAreas : MySessionComponentBase
    {
        private static MyFloraAreas Static;
        private static readonly double DEBUG_BOX_Y_MAX_POS = 500.25;
        private static readonly double DEBUG_BOX_Y_MIN_POS = 500.0;
        private static readonly Vector3D DEFAULT_INFLATE_VALUE = new Vector3D(20.0, 0.0, 20.0);
        private double BOX_INCLUDE_DIST = 5.0;
        private double BOX_INCLUDE_DIST_SQ;
        private BoundingBoxD DEFAULT_BOX;
        private bool m_loadPhase;
        private bool m_findValidForestPhase;
        private int m_updateCounter;
        private MyVoxelBase m_ground;
        private MyStorageData m_voxelCache;
        private HashSet<MyStringHash> m_allowedMaterials;
        private double m_worldArea;
        private double m_currentForestArea;
        private double m_forestsPercent;
        private Dictionary<long, MyEnvironmentItems> m_envItems;
        private List<Area> m_forestAreas;
        private List<BoundingBoxD> m_highLevelBoxes;
        private Queue<Vector3D> m_initialForestLocations;
        private int m_hlCurrentBox;
        private int m_hlSelectionCounter;
        private double m_hlSize;
        private HashSet<Vector3I> m_checkedSectors;
        private Queue<long> m_checkQueue;
        private MyDynamicAABBTreeD m_aabbTree;
        private const int INVALIDATE_TIME = 3;
        private MyTimeSpan m_invalidateAreaTimer;
        private bool m_immediateInvalidate;
        private List<MyEnvironmentItems.ItemInfo> m_tmpItemInfos;
        private List<Area> m_tmpAreas;
        private List<Area> m_tmpAreas2;
        private List<Vector3I> m_tmpSectors;
        private List<Vector3D> d_foundEnrichingPoints = new List<Vector3D>();
        private List<Vector3D> d_foundEnlargingPoints = new List<Vector3D>();
        [CompilerGenerated]
        private static Action LoadFinished;
        [CompilerGenerated]
        private static Action<int> ItemAddedToArea;
        [CompilerGenerated]
        private static Action<int> SelectedArea;
        [CompilerGenerated]
        private static Action<int> RemovedArea;

        public static  event Action<int> ItemAddedToArea
        {
            [CompilerGenerated] add
            {
                Action<int> itemAddedToArea = ItemAddedToArea;
                while (true)
                {
                    Action<int> a = itemAddedToArea;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    itemAddedToArea = Interlocked.CompareExchange<Action<int>>(ref ItemAddedToArea, action3, a);
                    if (ReferenceEquals(itemAddedToArea, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> itemAddedToArea = ItemAddedToArea;
                while (true)
                {
                    Action<int> source = itemAddedToArea;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    itemAddedToArea = Interlocked.CompareExchange<Action<int>>(ref ItemAddedToArea, action3, source);
                    if (ReferenceEquals(itemAddedToArea, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action LoadFinished
        {
            [CompilerGenerated] add
            {
                Action loadFinished = LoadFinished;
                while (true)
                {
                    Action a = loadFinished;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    loadFinished = Interlocked.CompareExchange<Action>(ref LoadFinished, action3, a);
                    if (ReferenceEquals(loadFinished, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action loadFinished = LoadFinished;
                while (true)
                {
                    Action source = loadFinished;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    loadFinished = Interlocked.CompareExchange<Action>(ref LoadFinished, action3, source);
                    if (ReferenceEquals(loadFinished, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<int> RemovedArea
        {
            [CompilerGenerated] add
            {
                Action<int> removedArea = RemovedArea;
                while (true)
                {
                    Action<int> a = removedArea;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    removedArea = Interlocked.CompareExchange<Action<int>>(ref RemovedArea, action3, a);
                    if (ReferenceEquals(removedArea, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> removedArea = RemovedArea;
                while (true)
                {
                    Action<int> source = removedArea;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    removedArea = Interlocked.CompareExchange<Action<int>>(ref RemovedArea, action3, source);
                    if (ReferenceEquals(removedArea, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<int> SelectedArea
        {
            [CompilerGenerated] add
            {
                Action<int> selectedArea = SelectedArea;
                while (true)
                {
                    Action<int> a = selectedArea;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    selectedArea = Interlocked.CompareExchange<Action<int>>(ref SelectedArea, action3, a);
                    if (ReferenceEquals(selectedArea, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> selectedArea = SelectedArea;
                while (true)
                {
                    Action<int> source = selectedArea;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    selectedArea = Interlocked.CompareExchange<Action<int>>(ref SelectedArea, action3, source);
                    if (ReferenceEquals(selectedArea, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyFloraAreas()
        {
            this.BOX_INCLUDE_DIST_SQ = this.BOX_INCLUDE_DIST * this.BOX_INCLUDE_DIST;
            Vector3D max = new Vector3D(0.25, 0.25, 0.25);
            this.DEFAULT_BOX = new BoundingBoxD(-max, max);
        }

        public void AddEnvironmentItem(MyEnvironmentItems item)
        {
            item.OnMarkForClose += new Action<MyEntity>(this.item_OnMarkForClose);
            this.m_envItems.Add(item.EntityId, item);
            this.m_checkQueue.Enqueue(item.EntityId);
            item.ItemRemoved += new Action<MyEnvironmentItems, MyEnvironmentItems.ItemInfo>(this.item_ItemRemoved);
            item.ItemAdded += new Action<MyEnvironmentItems, MyEnvironmentItems.ItemInfo>(this.item_ItemAdded);
            this.m_loadPhase = true;
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
        }

        private double CalculateEntireArea()
        {
            this.m_highLevelBoxes.Clear();
            this.CreateHighLevelBoxes(this.m_highLevelBoxes);
            double num = 0.0;
            foreach (BoundingBoxD xd in this.m_highLevelBoxes)
            {
                Vector3D size = xd.Size;
                num += size.X * size.Z;
            }
            return num;
        }

        private void ClearInvalidAreas(List<Area> areas)
        {
            int idx = 0;
            while (idx < areas.Count)
            {
                if (!areas[idx].IsValid)
                {
                    this.RemoveArea(areas, idx);
                    continue;
                }
                idx++;
            }
        }

        private void ConstructAreas()
        {
            bool flag = false;
            Vector3D sectorPosition = new Vector3D();
            while (!flag && (this.m_checkQueue.Count > 0))
            {
                MyEnvironmentItems items = this.m_envItems[this.m_checkQueue.Peek()];
                foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in items.Sectors)
                {
                    if (!this.m_checkedSectors.Contains(pair.Key))
                    {
                        this.m_checkedSectors.Add(pair.Key);
                        sectorPosition = (pair.Key * items.Definition.SectorSize) + (items.Definition.SectorSize * 0.5f);
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    foreach (MyEnvironmentItems items2 in this.m_envItems.Values)
                    {
                        this.DistributeItems(items2, ref sectorPosition);
                    }
                    continue;
                }
                this.m_checkQueue.Dequeue();
            }
            if (flag)
            {
                BoundingBoxD bbox = BoundingBoxD.CreateInvalid();
                using (Dictionary<long, MyEnvironmentItems>.ValueCollection.Enumerator enumerator2 = this.m_envItems.Values.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        MyEnvironmentSector sector = enumerator2.Current.GetSector(ref sectorPosition);
                        if ((sector != null) && sector.IsValid)
                        {
                            bbox.Include(sector.SectorWorldBox);
                        }
                    }
                }
                bbox.Min.Y = DEBUG_BOX_Y_MIN_POS;
                bbox.Max.Y = DEBUG_BOX_Y_MAX_POS;
                bbox = bbox.Inflate(new Vector3D(100.0, 0.0, 100.0));
                this.m_aabbTree.OverlapAllBoundingBox<Area>(ref bbox, this.m_tmpAreas, 0, true);
                this.MergeAreas(this.m_tmpAreas, 1);
                this.m_tmpAreas.Clear();
                this.ClearInvalidAreas(this.m_forestAreas);
            }
        }

        private void CreateHighLevelBoxes(List<BoundingBoxD> boxes)
        {
            foreach (Vector3I vectori in this.m_checkedSectors)
            {
                BoundingBoxD item = BoundingBoxD.CreateInvalid();
                bool flag = false;
                foreach (KeyValuePair<long, MyEnvironmentItems> pair in this.m_envItems)
                {
                    MyEnvironmentSector sector = pair.Value.GetSector(ref vectori);
                    if ((sector != null) && sector.IsValid)
                    {
                        flag = true;
                        item.Include(sector.SectorWorldBox);
                    }
                }
                if (!flag)
                {
                    this.m_tmpSectors.Add(vectori);
                }
                else
                {
                    item.Max.Y = DEBUG_BOX_Y_MAX_POS;
                    item.Min.Y = DEBUG_BOX_Y_MIN_POS;
                    List<BoundingBoxD> list1 = new List<BoundingBoxD>();
                    list1.Add(item);
                    List<BoundingBoxD> itemsToAdd = list1;
                    double num = 0.0;
                    foreach (BoundingBoxD xd2 in itemsToAdd)
                    {
                        num += xd2.Volume;
                    }
                    num = Math.Min((double) (num / ((double) itemsToAdd.Count)), (double) 10.0);
                    int index = 0;
                    while (true)
                    {
                        if (index >= boxes.Count)
                        {
                            int num3 = 0;
                            while (true)
                            {
                                if (num3 >= itemsToAdd.Count)
                                {
                                    boxes.AddList<BoundingBoxD>(itemsToAdd);
                                    break;
                                }
                                BoundingBoxD xd6 = itemsToAdd[num3];
                                if (xd6.Volume < num)
                                {
                                    itemsToAdd.RemoveAtFast<BoundingBoxD>(num3);
                                    continue;
                                }
                                num3++;
                            }
                            break;
                        }
                        BoundingBoxD box = boxes[index];
                        bool flag2 = false;
                        int num4 = 0;
                        while (true)
                        {
                            if (num4 < itemsToAdd.Count)
                            {
                                BoundingBoxD xd4 = itemsToAdd[num4];
                                BoundingBoxD xd5 = xd4.Inflate(new Vector3D(-0.01, 0.0, -0.01));
                                if (!box.Intersects(ref xd5))
                                {
                                    num4++;
                                    continue;
                                }
                                if (box.Contains(xd4) == ContainmentType.Contains)
                                {
                                    itemsToAdd.RemoveAtFast<BoundingBoxD>(num4);
                                    continue;
                                }
                                if (xd4.Contains(box) != ContainmentType.Contains)
                                {
                                    itemsToAdd.RemoveAtFast<BoundingBoxD>(num4);
                                    this.SplitArea(box, xd4, itemsToAdd);
                                    continue;
                                }
                                flag2 = true;
                            }
                            if (flag2)
                            {
                                boxes.RemoveAtFast<BoundingBoxD>(index);
                            }
                            else
                            {
                                index++;
                            }
                            break;
                        }
                    }
                }
            }
            foreach (Vector3I vectori2 in this.m_tmpSectors)
            {
                this.m_checkedSectors.Remove(vectori2);
            }
            this.m_tmpSectors.Clear();
            if (boxes.Count > 0)
            {
                boxes.Sort((x, y) => y.Volume.CompareTo(x.Volume));
                this.m_hlCurrentBox = 0;
                this.m_hlSize = boxes.Average<BoundingBoxD>(x => x.Volume);
                this.m_hlSelectionCounter = (int) (boxes.First<BoundingBoxD>().Volume / this.m_hlSize);
            }
        }

        public void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_FLORA_BOXES)
            {
                MatrixD worldMatrix = MatrixD.CreateTranslation(Vector3.Down * 256f);
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_forestAreas.Count)
                    {
                        int num4 = 0;
                        while (true)
                        {
                            if (num4 >= this.m_highLevelBoxes.Count)
                            {
                                double seconds = (this.m_invalidateAreaTimer - MySandboxGame.Static.TotalTime).Seconds;
                                MyRenderProxy.DebugDrawText2D(new Vector2(10f, 280f), "Total boxes count: " + this.m_forestAreas.Count, Color.Violet, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                MyRenderProxy.DebugDrawText2D(new Vector2(10f, 300f), "Taken area size: " + ((int) this.m_currentForestArea), Color.Violet, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                MyRenderProxy.DebugDrawText2D(new Vector2(10f, 320f), "World area size: " + this.m_worldArea, Color.Violet, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                MyRenderProxy.DebugDrawText2D(new Vector2(10f, 340f), "Taken area percent: " + (this.m_currentForestArea / this.m_worldArea), Color.Violet, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                MyRenderProxy.DebugDrawText2D(new Vector2(10f, 360f), "Invalidate area timer: " + seconds.ToString(), Color.SlateBlue, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                if (this.m_ground != null)
                                {
                                    object[] objArray1 = new object[] { "World dimensions: ", this.m_ground.SizeInMetres.X, " x ", this.m_ground.SizeInMetres.Z };
                                    MyRenderProxy.DebugDrawText2D(new Vector2(10f, 380f), string.Concat(objArray1), Color.Violet, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                }
                                break;
                            }
                            BoundingBoxD aabb = this.m_highLevelBoxes[num4];
                            aabb.Translate(worldMatrix);
                            MyRenderProxy.DebugDrawAABB(aabb, Color.Red, 1f, 1f, false, false, false);
                            Vector3D vectord = MySector.MainCamera.Position - aabb.Center;
                            if (vectord.Length() <= 30.0)
                            {
                                MyRenderProxy.DebugDrawText3D(aabb.Center, "Gran: " + ((int) (0.5 + aabb.Volume)), Color.CadetBlue, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                                Color coral = Color.Coral;
                                MyStringId? faceMaterial = null;
                                faceMaterial = null;
                                MySimpleObjectDraw.DrawTransparentBox(ref MatrixD.Identity, ref aabb, ref coral, MySimpleObjectRasterizer.Solid, 1, 1f, faceMaterial, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                            }
                            num4++;
                        }
                        break;
                    }
                    Area area = this.m_forestAreas[num2];
                    BoundingBoxD forestBox = area.ForestBox;
                    forestBox.Translate(worldMatrix);
                    MyRenderProxy.DebugDrawAABB(forestBox, area.IsFull ? Color.DarkOrange : Color.Honeydew, 1f, 1f, false, false, false);
                    if ((MySector.MainCamera.Position - forestBox.Center).Length() <= 5.0)
                    {
                        string text = $"Gran {num2}: {(int) (0.5 + forestBox.Volume)}";
                        MyRenderProxy.DebugDrawText3D(forestBox.Center, text, Color.PaleVioletRed, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                        foreach (KeyValuePair<long, HashSet<int>> pair in area.ItemIds)
                        {
                            MyEnvironmentItems items = this.m_envItems[pair.Key];
                            foreach (int num3 in pair.Value)
                            {
                                MyEnvironmentItems.ItemInfo info;
                                if (items.TryGetItemInfoById(num3, out info))
                                {
                                    Vector3D position = Vector3D.Transform(info.Transform.TransformMatrix.Translation, worldMatrix);
                                    MyRenderProxy.DebugDrawPoint(position, Color.Red, true, false);
                                    MyRenderProxy.DebugDrawText3D(position, "Item: " + info.SubtypeId.ToString(), Color.PaleVioletRed, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                                }
                            }
                        }
                    }
                    num2++;
                }
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_FLORA_SPAWNED_ITEMS)
            {
                List<Vector3D>.Enumerator enumerator3;
                using (enumerator3 = this.d_foundEnrichingPoints.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        MyRenderProxy.DebugDrawPoint(enumerator3.Current, Color.MidnightBlue, false, false);
                    }
                }
                using (enumerator3 = this.d_foundEnlargingPoints.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        MyRenderProxy.DebugDrawPoint(enumerator3.Current, Color.Moccasin, false, false);
                    }
                }
            }
        }

        private void DistributeItems(MyEnvironmentItems envItem, ref Vector3D sectorPosition)
        {
            envItem.GetItemsInSector(ref sectorPosition, this.m_tmpItemInfos);
            long entityId = envItem.EntityId;
            foreach (MyEnvironmentItems.ItemInfo info in this.m_tmpItemInfos)
            {
                bool flag = false;
                MyTransformD transform = info.Transform;
                BoundingBoxD worldBox = this.GetWorldBox(info.SubtypeId, transform.TransformMatrix);
                BoundingBoxD inflated = worldBox.GetInflated(DEFAULT_INFLATE_VALUE);
                this.m_aabbTree.OverlapAllBoundingBox<Area>(ref inflated, this.m_tmpAreas, 0, true);
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_tmpAreas.Count)
                    {
                        this.m_tmpAreas.Clear();
                        if (!flag)
                        {
                            Area userData = new Area {
                                ForestBox = worldBox
                            };
                            userData.AddItem(entityId, info.LocalId);
                            userData.ProxyId = this.m_aabbTree.AddProxy(ref worldBox, userData, 0, true);
                            this.m_forestAreas.Add(userData);
                        }
                        break;
                    }
                    BoundingBoxD forestBox = this.m_tmpAreas[num2].ForestBox;
                    Vector3D vectord = forestBox.Center - worldBox.Center;
                    if (vectord.LengthSquared() <= this.BOX_INCLUDE_DIST_SQ)
                    {
                        forestBox.Include(ref worldBox);
                        this.m_tmpAreas[num2].ForestBox = forestBox;
                        this.m_tmpAreas[num2].AddItem(entityId, info.LocalId);
                        flag = true;
                        this.m_aabbTree.MoveProxy(this.m_tmpAreas[num2].ProxyId, ref worldBox, Vector3D.Zero);
                    }
                    num2++;
                }
            }
            this.m_tmpItemInfos.Clear();
        }

        private void FindForestInitialCandidate()
        {
            BoundingBoxD worldAABB = this.m_ground.PositionComp.WorldAABB;
            worldAABB.Inflate(-(worldAABB.Size * 0.10000000149011612));
            MyBBSetSampler sampler = new MyBBSetSampler(worldAABB.Min, worldAABB.Max);
            bool flag = true;
            Vector3D vectord2 = new Vector3D();
            int num = 0;
            while (true)
            {
                vectord2 = sampler.Sample();
                Vector3D point = vectord2;
                point.Y = 0.5;
                flag = true;
                num++;
                Vector3D vectord6 = new Vector3D(20.0, 20.0, 20.0);
                foreach (Vector3D vectord7 in this.m_initialForestLocations)
                {
                    BoundingBoxD xd2 = new BoundingBoxD(vectord7 - vectord6, vectord7 + vectord6) {
                        Min = { Y = 0.0 },
                        Max = { Y = 1.0 }
                    };
                    if (xd2.Contains(point) == ContainmentType.Contains)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag || (num == 10))
                {
                    if (flag)
                    {
                        Vector3D from = new Vector3D(vectord2.X, worldAABB.Max.Y, vectord2.Z);
                        Vector3D to = new Vector3D(vectord2.X, worldAABB.Min.Y, vectord2.Z);
                        LineD line = new LineD(from, to);
                        MyIntersectionResultLineTriangleEx? t = null;
                        byte index = MyDefinitionManager.Static.GetVoxelMaterialDefinition("Grass").Index;
                        if (this.m_ground.GetIntersectionWithLine(ref line, out t, IntersectionFlags.DIRECT_TRIANGLES))
                        {
                            Vector3I vectori;
                            Vector3D intersectionPointInWorldSpace = t.Value.IntersectionPointInWorldSpace;
                            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.m_ground.PositionLeftBottomCorner, ref intersectionPointInWorldSpace, out vectori);
                            Vector3I lodVoxelRangeMin = vectori - Vector3I.One;
                            this.m_ground.Storage.ReadRange(this.m_voxelCache, MyStorageDataTypeFlags.Material, 0, lodVoxelRangeMin, (Vector3I) (vectori + Vector3I.One));
                            Vector3I zero = Vector3I.Zero;
                            Vector3I end = Vector3I.One * 2;
                            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref zero, ref end);
                            while (iterator.IsValid())
                            {
                                Vector3I current = iterator.Current;
                                if (this.m_voxelCache.Material(ref current) == index)
                                {
                                    Vector3I voxelCoord = (Vector3I) ((vectori - Vector3I.One) + current);
                                    Vector3D worldPosition = new Vector3D();
                                    MyVoxelCoordSystems.VoxelCoordToWorldPosition(this.m_ground.PositionLeftBottomCorner, ref voxelCoord, out worldPosition);
                                    this.m_initialForestLocations.Enqueue(worldPosition);
                                    return;
                                }
                                iterator.MoveNext();
                            }
                        }
                    }
                    return;
                }
            }
        }

        private BoundingBoxD GetWorldBox(MyStringHash id, MatrixD worldMatrix)
        {
            BoundingBoxD xd = MyModels.GetModelOnlyData(MyModel.GetById(MyEnvironmentItems.GetModelId(id))).BoundingBox.Transform(ref worldMatrix);
            xd.Inflate((double) 0.6);
            xd.Max.Y = DEBUG_BOX_Y_MAX_POS;
            xd.Min.Y = DEBUG_BOX_Y_MIN_POS;
            return xd;
        }

        public static bool HasBelongingItems(int areaId, MyEnvironmentItems items) => 
            Static.HasBelongingItemsInternal(areaId, items);

        private bool HasBelongingItemsInternal(int areaId, MyEnvironmentItems items)
        {
            Area userData = this.m_aabbTree.GetUserData<Area>(areaId);
            return (userData.ItemIds.ContainsKey(items.EntityId) ? (userData.ItemIds[items.EntityId].Count > 0) : false);
        }

        public static void ImmediateInvalidateAreaValues()
        {
            Static.InvalidateAreaValues();
        }

        private void InvalidateArea(Area area)
        {
            BoundingBoxD aabb = BoundingBoxD.CreateInvalid();
            foreach (KeyValuePair<long, HashSet<int>> pair in area.ItemIds)
            {
                foreach (int num in pair.Value)
                {
                    MyEnvironmentItems.ItemInfo info;
                    if (this.m_envItems[pair.Key].TryGetItemInfoById(num, out info))
                    {
                        aabb.Include(this.GetWorldBox(info.SubtypeId, info.Transform.TransformMatrix));
                    }
                }
            }
            area.ForestBox = aabb;
            if (area.ProxyId != -1)
            {
                this.m_aabbTree.MoveProxy(area.ProxyId, ref aabb, Vector3D.Zero);
            }
        }

        private void InvalidateAreaValues()
        {
            this.ClearInvalidAreas(this.m_forestAreas);
            this.m_currentForestArea = this.CalculateEntireArea();
            this.m_forestsPercent = this.m_currentForestArea / this.m_worldArea;
            this.m_invalidateAreaTimer = (this.m_currentForestArea != 0.0) ? (MySandboxGame.Static.TotalTime + MyTimeSpan.FromSeconds(180.0)) : MyTimeSpan.Zero;
            this.m_immediateInvalidate = false;
        }

        private void item_ItemAdded(MyEnvironmentItems envItems, MyEnvironmentItems.ItemInfo itemInfo)
        {
            this.m_checkedSectors.Add(envItems.GetSectorId(ref itemInfo.Transform.Position));
            BoundingBoxD worldBox = this.GetWorldBox(itemInfo.SubtypeId, itemInfo.Transform.TransformMatrix);
            BoundingBoxD inflated = worldBox.GetInflated(DEFAULT_INFLATE_VALUE);
            this.m_aabbTree.OverlapAllBoundingBox<Area>(ref inflated, this.m_tmpAreas, 0, true);
            Area userData = new Area {
                ForestBox = worldBox
            };
            userData.AddItem(envItems.EntityId, itemInfo.LocalId);
            userData.ProxyId = this.m_aabbTree.AddProxy(ref worldBox, userData, 0, true);
            this.m_tmpAreas.Add(userData);
            this.MergeAreas(this.m_tmpAreas, 1);
            if (userData.IsValid)
            {
                this.m_forestAreas.Add(userData);
            }
            this.m_tmpAreas.Clear();
        }

        private void item_ItemRemoved(MyEnvironmentItems item, MyEnvironmentItems.ItemInfo itemInfo)
        {
            long entityId = item.EntityId;
            int idx = 0;
            while (idx < this.m_forestAreas.Count)
            {
                Area area = this.m_forestAreas[idx];
                if ((!area.IsValid || !area.ItemIds.ContainsKey(entityId)) || !area.ItemIds[entityId].Contains(itemInfo.LocalId))
                {
                    idx++;
                    continue;
                }
                area.ItemIds[entityId].Remove(itemInfo.LocalId);
                if (!area.IsValid)
                {
                    this.RemoveArea(this.m_forestAreas, idx);
                    continue;
                }
                this.InvalidateArea(area);
                idx++;
            }
        }

        private void item_OnMarkForClose(MyEntity obj)
        {
            MyEnvironmentItems item = obj as MyEnvironmentItems;
            this.RemoveEnvironmentItem(item);
        }

        public override void LoadData()
        {
            base.LoadData();
            this.m_updateCounter = 0;
            this.m_envItems = new Dictionary<long, MyEnvironmentItems>(10);
            this.m_forestAreas = new List<Area>(100);
            this.m_highLevelBoxes = new List<BoundingBoxD>();
            this.m_tmpItemInfos = new List<MyEnvironmentItems.ItemInfo>(500);
            this.m_tmpAreas = new List<Area>();
            this.m_tmpAreas2 = new List<Area>();
            this.m_checkedSectors = new HashSet<Vector3I>();
            this.m_checkQueue = new Queue<long>();
            this.m_initialForestLocations = new Queue<Vector3D>();
            this.m_tmpSectors = new List<Vector3I>();
            this.m_aabbTree = new MyDynamicAABBTreeD(Vector3D.Zero, 1.0);
            this.m_allowedMaterials = new HashSet<MyStringHash>();
            this.m_loadPhase = true;
            this.m_findValidForestPhase = false;
            MyEntities.OnEntityAdd += new Action<MyEntity>(this.MyEntities_OnEntityAdd);
            MyEntities.OnEntityRemove += new Action<MyEntity>(this.MyEntities_OnEntityRemove);
            Static = this;
        }

        private void MergeAreas(List<Area> areas, int multiplier = 1)
        {
            int num = 5 * multiplier;
            double num2 = 2.5 * multiplier;
            double num3 = 0.3 * multiplier;
            int idx = 0;
            while (true)
            {
                bool flag;
                while (true)
                {
                    if (idx >= areas.Count)
                    {
                        return;
                    }
                    Area objA = areas[idx];
                    if (objA.IsValid)
                    {
                        BoundingBoxD inflated = objA.ForestBox.GetInflated(DEFAULT_INFLATE_VALUE);
                        this.m_aabbTree.OverlapAllBoundingBox<Area>(ref inflated, this.m_tmpAreas2, 0, true);
                        int num5 = 0;
                        flag = false;
                        while (num5 < this.m_tmpAreas2.Count)
                        {
                            Area objB = this.m_tmpAreas2[num5];
                            if (ReferenceEquals(objA, objB))
                            {
                                num5++;
                                continue;
                            }
                            if (!objB.IsValid)
                            {
                                this.RemoveArea(this.m_tmpAreas2, num5);
                                continue;
                            }
                            BoundingBoxD forestBox = objA.ForestBox;
                            BoundingBoxD box = objB.ForestBox;
                            BoundingBoxD mergedBox = forestBox;
                            mergedBox.Include(box);
                            double volume = forestBox.Volume;
                            double num7 = box.Volume;
                            double num8 = mergedBox.Volume;
                            float num9 = 0f;
                            if (volume > num7)
                            {
                                if (forestBox.Contains(box) == ContainmentType.Contains)
                                {
                                    this.RemoveArea(this.m_tmpAreas2, num5);
                                    continue;
                                }
                                num9 = (float) (1.0 + (num3 * (num7 / volume)));
                            }
                            else
                            {
                                if (box.Contains(forestBox) == ContainmentType.Contains)
                                {
                                    flag = true;
                                    break;
                                }
                                num9 = (float) (1.0 + (num3 * (volume / num7)));
                            }
                            if (num8 < num)
                            {
                                num9 = (float) Math.Min(num2, num9 * (((double) num) / num8));
                            }
                            if (((volume + num7) * num9) <= num8)
                            {
                                num5++;
                            }
                            else
                            {
                                objA.Merge(mergedBox, objB);
                                this.RemoveArea(this.m_tmpAreas2, num5);
                                this.m_aabbTree.MoveProxy(objA.ProxyId, ref mergedBox, Vector3D.Zero);
                            }
                        }
                        break;
                    }
                    this.RemoveArea(areas, idx);
                }
                if (flag)
                {
                    this.RemoveArea(areas, idx);
                }
                else
                {
                    idx++;
                }
                this.m_tmpAreas2.Clear();
            }
        }

        private void MyEntities_OnEntityAdd(MyEntity obj)
        {
            if (obj is MyEnvironmentItems)
            {
                this.AddEnvironmentItem(obj as MyEnvironmentItems);
            }
        }

        private void MyEntities_OnEntityRemove(MyEntity obj)
        {
            if (obj is MyEnvironmentItems)
            {
                this.RemoveEnvironmentItem(obj as MyEnvironmentItems);
            }
        }

        private bool RaycastForExactPosition(Vector3D start, Vector3D end, out Vector3D exact)
        {
            Vector3D? nullable;
            LineD line = new LineD(start, end);
            if (this.m_ground.GetIntersectionWithLine(ref line, out nullable, true, IntersectionFlags.ALL_TRIANGLES))
            {
                exact = nullable.Value;
                return true;
            }
            exact = Vector3D.Zero;
            return false;
        }

        private void RefineSampler(Area spawnArea, ref BoundingBoxD spawnBox, ref Vector3D desiredHalfSize, MyBBSetSampler setSampler)
        {
            List<MyEntity> entitiesInAABB = MyEntities.GetEntitiesInAABB(ref spawnBox, false);
            foreach (MyEntity entity in entitiesInAABB)
            {
                if (entity is MyEnvironmentItems)
                {
                    continue;
                }
                if (!(entity is MyVoxelBase))
                {
                    BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
                    worldAABB.Inflate(desiredHalfSize);
                    worldAABB.Min.Y = DEBUG_BOX_Y_MIN_POS;
                    worldAABB.Max.Y = DEBUG_BOX_Y_MAX_POS;
                    setSampler.SubtractBB(ref worldAABB);
                }
            }
            entitiesInAABB.Clear();
            this.m_aabbTree.OverlapAllBoundingBox<Area>(ref spawnBox, this.m_tmpAreas2, 0, true);
            foreach (Area area in this.m_tmpAreas2)
            {
                if (!ReferenceEquals(area, spawnArea))
                {
                    BoundingBoxD forestBox = area.ForestBox;
                    forestBox.Inflate(desiredHalfSize);
                    setSampler.SubtractBB(ref forestBox);
                }
            }
            this.m_tmpAreas2.Clear();
        }

        private void RemoveArea(List<Area> areas, int idx)
        {
            Area local1 = areas[idx];
            int proxyId = local1.ProxyId;
            areas.RemoveAtFast<Area>(idx);
            if (proxyId != -1)
            {
                this.m_aabbTree.RemoveProxy(proxyId);
            }
            local1.Clean();
            if (RemovedArea != null)
            {
                RemovedArea(proxyId);
            }
        }

        private void RemoveEnvironmentItem(MyEnvironmentItems item)
        {
            if (this.m_envItems.ContainsKey(item.EntityId))
            {
                this.m_envItems.Remove(item.EntityId);
                item.OnMarkForClose -= new Action<MyEntity>(this.item_OnMarkForClose);
                item.ItemAdded -= new Action<MyEnvironmentItems, MyEnvironmentItems.ItemInfo>(this.item_ItemAdded);
                item.ItemRemoved -= new Action<MyEnvironmentItems, MyEnvironmentItems.ItemInfo>(this.item_ItemRemoved);
                foreach (Area area in this.m_forestAreas)
                {
                    if (area.IsValid)
                    {
                        area.ItemIds.Remove(item.EntityId);
                    }
                }
                this.m_immediateInvalidate = true;
            }
        }

        private void SplitArea(BoundingBoxD box1, BoundingBoxD box2, List<BoundingBoxD> output)
        {
            BoundingBoxD xd1 = box1.Intersect(box2);
            double x = Math.Min(xd1.Min.X, box2.Min.X);
            double num2 = xd1.Min.X;
            double num3 = xd1.Max.X;
            double num4 = Math.Max(xd1.Max.X, box2.Max.X);
            double z = Math.Min(xd1.Min.Z, box2.Min.Z);
            double num6 = xd1.Min.Z;
            double num7 = xd1.Max.Z;
            double num8 = Math.Max(xd1.Max.Z, box2.Max.Z);
            bool flag = x == num2;
            bool flag2 = num3 == num4;
            bool flag3 = z == num6;
            bool flag4 = num7 == num8;
            double y = DEBUG_BOX_Y_MIN_POS;
            double num10 = DEBUG_BOX_Y_MAX_POS;
            if (flag & flag2)
            {
                if (flag3 && !flag4)
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, num7), new Vector3D(num4, num10, num8)));
                }
                else if (!flag3 & flag4)
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num4, num10, num6)));
                }
                else
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num4, num10, num6)));
                    output.Add(new BoundingBoxD(new Vector3D(x, y, num7), new Vector3D(num4, num10, num8)));
                }
            }
            else if (flag3 & flag4)
            {
                if (flag && !flag2)
                {
                    output.Add(new BoundingBoxD(new Vector3D(num3, y, z), new Vector3D(num4, num10, num8)));
                }
                else if (!flag & flag2)
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num2, num10, num6)));
                }
                else
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num2, num10, num8)));
                    output.Add(new BoundingBoxD(new Vector3D(num3, y, z), new Vector3D(num4, num10, num8)));
                }
            }
            else if (flag)
            {
                if (flag3)
                {
                    output.Add(new BoundingBoxD(new Vector3D(num3, y, z), new Vector3D(num4, num10, num7)));
                    output.Add(new BoundingBoxD(new Vector3D(x, y, num7), new Vector3D(num4, num10, num8)));
                }
                else if (flag4)
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num4, num10, num6)));
                    output.Add(new BoundingBoxD(new Vector3D(num3, y, num6), new Vector3D(num4, num10, num8)));
                }
                else
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num4, num10, num6)));
                    output.Add(new BoundingBoxD(new Vector3D(num3, y, num6), new Vector3D(num4, num10, num7)));
                    output.Add(new BoundingBoxD(new Vector3D(x, y, num7), new Vector3D(num4, num10, num8)));
                }
            }
            else if (!flag2)
            {
                if (flag3)
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num2, num10, num8)));
                    output.Add(new BoundingBoxD(new Vector3D(num2, y, num7), new Vector3D(num3, num10, num8)));
                    output.Add(new BoundingBoxD(new Vector3D(num3, y, z), new Vector3D(num4, num10, num8)));
                }
                else if (flag4)
                {
                    output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num2, num10, num8)));
                    output.Add(new BoundingBoxD(new Vector3D(num2, y, z), new Vector3D(num3, num10, num7)));
                    output.Add(new BoundingBoxD(new Vector3D(num3, y, z), new Vector3D(num4, num10, num8)));
                }
            }
            else if (flag3)
            {
                output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num2, num10, num7)));
                output.Add(new BoundingBoxD(new Vector3D(x, y, num7), new Vector3D(num4, num10, num8)));
            }
            else if (flag4)
            {
                output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num4, num10, num6)));
                output.Add(new BoundingBoxD(new Vector3D(x, y, num6), new Vector3D(num4, num10, num8)));
            }
            else
            {
                output.Add(new BoundingBoxD(new Vector3D(x, y, z), new Vector3D(num4, num10, num6)));
                output.Add(new BoundingBoxD(new Vector3D(x, y, num6), new Vector3D(num3, num10, num7)));
                output.Add(new BoundingBoxD(new Vector3D(x, y, num7), new Vector3D(num4, num10, num8)));
            }
        }

        public static bool TryFindLocationInsideForest(out Vector3D location, Predicate<AreaData> predicate = null)
        {
            Vector3D? desiredLocationSize = null;
            return Static.TryFindLocationInsideForestInternal(desiredLocationSize, out location, predicate);
        }

        public static bool TryFindLocationInsideForest(Vector3D desiredLocationSize, out Vector3D location, Predicate<AreaData> predicate = null) => 
            Static.TryFindLocationInsideForestInternal(new Vector3D?(desiredLocationSize), out location, predicate);

        private bool TryFindLocationInsideForestInternal(Vector3D? desiredLocationSize, out Vector3D location, Predicate<AreaData> predicate = null)
        {
            if (!this.TryGetRandomAreas(this.m_tmpAreas))
            {
                location = Vector3D.Zero;
                return false;
            }
            Vector3D desiredHalfSize = (desiredLocationSize != null) ? (desiredLocationSize.Value * 0.5) : Vector3D.Zero;
            desiredHalfSize.Y = 0.0;
            int num = 0;
            int randomInt = MyUtils.GetRandomInt(this.m_tmpAreas.Count);
            while (num < this.m_tmpAreas.Count)
            {
                Area spawnArea = this.m_tmpAreas[randomInt];
                randomInt = (randomInt + 1) % this.m_tmpAreas.Count;
                num++;
                if (spawnArea.IsValid && !spawnArea.IsFull)
                {
                    Vector3D vectord2;
                    if ((predicate != null) && !predicate(spawnArea.GetAreaData()))
                    {
                        spawnArea.IsFull = true;
                        continue;
                    }
                    BoundingBoxD forestBox = spawnArea.ForestBox;
                    MyBBSetSampler setSampler = new MyBBSetSampler(forestBox.Min, forestBox.Max);
                    this.RefineSampler(spawnArea, ref forestBox, ref desiredHalfSize, setSampler);
                    if (setSampler.Valid && this.TryGetExactLocation(spawnArea, setSampler.Sample(), 10f, out vectord2))
                    {
                        location = vectord2;
                        this.d_foundEnrichingPoints.Add(vectord2);
                        this.m_tmpAreas.Clear();
                        if (SelectedArea != null)
                        {
                            SelectedArea(spawnArea.ProxyId);
                        }
                        return true;
                    }
                }
            }
            location = Vector3D.Zero;
            this.m_tmpAreas.Clear();
            return false;
        }

        public static bool TryFindLocationOutsideForest(out Vector3D location, Predicate<AreaData> predicate = null)
        {
            Vector3D? desiredLocationSize = null;
            return Static.TryFindLocationOutsideForestInternal(desiredLocationSize, out location, predicate);
        }

        public static bool TryFindLocationOutsideForest(Vector3D desiredLocationSize, out Vector3D location, Predicate<AreaData> predicate = null) => 
            Static.TryFindLocationOutsideForestInternal(new Vector3D?(desiredLocationSize), out location, predicate);

        private unsafe bool TryFindLocationOutsideForestInternal(Vector3D? desiredLocationSize, out Vector3D location, Predicate<AreaData> predicate = null)
        {
            Vector3D size = (desiredLocationSize != null) ? (desiredLocationSize.Value * 0.5) : Vector3D.Zero;
            size.Y = 0.0;
            if (this.m_highLevelBoxes.Count == 0)
            {
                bool flag = false;
                while ((this.m_initialForestLocations.Count > 0) && !flag)
                {
                    Vector3D min = this.m_initialForestLocations.Dequeue();
                    flag = true;
                    BoundingBoxD boundingBox = new BoundingBoxD(min, min);
                    boundingBox.Inflate(size);
                    List<MyEntity> entitiesInAABB = MyEntities.GetEntitiesInAABB(ref boundingBox, false);
                    foreach (MyEntity entity in entitiesInAABB)
                    {
                        if (entity is MyEnvironmentItems)
                        {
                            continue;
                        }
                        if (!(entity is MyVoxelBase) && entity.PositionComp.WorldAABB.Intersects(boundingBox))
                        {
                            flag = false;
                            break;
                        }
                    }
                    entitiesInAABB.Clear();
                    if (flag)
                    {
                        Vector3D end = min;
                        double* numPtr1 = (double*) ref end.Y;
                        numPtr1[0] -= 20.0;
                        if (this.RaycastForExactPosition(min, end, out location))
                        {
                            this.d_foundEnlargingPoints.Add(location);
                            return true;
                        }
                        flag = false;
                    }
                }
                location = Vector3D.Zero;
                return false;
            }
            if (!this.TryGetRandomAreas(this.m_tmpAreas))
            {
                location = Vector3D.Zero;
                return false;
            }
            int num = 0;
            int randomInt = MyUtils.GetRandomInt(this.m_tmpAreas.Count);
            while (num < this.m_tmpAreas.Count)
            {
                Area spawnArea = this.m_tmpAreas[randomInt];
                randomInt = (randomInt + 1) % this.m_tmpAreas.Count;
                num++;
                if (spawnArea.IsValid && !spawnArea.IsFull)
                {
                    if ((predicate != null) && !predicate(spawnArea.GetAreaData()))
                    {
                        spawnArea.IsFull = true;
                        continue;
                    }
                    BoundingBoxD forestBox = spawnArea.ForestBox;
                    BoundingBoxD bb = spawnArea.ForestBox;
                    forestBox = bb.Inflate(size);
                    forestBox.Inflate(new Vector3D(0.2, 0.0, 0.2));
                    MyBBSetSampler setSampler = new MyBBSetSampler(forestBox.Min, forestBox.Max);
                    setSampler.SubtractBB(ref bb);
                    this.RefineSampler(spawnArea, ref forestBox, ref size, setSampler);
                    if (setSampler.Valid)
                    {
                        Vector3D vectord4;
                        if (!this.TryGetExactLocation(spawnArea, setSampler.Sample(), 40f, out vectord4))
                        {
                            location = Vector3D.Zero;
                            this.m_tmpAreas.Clear();
                            return false;
                        }
                        location = vectord4;
                        this.d_foundEnlargingPoints.Add(vectord4);
                        this.m_tmpAreas.Clear();
                        return true;
                    }
                }
            }
            location = Vector3D.Zero;
            this.m_tmpAreas.Clear();
            return false;
        }

        private unsafe bool TryGetExactLocation(Area area, Vector3D point, float thresholdDistance, out Vector3D exact)
        {
            try
            {
                exact = new Vector3D();
                using (Dictionary<long, HashSet<int>>.Enumerator enumerator = area.ItemIds.GetEnumerator())
                {
                    while (true)
                    {
                        bool flag;
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<long, HashSet<int>> current = enumerator.Current;
                        MyEnvironmentItems items = this.m_envItems[current.Key];
                        HashSet<int>.Enumerator enumerator2 = area.ItemIds[current.Key].GetEnumerator();
                        try
                        {
                            while (true)
                            {
                                MatrixD xd;
                                if (!enumerator2.MoveNext())
                                {
                                    break;
                                }
                                int itemInstanceId = enumerator2.Current;
                                items.GetItemWorldMatrix(itemInstanceId, out xd);
                                point.Y = xd.Translation.Y;
                                Vector3D vectord = xd.Translation - point;
                                if (vectord.LengthSquared() < thresholdDistance)
                                {
                                    Vector3D end = new Vector3D(point.X, Math.Max((double) 0.0, (double) (point.Y - 25.0)), point.Z);
                                    double* numPtr1 = (double*) ref point.Y;
                                    numPtr1[0] += 25.0;
                                    if (this.RaycastForExactPosition(point, end, out exact))
                                    {
                                        return true;
                                    }
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            enumerator2.Dispose();
                            continue;
                        }
                        return flag;
                    }
                }
            }
            finally
            {
            }
            return false;
        }

        private bool TryGetRandomAreas(List<Area> output)
        {
            if (this.m_highLevelBoxes.Count != 0)
            {
                while (this.m_hlSelectionCounter > 0)
                {
                    this.m_hlSelectionCounter--;
                    BoundingBoxD bbox = this.m_highLevelBoxes[this.m_hlCurrentBox];
                    this.m_aabbTree.OverlapAllBoundingBox<Area>(ref bbox, output, 0, true);
                    if ((this.m_hlSelectionCounter == 0) || (output.Count == 0))
                    {
                        this.m_hlCurrentBox = (this.m_hlCurrentBox + 1) % this.m_highLevelBoxes.Count;
                        this.m_hlSelectionCounter = (int) Math.Ceiling((double) (this.m_highLevelBoxes[this.m_hlCurrentBox].Volume / this.m_hlSize));
                    }
                    if (output.Count != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
            this.m_aabbTree.Clear();
            foreach (KeyValuePair<long, MyEnvironmentItems> pair in this.m_envItems)
            {
                pair.Value.ItemAdded -= new Action<MyEnvironmentItems, MyEnvironmentItems.ItemInfo>(this.item_ItemAdded);
                pair.Value.ItemRemoved -= new Action<MyEnvironmentItems, MyEnvironmentItems.ItemInfo>(this.item_ItemRemoved);
            }
            MyEntities.OnEntityAdd -= new Action<MyEntity>(this.MyEntities_OnEntityAdd);
            MyEntities.OnEntityRemove -= new Action<MyEntity>(this.MyEntities_OnEntityRemove);
        }

        private void UpdateAreas()
        {
            if (((this.m_invalidateAreaTimer - MySandboxGame.Static.TotalTime).Seconds < 0.0) || this.m_immediateInvalidate)
            {
                this.InvalidateAreaValues();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (this.m_immediateInvalidate)
            {
                this.UpdateAreas();
            }
            int num = this.m_updateCounter + 1;
            this.m_updateCounter = num;
            if ((num % 10) == 0)
            {
                if (!this.m_loadPhase)
                {
                    if (this.m_ground != null)
                    {
                        this.UpdateAreas();
                    }
                }
                else if (!this.m_findValidForestPhase)
                {
                    this.UpdateLoad();
                }
                else
                {
                    this.UpdateFindCandidates();
                }
            }
            this.DebugDraw();
        }

        private void UpdateFindCandidates()
        {
            this.FindForestInitialCandidate();
            if (this.m_initialForestLocations.Count == 5)
            {
                this.m_loadPhase = false;
                this.m_findValidForestPhase = false;
                if (LoadFinished != null)
                {
                    LoadFinished();
                }
            }
        }

        private void UpdateLoad()
        {
            this.ConstructAreas();
            if (this.m_checkQueue.Count == 0)
            {
                bool flag = true;
                this.m_ground = MySession.Static.VoxelMaps.TryGetVoxelMapByNameStart("Ground");
                if (this.m_ground != null)
                {
                    this.m_worldArea = this.m_ground.SizeInMetres.X * this.m_ground.SizeInMetres.Z;
                    this.m_voxelCache = new MyStorageData(MyStorageDataTypeFlags.All);
                    this.m_voxelCache.Resize(Vector3I.One * 3);
                    this.InvalidateAreaValues();
                    if (this.m_highLevelBoxes.Count == 0)
                    {
                        flag = false;
                        this.m_findValidForestPhase = true;
                    }
                }
                if (flag)
                {
                    this.m_loadPhase = false;
                    if (LoadFinished != null)
                    {
                        LoadFinished();
                    }
                }
            }
        }

        public static double ForestsPercent =>
            Static.m_forestsPercent;

        public static double WorldArea =>
            Static.m_worldArea;

        public static double FullAreasRatio
        {
            get
            {
                int num = 0;
                using (List<Area>.Enumerator enumerator = Static.m_forestAreas.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.IsFull)
                        {
                            continue;
                        }
                        num++;
                    }
                }
                return (((double) num) / ((double) Static.m_forestAreas.Count));
            }
        }

        public override bool IsRequiredByGame =>
            ((MyPerGameSettings.Game == GameEnum.ME_GAME) && Sync.IsServer);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyFloraAreas.<>c <>9 = new MyFloraAreas.<>c();
            public static Comparison<BoundingBoxD> <>9__83_0;
            public static Func<BoundingBoxD, double> <>9__83_1;

            internal int <CreateHighLevelBoxes>b__83_0(BoundingBoxD x, BoundingBoxD y) => 
                y.Volume.CompareTo(x.Volume);

            internal double <CreateHighLevelBoxes>b__83_1(BoundingBoxD x) => 
                x.Volume;
        }

        private class Area
        {
            public BoundingBoxD ForestBox;
            public Dictionary<long, HashSet<int>> ItemIds;

            public Area()
            {
                this.ProxyId = -1;
                this.ForestBox = BoundingBoxD.CreateInvalid();
                this.ItemIds = new Dictionary<long, HashSet<int>>();
                this.IsFull = false;
            }

            public void AddItem(long entityId, int localId)
            {
                if (!this.ItemIds.ContainsKey(entityId))
                {
                    this.ItemIds[entityId] = new HashSet<int>();
                }
                this.ItemIds[entityId].Add(localId);
                this.IsFull = false;
                Action<int> itemAddedToArea = MyFloraAreas.ItemAddedToArea;
                if (itemAddedToArea != null)
                {
                    itemAddedToArea(this.ProxyId);
                }
            }

            public void Clean()
            {
                this.ProxyId = -1;
                this.ForestBox = BoundingBoxD.CreateInvalid();
                this.ItemIds = null;
                this.IsFull = false;
            }

            public MyFloraAreas.AreaData GetAreaData() => 
                new MyFloraAreas.AreaData(this.ProxyId, this.ForestBox, this.ItemIds);

            public void Merge(BoundingBoxD mergedBox, MyFloraAreas.Area area)
            {
                this.ForestBox = mergedBox;
                foreach (long num in area.ItemIds.Keys)
                {
                    if (!this.ItemIds.ContainsKey(num))
                    {
                        this.ItemIds.Add(num, area.ItemIds[num]);
                        continue;
                    }
                    this.ItemIds[num].UnionWith(area.ItemIds[num]);
                }
                this.IsFull = false;
                area.ForestBox = BoundingBoxD.CreateInvalid();
                area.ItemIds = null;
                Action<int> itemAddedToArea = MyFloraAreas.ItemAddedToArea;
                if (itemAddedToArea != null)
                {
                    itemAddedToArea(this.ProxyId);
                }
            }

            public void RemoveItem(long entityId, int localId)
            {
                this.ItemIds[entityId].Remove(localId);
                this.IsFull = false;
            }

            public int ProxyId { get; set; }

            public bool IsFull { get; set; }

            public bool IsValid
            {
                get
                {
                    if (this.ItemIds != null)
                    {
                        using (Dictionary<long, HashSet<int>>.Enumerator enumerator = this.ItemIds.GetEnumerator())
                        {
                            while (true)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    break;
                                }
                                KeyValuePair<long, HashSet<int>> current = enumerator.Current;
                                if (current.Value.Count > 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AreaData
        {
            public readonly int Id;
            public readonly BoundingBoxD ForestBox;
            public readonly DictionaryReader<long, HashSetReader<int>> ItemIds;
            public AreaData(int id, BoundingBoxD box, Dictionary<long, HashSet<int>> items)
            {
                this.Id = id;
                this.ForestBox = box;
                this.ItemIds = DictionaryReader<long, HashSetReader<int>>.Empty;
                Dictionary<long, HashSetReader<int>> collection = new Dictionary<long, HashSetReader<int>>();
                foreach (KeyValuePair<long, HashSet<int>> pair in items)
                {
                    collection[pair.Key] = new HashSetReader<int>(pair.Value);
                }
                this.ItemIds = new DictionaryReader<long, HashSetReader<int>>(collection);
            }
        }
    }
}

