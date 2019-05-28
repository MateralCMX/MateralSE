namespace Sandbox.Game.Entities.Cube
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Models;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;
    using VRageRender.Messages;

    public class MyCubeGridRenderCell
    {
        private static readonly MyObjectBuilderType m_edgeDefinitionType = new MyObjectBuilderType(typeof(MyObjectBuilder_EdgesDefinition));
        public readonly MyRenderComponentCubeGrid m_gridRenderComponent;
        public readonly float EdgeViewDistance;
        public string DebugName;
        private BoundingBox m_boundingBox = BoundingBox.CreateInvalid();
        private BoundingBox m_tmpBoundingBox;
        private static List<MyCubeInstanceData> m_tmpInstanceData = new List<MyCubeInstanceData>();
        private static readonly Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> m_tmpInstanceParts = new Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>>();
        private static List<MyCubeInstanceDecalData> m_tmpDecalData = new List<MyCubeInstanceDecalData>();
        private uint m_parentCullObject = uint.MaxValue;
        private uint m_instanceBufferId = uint.MaxValue;
        private readonly Dictionary<MyInstanceBucket, MyRenderInstanceInfo> m_instanceInfo = new Dictionary<MyInstanceBucket, MyRenderInstanceInfo>();
        private readonly Dictionary<MyInstanceBucket, uint> m_instanceGroupRenderObjects = new Dictionary<MyInstanceBucket, uint>();
        private readonly ConcurrentDictionary<MyCubePart, ConcurrentDictionary<uint, bool>> m_cubeParts = new ConcurrentDictionary<MyCubePart, ConcurrentDictionary<uint, bool>>();
        private readonly ConcurrentDictionary<long, MyEdgeRenderData> m_edgesToRender = new ConcurrentDictionary<long, MyEdgeRenderData>();
        private readonly ConcurrentDictionary<long, MyFourEdgeInfo> m_dirtyEdges = new ConcurrentDictionary<long, MyFourEdgeInfo>();
        private readonly ConcurrentDictionary<long, MyFourEdgeInfo> m_edgeInfosNew = new ConcurrentDictionary<long, MyFourEdgeInfo>();
        private static readonly int m_edgeTypeCount = (MyUtils.GetMaxValueFromEnum<MyCubeEdgeType>() + 1);
        private static readonly Dictionary<MyStringHash, int[]> m_edgeModelIdCache = new Dictionary<MyStringHash, int[]>(MyStringHash.Comparer);
        private static readonly List<EdgeInfoNormal> m_edgesToCompare = new List<EdgeInfoNormal>();

        public MyCubeGridRenderCell(MyRenderComponentCubeGrid gridRender)
        {
            this.m_gridRenderComponent = gridRender;
            this.EdgeViewDistance = (gridRender.GridSizeEnum == MyCubeSize.Large) ? ((float) 130) : ((float) 0x23);
        }

        public void AddCubePart(MyCubePart part)
        {
            this.m_cubeParts.TryAdd(part, null);
        }

        internal void AddCubePartDecal(MyCubePart part, uint decalId)
        {
            this.m_cubeParts.GetOrAdd(part, x => new ConcurrentDictionary<uint, bool>()).TryAdd(decalId, true);
        }

        public bool AddEdgeInfo(long hash, MyEdgeInfo info, MySlimBlock owner)
        {
            MyFourEdgeInfo orAdd;
            bool flag;
            if (!this.m_edgeInfosNew.TryGetValue(hash, out orAdd))
            {
                orAdd = new MyFourEdgeInfo(info.LocalOrthoMatrix, info.EdgeType);
                orAdd = this.m_edgeInfosNew.GetOrAdd(hash, orAdd);
            }
            MyFourEdgeInfo info3 = orAdd;
            lock (info3)
            {
                flag = orAdd.AddInstance((Vector3) (owner.Position * owner.CubeGrid.GridSize), info.Color, owner.SkinSubtypeId, info.EdgeModel, info.PackedNormal0, info.PackedNormal1);
            }
            if (flag)
            {
                if (!orAdd.Full)
                {
                    this.m_dirtyEdges[hash] = orAdd;
                }
                else
                {
                    this.m_dirtyEdges.Remove<long, MyFourEdgeInfo>(hash);
                    this.m_edgesToRender.Remove<long, MyEdgeRenderData>(hash);
                }
            }
            return flag;
        }

        private void AddEdgeParts(Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> instanceParts)
        {
            MyCubeInstanceData instance = new MyCubeInstanceData();
            instance.ResetBones();
            instance.SetTextureOffset(new Vector4UByte(0, 0, 1, 1));
            foreach (KeyValuePair<long, MyEdgeRenderData> pair in this.m_edgesToRender)
            {
                int modelId = pair.Value.ModelId;
                MyFourEdgeInfo edgeInfo = pair.Value.EdgeInfo;
                instance.PackedOrthoMatrix = edgeInfo.LocalOrthoMatrix;
                this.AddInstancePart(instanceParts, modelId, MyStringHash.NullOrEmpty, ref instance, null, 0, this.EdgeViewDistance);
            }
        }

        private void AddInstancePart(Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> instanceParts, int modelId, MyStringHash skinSubtypeId, ref MyCubeInstanceData instance, ConcurrentDictionary<uint, bool> decals, MyInstanceFlagsEnum flags, float maxViewDistance = 3.402823E+38f)
        {
            Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo> tuple;
            MyInstanceBucket key = new MyInstanceBucket(modelId, skinSubtypeId);
            if (!instanceParts.TryGetValue(key, out tuple))
            {
                tuple = Tuple.Create<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>(new List<MyCubeInstanceMergedData>(), new Sandbox.Game.Entities.Cube.MyInstanceInfo(flags, maxViewDistance));
                instanceParts.Add(key, tuple);
            }
            Vector3 translation = instance.LocalMatrix.Translation;
            this.m_tmpBoundingBox.Min = translation - new Vector3(this.m_gridRenderComponent.GridSize);
            this.m_tmpBoundingBox.Max = translation + new Vector3(this.m_gridRenderComponent.GridSize);
            this.m_boundingBox.Include(this.m_tmpBoundingBox);
            MyCubeInstanceMergedData item = new MyCubeInstanceMergedData {
                CubeInstanceData = instance,
                Decals = decals
            };
            tuple.Item1.Add(item);
        }

        private void AddRenderObjectId(bool registerForPositionUpdates, out uint renderObjectId, uint newId)
        {
            renderObjectId = newId;
            Sandbox.Game.Entities.MyEntities.AddRenderObjectToMap(newId, this.m_gridRenderComponent.Container.Entity);
            if (registerForPositionUpdates)
            {
                try
                {
                    while (true)
                    {
                        uint[] renderObjectIDs = this.m_gridRenderComponent.RenderObjectIDs;
                        int index = 0;
                        while (true)
                        {
                            if (index < renderObjectIDs.Length)
                            {
                                if (renderObjectIDs[index] != uint.MaxValue)
                                {
                                    index++;
                                    continue;
                                }
                                renderObjectIDs[index] = renderObjectId;
                            }
                            else
                            {
                                this.m_gridRenderComponent.ResizeRenderObjectArray(renderObjectIDs.Length + 3);
                                continue;
                            }
                            break;
                        }
                        break;
                    }
                }
                finally
                {
                    this.m_gridRenderComponent.SetVisibilityUpdates(true);
                }
            }
        }

        private void ClearInstanceParts(Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> instanceParts)
        {
            this.m_boundingBox = BoundingBox.CreateInvalid();
            foreach (KeyValuePair<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> pair in instanceParts)
            {
                pair.Value.Item1.Clear();
            }
        }

        internal void DebugDraw()
        {
            string text = $"CubeParts:{this.m_cubeParts.Count}, EdgeParts{this.m_edgeInfosNew.Count}";
            MyRenderProxy.DebugDrawText3D(this.m_boundingBox.Center + this.m_gridRenderComponent.Container.Entity.PositionComp.WorldMatrix.Translation, text, Color.Red, 0.75f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            MyRenderProxy.DebugDrawOBB((Matrix.CreateScale(this.m_boundingBox.Size) * Matrix.CreateTranslation(this.m_boundingBox.Center)) * this.m_gridRenderComponent.Container.Entity.PositionComp.WorldMatrix, Color.Red.ToVector3(), 0.25f, true, true, true, false);
        }

        private bool InstanceDataCleared(Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> instanceParts)
        {
            using (Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>>.Enumerator enumerator = instanceParts.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> current = enumerator.Current;
                    if (current.Value.Item1.Count > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsEdgeVisible(MyFourEdgeInfo edgeInfo, out int modelId)
        {
            int[] numArray;
            modelId = 0;
            m_edgesToCompare.Clear();
            if (edgeInfo.Full)
            {
                return false;
            }
            for (int i = 0; i < 4; i++)
            {
                Color color;
                MyStringHash hash2;
                MyStringHash hash3;
                Base27Directions.Direction direction;
                Base27Directions.Direction direction2;
                if (edgeInfo.GetNormalInfo(i, out color, out hash3, out hash2, out direction, out direction2))
                {
                    EdgeInfoNormal item = new EdgeInfoNormal {
                        Normal = Base27Directions.GetVector(direction),
                        Color = color,
                        SkinSubtypeId = hash3,
                        EdgeModel = hash2
                    };
                    m_edgesToCompare.Add(item);
                    item = new EdgeInfoNormal {
                        Normal = Base27Directions.GetVector(direction2),
                        Color = color,
                        SkinSubtypeId = hash3,
                        EdgeModel = hash2
                    };
                    m_edgesToCompare.Add(item);
                }
            }
            if (m_edgesToCompare.Count == 0)
            {
                return false;
            }
            bool flag = m_edgesToCompare.Count == 4;
            MyStringHash edgeModel = m_edgesToCompare[0].EdgeModel;
            int index = 0;
            while (index < m_edgesToCompare.Count)
            {
                int num6 = index + 1;
                while (true)
                {
                    if (num6 < m_edgesToCompare.Count)
                    {
                        if (!MyUtils.IsZero(m_edgesToCompare[index].Normal + m_edgesToCompare[num6].Normal, 0.1f))
                        {
                            num6++;
                            continue;
                        }
                        m_edgesToCompare.RemoveAt(num6);
                        m_edgesToCompare.RemoveAt(index);
                        index--;
                    }
                    index++;
                    break;
                }
            }
            if (m_edgesToCompare.Count == 1)
            {
                return false;
            }
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            if (m_edgesToCompare.Count > 0)
            {
                Color color = m_edgesToCompare[0].Color;
                MyStringHash skinSubtypeId = m_edgesToCompare[0].SkinSubtypeId;
                edgeModel = m_edgesToCompare[0].EdgeModel;
                for (int j = 1; j < m_edgesToCompare.Count; j++)
                {
                    EdgeInfoNormal normal2 = m_edgesToCompare[j];
                    flag2 |= normal2.Color != color;
                    flag4 |= normal2.SkinSubtypeId != skinSubtypeId;
                    flag3 |= edgeModel != normal2.EdgeModel;
                    if ((flag2 | flag3) | flag4)
                    {
                        break;
                    }
                }
            }
            if (!((m_edgesToCompare.Count != 1) ? (!((flag2 | flag3) | flag4) ? ((m_edgesToCompare.Count <= 2) ? ((m_edgesToCompare.Count != 0) ? (Math.Abs(Vector3.Dot(m_edgesToCompare[0].Normal, m_edgesToCompare[1].Normal)) <= 0.85f) : flag) : true) : true) : false))
            {
                return false;
            }
            int edgeTypeCount = m_edgeTypeCount;
            if (!m_edgeModelIdCache.TryGetValue(edgeModel, out numArray))
            {
                MyDefinitionId id = new MyDefinitionId(m_edgeDefinitionType, edgeModel);
                MyEdgesDefinition edgesDefinition = MyDefinitionManager.Static.GetEdgesDefinition(id);
                MyEdgesModelSet small = edgesDefinition.Small;
                MyEdgesModelSet large = edgesDefinition.Large;
                numArray = new int[m_edgeTypeCount * 2];
                MyCubeEdgeType[] values = MyEnum<MyCubeEdgeType>.Values;
                int num8 = 0;
                while (true)
                {
                    int num9;
                    int num10;
                    if (num8 >= values.Length)
                    {
                        m_edgeModelIdCache.Add(edgeModel, numArray);
                        break;
                    }
                    MyCubeEdgeType type = values[num8];
                    switch (type)
                    {
                        case MyCubeEdgeType.Vertical:
                            num9 = MyModel.GetId(small.Vertical);
                            num10 = MyModel.GetId(large.Vertical);
                            break;

                        case MyCubeEdgeType.Vertical_Diagonal:
                            num9 = MyModel.GetId(small.VerticalDiagonal);
                            num10 = MyModel.GetId(large.VerticalDiagonal);
                            break;

                        case MyCubeEdgeType.Horizontal:
                            num9 = MyModel.GetId(small.Horisontal);
                            num10 = MyModel.GetId(large.Horisontal);
                            break;

                        case MyCubeEdgeType.Horizontal_Diagonal:
                            num9 = MyModel.GetId(small.HorisontalDiagonal);
                            num10 = MyModel.GetId(large.HorisontalDiagonal);
                            break;

                        case MyCubeEdgeType.Hidden:
                            num9 = 0;
                            num10 = 0;
                            break;

                        default:
                            throw new Exception("Unhandled edge type");
                    }
                    int num11 = (int) type;
                    numArray[num11] = num9;
                    numArray[num11 + edgeTypeCount] = num10;
                    num8++;
                }
            }
            int edgeType = (int) edgeInfo.EdgeType;
            modelId = numArray[((this.m_gridRenderComponent.GridSizeEnum == MyCubeSize.Large) ? edgeTypeCount : 0) + edgeType];
            return true;
        }

        public void OnRemovedFromRender()
        {
            this.UpdateRenderEntitiesInstanceData(0, this.m_parentCullObject);
            if (this.m_parentCullObject != uint.MaxValue)
            {
                this.RemoveRenderObjectId(true, ref this.m_parentCullObject, MyRenderProxy.ObjectType.ManualCull);
            }
            if (this.m_instanceBufferId != uint.MaxValue)
            {
                this.RemoveRenderObjectId(false, ref this.m_instanceBufferId, MyRenderProxy.ObjectType.InstanceBuffer);
            }
        }

        public void RebuildInstanceParts(RenderFlags renderFlags)
        {
            Thread currentThread = Thread.CurrentThread;
            Thread updateThread = MySandboxGame.Static.UpdateThread;
            Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> tmpInstanceParts = m_tmpInstanceParts;
            foreach (KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>> pair in this.m_cubeParts)
            {
                MyCubePart key = pair.Key;
                ConcurrentDictionary<uint, bool> decals = pair.Value;
                this.AddInstancePart(tmpInstanceParts, key.Model.UniqueId, key.SkinSubtypeId, ref key.InstanceData, decals, MyInstanceFlagsEnum.CastShadows | MyInstanceFlagsEnum.EnableColorMask | MyInstanceFlagsEnum.ShowLod1, float.MaxValue);
            }
            this.UpdateDirtyEdges();
            this.AddEdgeParts(tmpInstanceParts);
            this.UpdateRenderInstanceData(tmpInstanceParts, renderFlags);
            this.ClearInstanceParts(tmpInstanceParts);
            if (this.m_gridRenderComponent != null)
            {
                this.m_gridRenderComponent.FadeIn = false;
            }
        }

        public bool RemoveCubePart(MyCubePart part)
        {
            ConcurrentDictionary<uint, bool> dictionary;
            return this.m_cubeParts.TryRemove(part, out dictionary);
        }

        internal void RemoveCubePartDecal(MyCubePart part, uint decalId)
        {
            ConcurrentDictionary<uint, bool> dictionary;
            if (this.m_cubeParts.TryGetValue(part, out dictionary))
            {
                bool flag;
                dictionary.TryRemove(decalId, out flag);
            }
        }

        public bool RemoveEdgeInfo(long hash, MySlimBlock owner)
        {
            MyFourEdgeInfo info;
            bool local1 = this.m_edgeInfosNew.TryGetValue(hash, out info) && info.RemoveInstance((Vector3) (owner.Position * owner.CubeGrid.GridSize));
            if (local1)
            {
                if (info.Empty)
                {
                    this.m_dirtyEdges.Remove<long, MyFourEdgeInfo>(hash);
                    this.m_edgeInfosNew.Remove<long, MyFourEdgeInfo>(hash);
                    this.m_edgesToRender.Remove<long, MyEdgeRenderData>(hash);
                    return local1;
                }
                this.m_dirtyEdges[hash] = info;
            }
            return local1;
        }

        private void RemoveRenderObjectId(bool unregisterFromPositionUpdates, ref uint renderObjectId, MyRenderProxy.ObjectType type)
        {
            Sandbox.Game.Entities.MyEntities.RemoveRenderObjectFromMap(renderObjectId);
            MyRenderProxy.RemoveRenderObject(renderObjectId, type, this.m_gridRenderComponent.FadeOut);
            if (unregisterFromPositionUpdates)
            {
                uint[] renderObjectIDs = this.m_gridRenderComponent.RenderObjectIDs;
                for (int i = 0; i < renderObjectIDs.Length; i++)
                {
                    if (renderObjectIDs[i] == renderObjectId)
                    {
                        renderObjectIDs[i] = uint.MaxValue;
                        break;
                    }
                }
            }
            renderObjectId = uint.MaxValue;
        }

        private void UpdateDirtyEdges()
        {
            foreach (KeyValuePair<long, MyFourEdgeInfo> pair in this.m_dirtyEdges)
            {
                int num;
                if (this.IsEdgeVisible(pair.Value, out num))
                {
                    this.m_edgesToRender[pair.Key] = new MyEdgeRenderData(num, pair.Value);
                    continue;
                }
                this.m_edgesToRender.Remove<long, MyEdgeRenderData>(pair.Key);
            }
        }

        private unsafe void UpdateRenderEntitiesInstanceData(RenderFlags renderFlags, uint parentCullObject)
        {
            foreach (KeyValuePair<MyInstanceBucket, MyRenderInstanceInfo> pair in this.m_instanceInfo)
            {
                uint num;
                bool inScene;
                int num1;
                bool flag = this.m_instanceGroupRenderObjects.TryGetValue(pair.Key, out num);
                if (pair.Value.InstanceCount <= 0)
                {
                    inScene = false;
                }
                else
                {
                    IMyEntity entity = this.m_gridRenderComponent.Entity;
                    if (entity != null)
                    {
                        inScene = entity.InScene;
                    }
                    else
                    {
                        IMyEntity local1 = entity;
                        inScene = false;
                    }
                }
                bool flag2 = (bool) num1;
                RenderFlags flags = renderFlags;
                if (!(!flag & flag2))
                {
                    if (flag && !flag2)
                    {
                        uint renderObjectId = this.m_instanceGroupRenderObjects[pair.Key];
                        this.RemoveRenderObjectId(!MyFakes.MANUAL_CULL_OBJECTS, ref renderObjectId, MyRenderProxy.ObjectType.Entity);
                        this.m_instanceGroupRenderObjects.Remove(pair.Key);
                        continue;
                    }
                }
                else
                {
                    uint* numPtr1;
                    object[] objArray1 = new object[] { "CubeGridRenderCell ", this.m_gridRenderComponent.Container.Entity.DisplayName, " ", this.m_gridRenderComponent.Container.Entity.EntityId, ", part: ", pair.Key };
                    this.AddRenderObjectId(!MyFakes.MANUAL_CULL_OBJECTS, out (uint) ref numPtr1, MyRenderProxy.CreateRenderEntity(string.Concat(objArray1), MyModel.GetById(pair.Key.ModelId), this.m_gridRenderComponent.Container.Entity.PositionComp.WorldMatrix, MyMeshDrawTechnique.MESH, flags, CullingOptions.Default, this.m_gridRenderComponent.GetDiffuseColor(), Vector3.Zero, this.m_gridRenderComponent.Transparency, pair.Value.MaxViewDistance, 0, this.m_gridRenderComponent.CubeGrid.GridScale, (this.m_gridRenderComponent.Transparency == 0f) && this.m_gridRenderComponent.FadeIn));
                    if (pair.Key.SkinSubtypeId != MyStringHash.NullOrEmpty)
                    {
                        Dictionary<string, MyTextureChange> assetModifierDefinitionForRender = MyDefinitionManager.Static.GetAssetModifierDefinitionForRender(pair.Key.SkinSubtypeId);
                        if (assetModifierDefinitionForRender != null)
                        {
                            numPtr1 = (uint*) ref num;
                            MyRenderProxy.ChangeMaterialTexture(num, assetModifierDefinitionForRender);
                        }
                    }
                    this.m_instanceGroupRenderObjects[pair.Key] = num;
                    if (MyFakes.MANUAL_CULL_OBJECTS)
                    {
                        MyRenderProxy.SetParentCullObject(num, parentCullObject, new Matrix?(Matrix.Identity));
                    }
                }
                if (flag2)
                {
                    MyRenderProxy.SetInstanceBuffer(num, pair.Value.InstanceBufferId, pair.Value.InstanceStart, pair.Value.InstanceCount, this.m_boundingBox, null);
                }
            }
        }

        private void UpdateRenderInstanceData(Dictionary<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> instanceParts, RenderFlags renderFlags)
        {
            if (this.m_parentCullObject == uint.MaxValue)
            {
                object[] objArray1 = new object[] { this.m_gridRenderComponent.Container.Entity.DisplayName, " ", this.m_gridRenderComponent.Container.Entity.EntityId, ", cull object" };
                this.AddRenderObjectId(true, out this.m_parentCullObject, MyRenderProxy.CreateManualCullObject(string.Concat(objArray1), this.m_gridRenderComponent.Container.Entity.PositionComp.WorldMatrix));
            }
            if (this.m_instanceBufferId == uint.MaxValue)
            {
                object[] objArray2 = new object[] { this.m_gridRenderComponent.Container.Entity.DisplayName, " ", this.m_gridRenderComponent.Container.Entity.EntityId, ", instance buffer ", this.DebugName };
                this.AddRenderObjectId(false, out this.m_instanceBufferId, MyRenderProxy.CreateRenderInstanceBuffer(string.Concat(objArray2), MyRenderInstanceBufferType.Cube, this.m_gridRenderComponent.GetRenderObjectID()));
            }
            this.m_instanceInfo.Clear();
            m_tmpDecalData.AssertEmpty<MyCubeInstanceDecalData>();
            m_tmpInstanceData.AssertEmpty<MyCubeInstanceData>();
            int num = -1;
            foreach (KeyValuePair<MyInstanceBucket, Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo>> pair in instanceParts)
            {
                MyInstanceBucket key = pair.Key;
                Tuple<List<MyCubeInstanceMergedData>, Sandbox.Game.Entities.Cube.MyInstanceInfo> tuple = pair.Value;
                this.m_instanceInfo.Add(key, new MyRenderInstanceInfo(this.m_instanceBufferId, m_tmpInstanceData.Count, tuple.Item1.Count, tuple.Item2.MaxViewDistance, tuple.Item2.Flags));
                List<MyCubeInstanceMergedData> list = tuple.Item1;
                for (int i = 0; i < list.Count; i++)
                {
                    num++;
                    m_tmpInstanceData.Add(list[i].CubeInstanceData);
                    ConcurrentDictionary<uint, bool> decals = list[i].Decals;
                    if (decals != null)
                    {
                        foreach (uint num3 in decals.Keys)
                        {
                            MyCubeInstanceDecalData item = new MyCubeInstanceDecalData {
                                DecalId = num3,
                                InstanceIndex = num
                            };
                            m_tmpDecalData.Add(item);
                        }
                    }
                }
            }
            if (m_tmpInstanceData.Count > 0)
            {
                MyRenderProxy.UpdateRenderCubeInstanceBuffer(this.m_instanceBufferId, ref m_tmpInstanceData, (int) (m_tmpInstanceData.Count * 1.2f), ref m_tmpDecalData);
            }
            m_tmpDecalData.AssertEmpty<MyCubeInstanceDecalData>();
            m_tmpInstanceData.AssertEmpty<MyCubeInstanceData>();
            this.UpdateRenderEntitiesInstanceData(renderFlags, this.m_parentCullObject);
        }

        public ConcurrentDictionary<MyCubePart, ConcurrentDictionary<uint, bool>> CubeParts =>
            this.m_cubeParts;

        internal uint ParentCullObject =>
            this.m_parentCullObject;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCubeGridRenderCell.<>c <>9 = new MyCubeGridRenderCell.<>c();
            public static Func<MyCubePart, ConcurrentDictionary<uint, bool>> <>9__24_0;

            internal ConcurrentDictionary<uint, bool> <AddCubePartDecal>b__24_0(MyCubePart x) => 
                new ConcurrentDictionary<uint, bool>();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EdgeInfoNormal
        {
            public Vector3 Normal;
            public VRageMath.Color Color;
            public MyStringHash SkinSubtypeId;
            public MyStringHash EdgeModel;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyCubeInstanceMergedData
        {
            public MyCubeInstanceData CubeInstanceData;
            public ConcurrentDictionary<uint, bool> Decals;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyEdgeRenderData
        {
            public readonly int ModelId;
            public readonly MyFourEdgeInfo EdgeInfo;
            public MyEdgeRenderData(int modelId, MyFourEdgeInfo edgeInfo)
            {
                this.ModelId = modelId;
                this.EdgeInfo = edgeInfo;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyInstanceBucket
        {
            public int ModelId;
            public MyStringHash SkinSubtypeId;
            public MyInstanceBucket(int modelId, MyStringHash skinSubtypeId)
            {
                this.ModelId = modelId;
                this.SkinSubtypeId = skinSubtypeId;
            }
        }
    }
}

