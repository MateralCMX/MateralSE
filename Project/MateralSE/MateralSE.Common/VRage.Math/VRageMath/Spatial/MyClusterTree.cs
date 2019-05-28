namespace VRageMath.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRageMath;

    public class MyClusterTree
    {
        public Func<int, BoundingBoxD, object> OnClusterCreated;
        public Action<object> OnClusterRemoved;
        public Action<object> OnFinishBatch;
        public Action OnClustersReordered;
        public Func<long, bool> GetEntityReplicableExistsById;
        public const ulong CLUSTERED_OBJECT_ID_UNITIALIZED = ulong.MaxValue;
        public static Vector3 IdealClusterSize = new Vector3(20000f);
        public static Vector3 IdealClusterSizeHalfSqr = ((IdealClusterSize * IdealClusterSize) / 4f);
        public static Vector3 MinimumDistanceFromBorder = (IdealClusterSize / 100f);
        public static Vector3 MaximumForSplit = (IdealClusterSize * 2f);
        public static float MaximumClusterSize = 100000f;
        public readonly BoundingBoxD? SingleCluster;
        public readonly bool ForcedClusters;
        private bool m_suppressClusterReorder;
        private FastResourceLock m_clustersLock = new FastResourceLock();
        private FastResourceLock m_clustersReorderLock = new FastResourceLock();
        private MyDynamicAABBTreeD m_clusterTree = new MyDynamicAABBTreeD(Vector3D.Zero, 1.0);
        private MyDynamicAABBTreeD m_staticTree = new MyDynamicAABBTreeD(Vector3D.Zero, 1.0);
        private Dictionary<ulong, MyObjectData> m_objectsData = new Dictionary<ulong, MyObjectData>();
        private List<MyCluster> m_clusters = new List<MyCluster>();
        private ulong m_clusterObjectCounter;
        private List<MyCluster> m_returnedClusters = new List<MyCluster>(1);
        private List<object> m_userObjects = new List<object>();
        [ThreadStatic]
        private static List<MyLineSegmentOverlapResult<MyCluster>> m_lineResultListPerThread;
        [ThreadStatic]
        private static List<MyCluster> m_resultListPerThread;
        [ThreadStatic]
        private static List<ulong> m_objectDataResultListPerThread;

        public MyClusterTree(BoundingBoxD? singleCluster, bool forcedClusters)
        {
            this.SingleCluster = singleCluster;
            this.ForcedClusters = forcedClusters;
        }

        public ulong AddObject(BoundingBoxD bbox, IMyActivationHandler activationHandler, ulong? customId, string tag, long entityId, bool batch)
        {
            using (this.m_clustersLock.AcquireExclusiveUsing())
            {
                BoundingBoxD inflated;
                ulong local1;
                if ((this.SingleCluster != null) && (this.m_clusters.Count == 0))
                {
                    BoundingBoxD clusterBB = this.SingleCluster.Value;
                    clusterBB.Inflate((double) 200.0);
                    this.CreateCluster(ref clusterBB);
                }
                if ((this.SingleCluster != null) || this.ForcedClusters)
                {
                    inflated = bbox;
                }
                else
                {
                    inflated = bbox.GetInflated(IdealClusterSize / 100f);
                }
                this.m_clusterTree.OverlapAllBoundingBox<MyCluster>(ref inflated, this.m_returnedClusters, 0, true);
                MyCluster cluster = null;
                bool flag = false;
                if (this.m_returnedClusters.Count != 1)
                {
                    if (this.m_returnedClusters.Count <= 1)
                    {
                        if (this.m_returnedClusters.Count == 0)
                        {
                            if (this.SingleCluster == null)
                            {
                                if (!activationHandler.IsStaticForCluster)
                                {
                                    BoundingBoxD xd3 = new BoundingBoxD(bbox.Center - (IdealClusterSize / 2f), bbox.Center + (IdealClusterSize / 2f));
                                    this.m_clusterTree.OverlapAllBoundingBox<MyCluster>(ref xd3, this.m_returnedClusters, 0, true);
                                    if (this.m_returnedClusters.Count == 0)
                                    {
                                        this.m_staticTree.OverlapAllBoundingBox<ulong>(ref xd3, m_objectDataResultList, 0, true);
                                        cluster = this.CreateCluster(ref xd3);
                                        foreach (ulong num4 in m_objectDataResultList)
                                        {
                                            if (this.m_objectsData[num4].Cluster == null)
                                            {
                                                this.AddObjectToCluster(cluster, num4, false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        flag = true;
                                    }
                                }
                            }
                            else
                            {
                                return ulong.MaxValue;
                            }
                        }
                    }
                    else if (!activationHandler.IsStaticForCluster)
                    {
                        flag = true;
                    }
                }
                else if (this.m_returnedClusters[0].AABB.Contains(inflated) == ContainmentType.Contains)
                {
                    cluster = this.m_returnedClusters[0];
                }
                else if ((this.m_returnedClusters[0].AABB.Contains(inflated) != ContainmentType.Intersects) || !activationHandler.IsStaticForCluster)
                {
                    flag = true;
                }
                else if (this.m_returnedClusters[0].AABB.Contains(bbox) != ContainmentType.Disjoint)
                {
                    cluster = this.m_returnedClusters[0];
                }
                if (customId != null)
                {
                    local1 = customId.Value;
                }
                else
                {
                    ulong clusterObjectCounter = this.m_clusterObjectCounter;
                    this.m_clusterObjectCounter = clusterObjectCounter + ((ulong) 1L);
                    local1 = clusterObjectCounter;
                }
                ulong objectId = local1;
                int num2 = -1;
                MyObjectData data1 = new MyObjectData();
                data1.Id = objectId;
                data1.ActivationHandler = activationHandler;
                data1.AABB = bbox;
                data1.StaticId = num2;
                data1.Tag = tag;
                data1.EntityId = entityId;
                this.m_objectsData[objectId] = data1;
                if ((flag && (this.SingleCluster == null)) && !this.ForcedClusters)
                {
                    this.ReorderClusters(bbox, objectId);
                    bool isStaticForCluster = this.m_objectsData[objectId].ActivationHandler.IsStaticForCluster;
                }
                if (activationHandler.IsStaticForCluster)
                {
                    num2 = this.m_staticTree.AddProxy(ref bbox, objectId, 0, true);
                    this.m_objectsData[objectId].StaticId = num2;
                }
                return ((cluster == null) ? objectId : this.AddObjectToCluster(cluster, objectId, batch));
            }
        }

        private ulong AddObjectToCluster(MyCluster cluster, ulong objectId, bool batch)
        {
            cluster.Objects.Add(objectId);
            MyObjectData data = this.m_objectsData[objectId];
            this.m_objectsData[objectId].Id = objectId;
            this.m_objectsData[objectId].Cluster = cluster;
            if (batch)
            {
                if (data.ActivationHandler != null)
                {
                    data.ActivationHandler.ActivateBatch(cluster.UserData, objectId);
                }
            }
            else if (data.ActivationHandler != null)
            {
                data.ActivationHandler.Activate(cluster.UserData, objectId);
            }
            return objectId;
        }

        public static BoundingBoxD AdjustAABBByVelocity(BoundingBoxD aabb, Vector3 velocity, float inflate = 1.1f)
        {
            if (velocity.LengthSquared() > 0.001f)
            {
                velocity.Normalize();
            }
            aabb.Inflate((double) inflate);
            BoundingBoxD box = aabb + ((velocity * 10f) * inflate);
            aabb.Include(box);
            return aabb;
        }

        public void CastRay(Vector3D from, Vector3D to, List<MyClusterQueryResult> results)
        {
            if ((this.m_clusterTree != null) && (results != null))
            {
                LineD line = new LineD(from, to);
                this.m_clusterTree.OverlapAllLineSegment<MyCluster>(ref line, m_lineResultList, true);
                foreach (MyLineSegmentOverlapResult<MyCluster> result in m_lineResultList)
                {
                    if (result.Element != null)
                    {
                        MyClusterQueryResult item = new MyClusterQueryResult {
                            AABB = result.Element.AABB,
                            UserData = result.Element.UserData
                        };
                        results.Add(item);
                    }
                }
            }
        }

        private MyCluster CreateCluster(ref BoundingBoxD clusterBB)
        {
            MyCluster cluster1 = new MyCluster();
            cluster1.AABB = clusterBB;
            cluster1.Objects = new HashSet<ulong>();
            MyCluster userData = cluster1;
            userData.ClusterId = this.m_clusterTree.AddProxy(ref userData.AABB, userData, 0, true);
            if (this.OnClusterCreated != null)
            {
                userData.UserData = this.OnClusterCreated(userData.ClusterId, userData.AABB);
            }
            this.m_clusters.Add(userData);
            this.m_userObjects.Add(userData.UserData);
            return userData;
        }

        public void Deserialize(List<BoundingBoxD> list)
        {
            // Invalid method body.
        }

        public void Dispose()
        {
            foreach (MyCluster cluster in this.m_clusters)
            {
                if (this.OnClusterRemoved != null)
                {
                    this.OnClusterRemoved(cluster.UserData);
                }
            }
            this.m_clusters.Clear();
            this.m_userObjects.Clear();
            this.m_clusterTree.Clear();
            this.m_objectsData.Clear();
            this.m_staticTree.Clear();
            this.m_clusterObjectCounter = 0UL;
        }

        public void EnsureClusterSpace(BoundingBoxD aabb)
        {
            if ((this.SingleCluster == null) && !this.ForcedClusters)
            {
                using (this.m_clustersLock.AcquireExclusiveUsing())
                {
                    this.m_clusterTree.OverlapAllBoundingBox<MyCluster>(ref aabb, this.m_returnedClusters, 0, true);
                    bool flag = true;
                    if ((this.m_returnedClusters.Count == 1) && (this.m_returnedClusters[0].AABB.Contains(aabb) == ContainmentType.Contains))
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        ulong clusterObjectCounter = this.m_clusterObjectCounter;
                        this.m_clusterObjectCounter = clusterObjectCounter + ((ulong) 1L);
                        ulong objectId = clusterObjectCounter;
                        int num2 = -1;
                        MyObjectData data1 = new MyObjectData();
                        data1.Id = objectId;
                        data1.Cluster = null;
                        data1.ActivationHandler = null;
                        data1.AABB = aabb;
                        data1.StaticId = num2;
                        this.m_objectsData[objectId] = data1;
                        this.ReorderClusters(aabb, objectId);
                        this.RemoveObjectFromCluster(this.m_objectsData[objectId], false);
                        this.m_objectsData.Remove(objectId);
                    }
                }
            }
        }

        public void GetAll(List<MyClusterQueryResult> results)
        {
            this.m_clusterTree.GetAll<MyCluster>(m_resultList, true, null);
            foreach (MyCluster cluster in m_resultList)
            {
                MyClusterQueryResult item = new MyClusterQueryResult {
                    AABB = cluster.AABB,
                    UserData = cluster.UserData
                };
                results.Add(item);
            }
        }

        public void GetAllStaticObjects(List<BoundingBoxD> staticObjects)
        {
            this.m_staticTree.GetAll<ulong>(m_objectDataResultList, true, null);
            staticObjects.Clear();
            foreach (ulong num in m_objectDataResultList)
            {
                staticObjects.Add(this.m_objectsData[num].AABB);
            }
        }

        public MyCluster GetClusterForPosition(Vector3D pos)
        {
            BoundingSphereD sphere = new BoundingSphereD(pos, 1.0);
            this.m_clusterTree.OverlapAllBoundingSphere<MyCluster>(ref sphere, this.m_returnedClusters, true);
            return ((this.m_returnedClusters.Count > 0) ? this.m_returnedClusters.Single<MyCluster>() : null);
        }

        public ListReader<MyCluster> GetClusters() => 
            this.m_clusters;

        public ListReader<object> GetList() => 
            new ListReader<object>(this.m_userObjects);

        public ListReader<object> GetListCopy() => 
            new ListReader<object>(new List<object>(this.m_userObjects));

        public Vector3D GetObjectOffset(ulong id)
        {
            MyObjectData data;
            return (!this.m_objectsData.TryGetValue(id, out data) ? Vector3D.Zero : ((data.Cluster != null) ? data.Cluster.AABB.Center : Vector3D.Zero));
        }

        public void Intersects(Vector3D translation, List<MyClusterQueryResult> results)
        {
            BoundingBoxD bbox = new BoundingBoxD(translation - new Vector3D(1.0), translation + new Vector3D(1.0));
            this.m_clusterTree.OverlapAllBoundingBox<MyCluster>(ref bbox, m_resultList, 0, true);
            foreach (MyCluster cluster in m_resultList)
            {
                MyClusterQueryResult item = new MyClusterQueryResult {
                    AABB = cluster.AABB,
                    UserData = cluster.UserData
                };
                results.Add(item);
            }
        }

        public void MoveObject(ulong id, BoundingBoxD aabb, Vector3 velocity)
        {
            using (this.m_clustersLock.AcquireExclusiveUsing())
            {
                MyObjectData data;
                if (this.m_objectsData.TryGetValue(id, out data))
                {
                    BoundingBoxD aABB = data.AABB;
                    data.AABB = aabb;
                    if (!this.m_suppressClusterReorder)
                    {
                        BoundingBoxD xd1 = AdjustAABBByVelocity(aabb, velocity, 0f);
                        aabb = xd1;
                        ContainmentType type = data.Cluster.AABB.Contains(aabb);
                        if (((type != ContainmentType.Contains) && (this.SingleCluster == null)) && !this.ForcedClusters)
                        {
                            if (type != ContainmentType.Disjoint)
                            {
                                aabb.InflateToMinimum(IdealClusterSize);
                                this.ReorderClusters(aabb.Include(aABB), id);
                            }
                            else
                            {
                                this.m_clusterTree.OverlapAllBoundingBox<MyCluster>(ref aabb, this.m_returnedClusters, 0, true);
                                if ((this.m_returnedClusters.Count != 1) || (this.m_returnedClusters[0].AABB.Contains(aabb) != ContainmentType.Contains))
                                {
                                    aabb.InflateToMinimum(IdealClusterSize);
                                    this.ReorderClusters(aabb.Include(aABB), id);
                                }
                                else
                                {
                                    MyCluster cluster = data.Cluster;
                                    this.RemoveObjectFromCluster(data, false);
                                    if (cluster.Objects.Count == 0)
                                    {
                                        this.RemoveCluster(cluster);
                                    }
                                    this.AddObjectToCluster(this.m_returnedClusters[0], data.Id, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RemoveCluster(MyCluster cluster)
        {
            this.m_clusterTree.RemoveProxy(cluster.ClusterId);
            this.m_clusters.Remove(cluster);
            this.m_userObjects.Remove(cluster.UserData);
            if (this.OnClusterRemoved != null)
            {
                this.OnClusterRemoved(cluster.UserData);
            }
        }

        public void RemoveObject(ulong id)
        {
            MyObjectData data;
            if (this.m_objectsData.TryGetValue(id, out data))
            {
                MyCluster cluster = data.Cluster;
                if (cluster != null)
                {
                    this.RemoveObjectFromCluster(data, false);
                    if (cluster.Objects.Count == 0)
                    {
                        this.RemoveCluster(cluster);
                    }
                }
                if (data.StaticId != -1)
                {
                    this.m_staticTree.RemoveProxy(data.StaticId);
                    data.StaticId = -1;
                }
                this.m_objectsData.Remove(id);
            }
        }

        private void RemoveObjectFromCluster(MyObjectData objectData, bool batch)
        {
            objectData.Cluster.Objects.Remove(objectData.Id);
            if (batch)
            {
                if (objectData.ActivationHandler != null)
                {
                    objectData.ActivationHandler.DeactivateBatch(objectData.Cluster.UserData);
                }
            }
            else
            {
                if (objectData.ActivationHandler != null)
                {
                    objectData.ActivationHandler.Deactivate(objectData.Cluster.UserData);
                }
                this.m_objectsData[objectData.Id].Cluster = null;
            }
        }

        public unsafe void ReorderClusters(BoundingBoxD aabb, ulong objectId = 18446744073709551615UL)
        {
            using (this.m_clustersReorderLock.AcquireExclusiveUsing())
            {
                int num;
                bool flag2;
                Stack<MyClusterDescription> stack;
                List<MyClusterDescription> list;
                List<MyObjectData> list2;
                Vector3 idealClusterSize;
                MyClusterDescription description3;
                BoundingBoxD xd3;
                int num5;
                bool flag3;
                BoundingBoxD xd5;
                double minValue;
                int num8;
                BoundingBoxD inflated = aabb.GetInflated(IdealClusterSize / 2f);
                inflated.InflateToMinimum(IdealClusterSize);
                this.m_clusterTree.OverlapAllBoundingBox<MyCluster>(ref inflated, m_resultList, 0, true);
                HashSet<MyObjectData> set = new HashSet<MyObjectData>();
                bool flag = false;
                while (true)
                {
                    if (flag)
                    {
                        this.m_staticTree.OverlapAllBoundingBox<ulong>(ref inflated, m_objectDataResultList, 0, true);
                        foreach (ulong num4 in m_objectDataResultList)
                        {
                            set.Add(this.m_objectsData[num4]);
                        }
                        num = 8;
                        flag2 = true;
                        stack = new Stack<MyClusterDescription>();
                        list = new List<MyClusterDescription>();
                        list2 = null;
                        idealClusterSize = IdealClusterSize;
                        break;
                    }
                    set.Clear();
                    if (objectId != ulong.MaxValue)
                    {
                        set.Add(this.m_objectsData[objectId]);
                    }
                    using (List<MyCluster>.Enumerator enumerator = m_resultList.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Func<KeyValuePair<ulong, MyObjectData>, bool> <>9__0;
                            Func<KeyValuePair<ulong, MyObjectData>, bool> predicate = <>9__0;
                            if (<>9__0 == null)
                            {
                                Func<KeyValuePair<ulong, MyObjectData>, bool> local1 = <>9__0;
                                predicate = <>9__0 = delegate (KeyValuePair<ulong, MyObjectData> x) {
                                    MyCluster collidedCluster;
                                    return collidedCluster.Objects.Contains(x.Key);
                                };
                            }
                            foreach (MyObjectData data in from x in this.m_objectsData.Where<KeyValuePair<ulong, MyObjectData>>(predicate) select x.Value)
                            {
                                set.Add(data);
                                inflated.Include(data.AABB.GetInflated(IdealClusterSize / 2f));
                            }
                        }
                    }
                    this.m_clusterTree.OverlapAllBoundingBox<MyCluster>(ref inflated, m_resultList, 0, true);
                    flag = m_resultList.Count == m_resultList.Count;
                    this.m_staticTree.OverlapAllBoundingBox<ulong>(ref inflated, m_objectDataResultList, 0, true);
                    foreach (ulong num3 in m_objectDataResultList)
                    {
                        if (this.m_objectsData[num3].Cluster == null)
                        {
                            continue;
                        }
                        if (!m_resultList.Contains(this.m_objectsData[num3].Cluster))
                        {
                            inflated.Include(this.m_objectsData[num3].AABB.GetInflated(IdealClusterSize / 2f));
                            flag = false;
                        }
                    }
                }
                goto TR_009B;
            TR_007E:
                xd3.InflateToMinimum(idealClusterSize);
                MyClusterDescription description2 = new MyClusterDescription {
                    AABB = xd3,
                    DynamicObjects = new List<MyObjectData>(),
                    StaticObjects = new List<MyObjectData>()
                };
                MyClusterDescription item = description2;
                foreach (MyObjectData data3 in description3.DynamicObjects.ToList<MyObjectData>())
                {
                    if (xd3.Contains(data3.AABB) == ContainmentType.Contains)
                    {
                        item.DynamicObjects.Add(data3);
                        description3.DynamicObjects.Remove(data3);
                    }
                }
                foreach (MyObjectData data4 in description3.StaticObjects.ToList<MyObjectData>())
                {
                    ContainmentType type = xd3.Contains(data4.AABB);
                    if ((type == ContainmentType.Contains) || (type == ContainmentType.Intersects))
                    {
                        item.StaticObjects.Add(data4);
                        description3.StaticObjects.Remove(data4);
                    }
                }
                if (description3.DynamicObjects.Count > 0)
                {
                    BoundingBoxD xd8 = BoundingBoxD.CreateInvalid();
                    foreach (MyObjectData data5 in description3.DynamicObjects)
                    {
                        xd8.Include(data5.AABB.GetInflated(idealClusterSize / 2f));
                    }
                    xd8.InflateToMinimum(idealClusterSize);
                    description2 = new MyClusterDescription {
                        AABB = xd8,
                        DynamicObjects = description3.DynamicObjects.ToList<MyObjectData>(),
                        StaticObjects = description3.StaticObjects.ToList<MyObjectData>()
                    };
                    MyClusterDescription description5 = description2;
                    if (description5.AABB.Size.AbsMax() > (2f * idealClusterSize.AbsMax()))
                    {
                        stack.Push(description5);
                    }
                    else
                    {
                        list.Add(description5);
                    }
                }
                if ((item.AABB.Size.AbsMax() > (2f * idealClusterSize.AbsMax())) & flag3)
                {
                    stack.Push(item);
                }
                else
                {
                    list.Add(item);
                }
                goto TR_0098;
            TR_0088:
                while (true)
                {
                    if (num8 < description3.DynamicObjects.Count)
                    {
                        MyObjectData data2 = description3.DynamicObjects[num8];
                        BoundingBoxD box = description3.DynamicObjects[num8 - 1].AABB.GetInflated(idealClusterSize / 2f);
                        BoundingBoxD xd7 = data2.AABB.GetInflated(idealClusterSize / 2f);
                        double dim = box.Max.GetDim(num5);
                        if (dim > minValue)
                        {
                            minValue = dim;
                        }
                        xd5.Include(box);
                        double num10 = xd7.Min.GetDim(num5);
                        if ((num10 - box.Max.GetDim(num5)) <= 0.0)
                        {
                            break;
                        }
                        if (minValue > num10)
                        {
                            break;
                        }
                        flag3 = true;
                        xd3 = xd5;
                        goto TR_007E;
                    }
                    else
                    {
                        goto TR_007E;
                    }
                    break;
                }
                num8++;
                goto TR_0088;
            TR_0098:
                while (true)
                {
                    if (stack.Count > 0)
                    {
                        description3 = stack.Pop();
                        if (description3.DynamicObjects.Count == 0)
                        {
                            continue;
                        }
                        BoundingBoxD xd2 = BoundingBoxD.CreateInvalid();
                        int num6 = 0;
                        while (true)
                        {
                            if (num6 < description3.DynamicObjects.Count)
                            {
                                BoundingBoxD box = description3.DynamicObjects[num6].AABB.GetInflated(idealClusterSize / 2f);
                                xd2.Include(box);
                                num6++;
                                continue;
                            }
                            xd3 = xd2;
                            num5 = xd2.Size.AbsMaxComponent();
                            switch (num5)
                            {
                                case 0:
                                    description3.DynamicObjects.Sort(AABBComparerX.Static);
                                    break;

                                case 1:
                                    description3.DynamicObjects.Sort(AABBComparerY.Static);
                                    break;

                                case 2:
                                    description3.DynamicObjects.Sort(AABBComparerZ.Static);
                                    break;

                                default:
                                    break;
                            }
                            flag3 = false;
                            if (xd2.Size.AbsMax() < MaximumForSplit.AbsMax())
                            {
                                goto TR_007E;
                            }
                            else
                            {
                                xd5 = BoundingBoxD.CreateInvalid();
                                minValue = double.MinValue;
                                num8 = 1;
                            }
                            break;
                        }
                    }
                    else
                    {
                        flag2 = false;
                        foreach (MyClusterDescription description6 in list)
                        {
                            BoundingBoxD aABB = description6.AABB;
                            Vector3D size = aABB.Size;
                            if (size.AbsMax() > MaximumClusterSize)
                            {
                                flag2 = true;
                                idealClusterSize /= 2f;
                                break;
                            }
                        }
                        if (flag2)
                        {
                            num--;
                        }
                        goto TR_009B;
                    }
                    break;
                }
                goto TR_0088;
            TR_009B:
                while (true)
                {
                    if ((num > 0) & flag2)
                    {
                        MyClusterDescription* descriptionPtr1;
                        MyClusterDescription* descriptionPtr2;
                        stack.Clear();
                        list.Clear();
                        description2 = new MyClusterDescription {
                            AABB = inflated
                        };
                        descriptionPtr1->DynamicObjects = (from x in set
                            where (x.ActivationHandler == null) || !x.ActivationHandler.IsStaticForCluster
                            select x).ToList<MyObjectData>();
                        descriptionPtr1 = (MyClusterDescription*) ref description2;
                        descriptionPtr2->StaticObjects = (from x in set
                            where (x.ActivationHandler != null) && x.ActivationHandler.IsStaticForCluster
                            select x).ToList<MyObjectData>();
                        descriptionPtr2 = (MyClusterDescription*) ref description2;
                        MyClusterDescription description = description2;
                        stack.Push(description);
                        list2 = (from x in description.StaticObjects
                            where x.Cluster != null
                            select x).ToList<MyObjectData>();
                        int count = description.StaticObjects.Count;
                    }
                    else
                    {
                        HashSet<MyCluster> set2 = new HashSet<MyCluster>();
                        HashSet<MyCluster> set3 = new HashSet<MyCluster>();
                        foreach (MyObjectData data6 in list2)
                        {
                            if (data6.Cluster != null)
                            {
                                set2.Add(data6.Cluster);
                                this.RemoveObjectFromCluster(data6, true);
                            }
                        }
                        foreach (MyObjectData data7 in list2)
                        {
                            if (data7.Cluster != null)
                            {
                                data7.ActivationHandler.FinishRemoveBatch(data7.Cluster.UserData);
                                data7.Cluster = null;
                            }
                        }
                        int num2 = 0;
                        foreach (MyClusterDescription description7 in list)
                        {
                            BoundingBoxD aABB = description7.AABB;
                            MyCluster cluster = this.CreateCluster(ref aABB);
                            foreach (MyObjectData data8 in description7.DynamicObjects)
                            {
                                if (data8.Cluster != null)
                                {
                                    set2.Add(data8.Cluster);
                                    this.RemoveObjectFromCluster(data8, true);
                                }
                            }
                            foreach (MyObjectData data9 in description7.DynamicObjects)
                            {
                                if (data9.Cluster != null)
                                {
                                    data9.ActivationHandler.FinishRemoveBatch(data9.Cluster.UserData);
                                    data9.Cluster = null;
                                }
                            }
                            foreach (MyCluster cluster2 in set2)
                            {
                                if (this.OnFinishBatch != null)
                                {
                                    this.OnFinishBatch(cluster2.UserData);
                                }
                            }
                            foreach (MyObjectData data10 in description7.DynamicObjects)
                            {
                                this.AddObjectToCluster(cluster, data10.Id, true);
                            }
                            foreach (MyObjectData data11 in description7.StaticObjects)
                            {
                                if (cluster.AABB.Contains(data11.AABB) != ContainmentType.Disjoint)
                                {
                                    this.AddObjectToCluster(cluster, data11.Id, true);
                                    num2++;
                                }
                            }
                            set3.Add(cluster);
                        }
                        foreach (MyCluster cluster3 in set2)
                        {
                            this.RemoveCluster(cluster3);
                        }
                        foreach (MyCluster cluster4 in set3)
                        {
                            if (this.OnFinishBatch != null)
                            {
                                this.OnFinishBatch(cluster4.UserData);
                            }
                            foreach (ulong num12 in cluster4.Objects)
                            {
                                if (this.m_objectsData[num12].ActivationHandler != null)
                                {
                                    this.m_objectsData[num12].ActivationHandler.FinishAddBatch();
                                }
                            }
                        }
                        if (this.OnClustersReordered != null)
                        {
                            this.OnClustersReordered();
                        }
                        return;
                    }
                    break;
                }
                goto TR_0098;
            }
        }

        public void Serialize(List<BoundingBoxD> list)
        {
            foreach (MyCluster cluster in this.m_clusters)
            {
                list.Add(cluster.AABB);
            }
        }

        public bool SuppressClusterReorder
        {
            get => 
                this.m_suppressClusterReorder;
            set => 
                (this.m_suppressClusterReorder = value);
        }

        private static List<MyLineSegmentOverlapResult<MyCluster>> m_lineResultList
        {
            get
            {
                if (m_lineResultListPerThread == null)
                {
                    m_lineResultListPerThread = new List<MyLineSegmentOverlapResult<MyCluster>>();
                }
                return m_lineResultListPerThread;
            }
        }

        private static List<MyCluster> m_resultList
        {
            get
            {
                if (m_resultListPerThread == null)
                {
                    m_resultListPerThread = new List<MyCluster>();
                }
                return m_resultListPerThread;
            }
        }

        private static List<ulong> m_objectDataResultList
        {
            get
            {
                if (m_objectDataResultListPerThread == null)
                {
                    m_objectDataResultListPerThread = new List<ulong>();
                }
                return m_objectDataResultListPerThread;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyClusterTree.<>c <>9 = new MyClusterTree.<>c();
            public static Func<KeyValuePair<ulong, MyClusterTree.MyObjectData>, MyClusterTree.MyObjectData> <>9__62_1;
            public static Func<MyClusterTree.MyObjectData, bool> <>9__62_3;
            public static Func<MyClusterTree.MyObjectData, bool> <>9__62_4;
            public static Func<MyClusterTree.MyObjectData, bool> <>9__62_2;

            internal MyClusterTree.MyObjectData <ReorderClusters>b__62_1(KeyValuePair<ulong, MyClusterTree.MyObjectData> x) => 
                x.Value;

            internal bool <ReorderClusters>b__62_2(MyClusterTree.MyObjectData x) => 
                (x.Cluster != null);

            internal bool <ReorderClusters>b__62_3(MyClusterTree.MyObjectData x) => 
                ((x.ActivationHandler == null) || !x.ActivationHandler.IsStaticForCluster);

            internal bool <ReorderClusters>b__62_4(MyClusterTree.MyObjectData x) => 
                ((x.ActivationHandler != null) && x.ActivationHandler.IsStaticForCluster);
        }

        private class AABBComparerX : IComparer<MyClusterTree.MyObjectData>
        {
            public static MyClusterTree.AABBComparerX Static = new MyClusterTree.AABBComparerX();

            public int Compare(MyClusterTree.MyObjectData x, MyClusterTree.MyObjectData y) => 
                x.AABB.Min.X.CompareTo(y.AABB.Min.X);
        }

        private class AABBComparerY : IComparer<MyClusterTree.MyObjectData>
        {
            public static MyClusterTree.AABBComparerY Static = new MyClusterTree.AABBComparerY();

            public int Compare(MyClusterTree.MyObjectData x, MyClusterTree.MyObjectData y) => 
                x.AABB.Min.Y.CompareTo(y.AABB.Min.Y);
        }

        private class AABBComparerZ : IComparer<MyClusterTree.MyObjectData>
        {
            public static MyClusterTree.AABBComparerZ Static = new MyClusterTree.AABBComparerZ();

            public int Compare(MyClusterTree.MyObjectData x, MyClusterTree.MyObjectData y) => 
                x.AABB.Min.Z.CompareTo(y.AABB.Min.Z);
        }

        public interface IMyActivationHandler
        {
            void Activate(object userData, ulong clusterObjectID);
            void ActivateBatch(object userData, ulong clusterObjectID);
            void Deactivate(object userData);
            void DeactivateBatch(object userData);
            void FinishAddBatch();
            void FinishRemoveBatch(object userData);

            bool IsStaticForCluster { get; }
        }

        public class MyCluster
        {
            public int ClusterId;
            public BoundingBoxD AABB;
            public HashSet<ulong> Objects;
            public object UserData;

            public override string ToString()
            {
                object[] objArray1 = new object[] { "MyCluster", this.ClusterId, ": ", this.AABB.Center };
                return string.Concat(objArray1);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyClusterDescription
        {
            public BoundingBoxD AABB;
            public List<MyClusterTree.MyObjectData> DynamicObjects;
            public List<MyClusterTree.MyObjectData> StaticObjects;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyClusterQueryResult
        {
            public BoundingBoxD AABB;
            public object UserData;
        }

        private class MyObjectData
        {
            public ulong Id;
            public MyClusterTree.MyCluster Cluster;
            public MyClusterTree.IMyActivationHandler ActivationHandler;
            public BoundingBoxD AABB;
            public int StaticId;
            public string Tag;
            public long EntityId;
        }
    }
}

