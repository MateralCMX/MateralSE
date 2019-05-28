namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Groups;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridPhysicalGroupData : IGroupData<MyCubeGrid>
    {
        [ThreadStatic]
        private static List<HkMassElement> s_tmpElements;
        private volatile Ref<GroupSharedPxProperties> m_groupPropertiesCache;
        private MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group m_group;
        internal readonly MyGroupControlSystem ControlSystem = new MyGroupControlSystem();

        [DebuggerStepThrough, Conditional("DEBUG")]
        private static void AssertThread()
        {
            Thread currentThread = Thread.CurrentThread;
            Thread updateThread = MySandboxGame.Static.UpdateThread;
        }

        private static void DrawDebugSphere(MyCubeGrid referenceGrid, Color color, Vector3 localPosition, double radius)
        {
            MyRenderProxy.DebugDrawSphere(Vector3D.Transform(localPosition, referenceGrid.PositionComp.WorldMatrix), (float) radius, color, 1f, false, false, true, false);
        }

        private static HkMassProperties? GetGridMassProperties(MyCubeGrid grid)
        {
            if (grid.Physics != null)
            {
                return grid.Physics.Shape.MassProperties;
            }
            return null;
        }

        public static GroupSharedPxProperties GetGroupSharedProperties(MyCubeGrid localGrid, bool checkMultithreading = true)
        {
            bool flag1 = checkMultithreading;
            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(localGrid);
            if (group != null)
            {
                return group.GroupData.GetSharedPxProperties(localGrid);
            }
            return new GroupSharedPxProperties(localGrid, GetGridMassProperties(localGrid).GetValueOrDefault(), 1);
        }

        private GroupSharedPxProperties GetSharedPxProperties(MyCubeGrid referenceGrid)
        {
            Ref<GroupSharedPxProperties> groupPropertiesCache = this.m_groupPropertiesCache;
            if (groupPropertiesCache == null)
            {
                HashSetReader<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> nodes = this.m_group.Nodes;
                MatrixD worldMatrixNormalizedInv = referenceGrid.PositionComp.WorldMatrixNormalizedInv;
                using (MyUtils.ReuseCollection<HkMassElement>(ref s_tmpElements))
                {
                    using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node>.Enumerator enumerator = nodes.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyCubeGrid nodeData = enumerator.Current.NodeData;
                            HkMassProperties? gridMassProperties = GetGridMassProperties(nodeData);
                            if (gridMassProperties != null)
                            {
                                HkMassElement item = new HkMassElement {
                                    Tranform = (Matrix) (nodeData.PositionComp.WorldMatrix * worldMatrixNormalizedInv),
                                    Properties = gridMassProperties.Value
                                };
                                s_tmpElements.Add(item);
                            }
                        }
                    }
                    HkMassProperties sharedProperties = HkInertiaTensorComputer.CombineMassProperties(s_tmpElements);
                    groupPropertiesCache = Ref.Create<GroupSharedPxProperties>(new GroupSharedPxProperties(referenceGrid, sharedProperties, nodes.Count));
                    this.m_groupPropertiesCache = groupPropertiesCache;
                }
            }
            return groupPropertiesCache.Value;
        }

        private void InvalidateCoMCache()
        {
            this.m_groupPropertiesCache = null;
        }

        public static void InvalidateSharedMassPropertiesCache(MyCubeGrid groupRepresentative)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(groupRepresentative);
            if (group != null)
            {
                group.GroupData.InvalidateCoMCache();
            }
        }

        internal static bool IsMajorGroup(MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group a, MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group b)
        {
            float num = 0f;
            foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node in a.Nodes)
            {
                if (node.NodeData.Physics != null)
                {
                    num += node.NodeData.Physics.Mass;
                }
            }
            foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node2 in b.Nodes)
            {
                if (node2.NodeData.Physics != null)
                {
                    num -= node2.NodeData.Physics.Mass;
                }
            }
            return (num > 0f);
        }

        public void OnCreate<TGroupData>(MyGroups<MyCubeGrid, TGroupData>.Group group) where TGroupData: IGroupData<MyCubeGrid>, new()
        {
            this.m_group = group as MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group;
        }

        public void OnNodeAdded(MyCubeGrid entity)
        {
            this.InvalidateCoMCache();
            entity.OnAddedToGroup(this);
        }

        public void OnNodeRemoved(MyCubeGrid entity)
        {
            this.InvalidateCoMCache();
            entity.OnRemovedFromGroup(this);
        }

        public void OnRelease()
        {
            this.m_group = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GroupSharedPxProperties
        {
            public readonly int GridCount;
            public readonly MyCubeGrid ReferenceGrid;
            public readonly HkMassProperties PxProperties;
            public Matrix InertiaTensor =>
                this.PxProperties.InertiaTensor;
            public float Mass =>
                this.PxProperties.Mass;
            public Vector3D CoMWorld
            {
                get
                {
                    Vector3D vectord;
                    Vector3 centerOfMass = this.PxProperties.CenterOfMass;
                    MatrixD worldMatrix = this.ReferenceGrid.WorldMatrix;
                    Vector3D.Transform(ref centerOfMass, ref worldMatrix, out vectord);
                    return vectord;
                }
            }
            public GroupSharedPxProperties(MyCubeGrid referenceGrid, HkMassProperties sharedProperties, int gridCount)
            {
                this.GridCount = gridCount;
                this.ReferenceGrid = referenceGrid;
                this.PxProperties = sharedProperties;
            }

            public unsafe Matrix GetInertiaTensorLocalToGrid(MyCubeGrid localGrid)
            {
                MatrixD inertiaTensor = this.InertiaTensor;
                if (!ReferenceEquals(this.ReferenceGrid, localGrid))
                {
                    MatrixD xd2 = this.ReferenceGrid.WorldMatrix * localGrid.PositionComp.WorldMatrixNormalizedInv;
                    MatrixD* xdPtr1 = (MatrixD*) ref inertiaTensor;
                    MatrixD.Multiply(ref (MatrixD) ref xdPtr1, ref xd2, out inertiaTensor);
                }
                return (Matrix) inertiaTensor;
            }
        }
    }
}

