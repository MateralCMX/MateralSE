namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Algorithms;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyHighLevelGroup : MyPathFindingSystem<MyNavigationPrimitive>, IMyNavigationGroup
    {
        private IMyNavigationGroup m_lowLevel;
        private Dictionary<int, MyHighLevelPrimitive> m_primitives;
        private Dictionary<int, List<IMyHighLevelPrimitiveObserver>> m_primitiveObservers;
        private MyNavgroupLinks m_links;
        private int m_removingPrimitive;
        private static List<int> m_tmpNeighbors = new List<int>();

        public MyHighLevelGroup(IMyNavigationGroup lowLevelPathfinding, MyNavgroupLinks links, Func<long> timestampFunction) : base(0x80, timestampFunction)
        {
            this.m_removingPrimitive = -1;
            this.m_lowLevel = lowLevelPathfinding;
            this.m_primitives = new Dictionary<int, MyHighLevelPrimitive>();
            this.m_primitiveObservers = new Dictionary<int, List<IMyHighLevelPrimitiveObserver>>();
            this.m_links = links;
        }

        public void AddPrimitive(int index, Vector3 localPosition)
        {
            this.m_primitives.Add(index, new MyHighLevelPrimitive(this, index, localPosition));
        }

        private void Connect(int a, int b)
        {
            MyHighLevelPrimitive primitive = this.GetPrimitive(a);
            MyHighLevelPrimitive primitive2 = this.GetPrimitive(b);
            if ((primitive != null) && (primitive2 != null))
            {
                primitive.Connect(b);
                primitive2.Connect(a);
            }
        }

        public void ConnectPrimitives(int a, int b)
        {
            this.Connect(a, b);
        }

        public void DebugDraw(bool lite)
        {
            long lastHighLevelTimestamp = MyCestmirPathfindingShorts.Pathfinding.LastHighLevelTimestamp;
            foreach (KeyValuePair<int, MyHighLevelPrimitive> pair in this.m_primitives)
            {
                if (lite)
                {
                    MyRenderProxy.DebugDrawPoint(pair.Value.WorldPosition, Color.CadetBlue, false, false);
                    continue;
                }
                MyHighLevelPrimitive primitive = pair.Value;
                Vector3D vectord = MySector.MainCamera.WorldMatrix.Down * 0.30000001192092896;
                float num2 = (float) Vector3D.Distance(primitive.WorldPosition, MySector.MainCamera.Position);
                float scale = 7f / num2;
                if (scale > 30f)
                {
                    scale = 30f;
                }
                if (scale < 0.5f)
                {
                    scale = 0.5f;
                }
                if (num2 < 100f)
                {
                    List<IMyHighLevelPrimitiveObserver> list = null;
                    if (this.m_primitiveObservers.TryGetValue(pair.Key, out list))
                    {
                        MyRenderProxy.DebugDrawText3D(primitive.WorldPosition + vectord, list.Count.ToString(), Color.Red, scale * 3f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                    MyRenderProxy.DebugDrawText3D(primitive.WorldPosition + vectord, pair.Key.ToString(), Color.CadetBlue, scale, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
                int index = 0;
                while (true)
                {
                    if (index >= primitive.GetOwnNeighborCount())
                    {
                        if (primitive.PathfindingData.GetTimestamp() == lastHighLevelTimestamp)
                        {
                            MyRenderProxy.DebugDrawSphere(primitive.WorldPosition, 0.5f, Color.DarkRed, 1f, false, false, true, false);
                        }
                        break;
                    }
                    MyHighLevelPrimitive ownNeighbor = primitive.GetOwnNeighbor(index) as MyHighLevelPrimitive;
                    MyRenderProxy.DebugDrawLine3D(primitive.WorldPosition, ownNeighbor.WorldPosition, Color.CadetBlue, Color.CadetBlue, false, false);
                    index++;
                }
            }
        }

        private void Disconnect(int a, int b)
        {
            MyHighLevelPrimitive primitive = this.GetPrimitive(a);
            MyHighLevelPrimitive primitive2 = this.GetPrimitive(b);
            if ((primitive != null) && (primitive2 != null))
            {
                primitive.Disconnect(b);
                primitive2.Disconnect(a);
            }
        }

        public void DisconnectPrimitives(int a, int b)
        {
            this.Disconnect(a, b);
        }

        public MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, ref double closestDistanceSq)
        {
            throw new NotImplementedException();
        }

        public IMyHighLevelComponent GetComponent(MyHighLevelPrimitive highLevelPrimitive) => 
            null;

        public IMyPathEdge<MyNavigationPrimitive> GetExternalEdge(MyNavigationPrimitive primitive, int index) => 
            this.m_links.GetEdge(primitive, index);

        public MyNavigationPrimitive GetExternalNeighbor(MyNavigationPrimitive primitive, int index) => 
            this.m_links.GetLinkedNeighbor(primitive, index);

        public int GetExternalNeighborCount(MyNavigationPrimitive primitive) => 
            this.m_links.GetLinkCount(primitive);

        public MyHighLevelPrimitive GetHighLevelPrimitive(MyNavigationPrimitive myNavigationTriangle) => 
            null;

        public MyHighLevelPrimitive GetPrimitive(int index)
        {
            MyHighLevelPrimitive primitive = null;
            this.m_primitives.TryGetValue(index, out primitive);
            return primitive;
        }

        public void GetPrimitivesOnPath(ref List<MyHighLevelPrimitive> primitives)
        {
            foreach (KeyValuePair<int, List<IMyHighLevelPrimitiveObserver>> pair in this.m_primitiveObservers)
            {
                MyHighLevelPrimitive item = this.TryGetPrimitive(pair.Key);
                primitives.Add(item);
            }
        }

        public Vector3 GlobalToLocal(Vector3D globalPos) => 
            this.m_lowLevel.GlobalToLocal(globalPos);

        public Vector3D LocalToGlobal(Vector3 localPos) => 
            this.m_lowLevel.LocalToGlobal(localPos);

        public void ObservePrimitive(MyHighLevelPrimitive primitive, IMyHighLevelPrimitiveObserver observer)
        {
            if (ReferenceEquals(primitive.Parent, this))
            {
                List<IMyHighLevelPrimitiveObserver> list = null;
                int index = primitive.Index;
                if (!this.m_primitiveObservers.TryGetValue(index, out list))
                {
                    list = new List<IMyHighLevelPrimitiveObserver>(4);
                    this.m_primitiveObservers.Add(index, list);
                }
                list.Add(observer);
            }
        }

        public void RefinePath(MyPath<MyNavigationPrimitive> path, List<Vector4D> output, ref Vector3 startPoint, ref Vector3 endPoint, int begin, int end)
        {
            throw new NotImplementedException();
        }

        public void RemovePrimitive(int index)
        {
            this.m_removingPrimitive = index;
            MyHighLevelPrimitive primitive = null;
            if (!this.m_primitives.TryGetValue(index, out primitive))
            {
                this.m_removingPrimitive = -1;
            }
            else
            {
                List<IMyHighLevelPrimitiveObserver> list = null;
                if (this.m_primitiveObservers.TryGetValue(index, out list))
                {
                    using (List<IMyHighLevelPrimitiveObserver>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Invalidate();
                        }
                    }
                }
                this.m_primitiveObservers.Remove(index);
                this.m_links.RemoveAllLinks(primitive);
                m_tmpNeighbors.Clear();
                primitive.GetNeighbours(m_tmpNeighbors);
                foreach (int num in m_tmpNeighbors)
                {
                    MyHighLevelPrimitive primitive2 = null;
                    this.m_primitives.TryGetValue(num, out primitive2);
                    if (primitive2 != null)
                    {
                        primitive2.Disconnect(index);
                    }
                }
                this.m_primitives.Remove(index);
                this.m_removingPrimitive = -1;
            }
        }

        public void StopObservingPrimitive(MyHighLevelPrimitive primitive, IMyHighLevelPrimitiveObserver observer)
        {
            if (ReferenceEquals(primitive.Parent, this))
            {
                List<IMyHighLevelPrimitiveObserver> list = null;
                int index = primitive.Index;
                if ((index != this.m_removingPrimitive) && this.m_primitiveObservers.TryGetValue(index, out list))
                {
                    list.Remove(observer);
                    if (list.Count == 0)
                    {
                        this.m_primitiveObservers.Remove(index);
                    }
                }
            }
        }

        public override string ToString() => 
            ((this.m_lowLevel != null) ? ("HLPFG of " + this.m_lowLevel.ToString()) : "Invalid HLPFG");

        public MyHighLevelPrimitive TryGetPrimitive(int index)
        {
            MyHighLevelPrimitive primitive = null;
            this.m_primitives.TryGetValue(index, out primitive);
            return primitive;
        }

        public IMyNavigationGroup LowLevelGroup =>
            this.m_lowLevel;

        public MyHighLevelGroup HighLevelGroup =>
            null;
    }
}

