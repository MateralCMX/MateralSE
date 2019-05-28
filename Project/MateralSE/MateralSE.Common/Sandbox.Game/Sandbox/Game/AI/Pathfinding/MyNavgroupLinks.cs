namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRageMath;
    using VRageRender;

    public class MyNavgroupLinks
    {
        private Dictionary<MyNavigationPrimitive, List<MyNavigationPrimitive>> m_links = new Dictionary<MyNavigationPrimitive, List<MyNavigationPrimitive>>();

        public void AddLink(MyNavigationPrimitive primitive1, MyNavigationPrimitive primitive2, bool onlyIfNotPresent = false)
        {
            this.AddLinkInternal(primitive1, primitive2, onlyIfNotPresent);
            this.AddLinkInternal(primitive2, primitive1, onlyIfNotPresent);
            primitive1.HasExternalNeighbors = true;
            primitive2.HasExternalNeighbors = true;
        }

        private void AddLinkInternal(MyNavigationPrimitive primitive1, MyNavigationPrimitive primitive2, bool onlyIfNotPresent = false)
        {
            List<MyNavigationPrimitive> list = null;
            this.m_links.TryGetValue(primitive1, out list);
            if (list == null)
            {
                list = new List<MyNavigationPrimitive>();
                this.m_links.Add(primitive1, list);
            }
            if (!onlyIfNotPresent)
            {
                list.Add(primitive2);
            }
            else if (!list.Contains(primitive2))
            {
                list.Add(primitive2);
            }
        }

        public void DebugDraw(Color lineColor)
        {
            if (MyFakes.DEBUG_DRAW_NAVMESH_LINKS)
            {
                foreach (KeyValuePair<MyNavigationPrimitive, List<MyNavigationPrimitive>> pair in this.m_links)
                {
                    MyNavigationPrimitive key = pair.Key;
                    List<MyNavigationPrimitive> list = pair.Value;
                    for (int i = 0; i < list.Count; i++)
                    {
                        Vector3D worldPosition = key.WorldPosition;
                        Vector3D vectord2 = list[i].WorldPosition;
                        Vector3D vectord3 = (worldPosition + vectord2) * 0.5;
                        Vector3D vectord4 = (vectord3 + worldPosition) * 0.5;
                        Vector3D up = Vector3D.Up;
                        MyRenderProxy.DebugDrawLine3D(worldPosition, vectord4 + (up * 0.4), lineColor, lineColor, false, false);
                        MyRenderProxy.DebugDrawLine3D(vectord4 + (up * 0.4), vectord3 + (up * 0.5), lineColor, lineColor, false, false);
                    }
                }
            }
        }

        public IMyPathEdge<MyNavigationPrimitive> GetEdge(MyNavigationPrimitive primitive, int index)
        {
            List<MyNavigationPrimitive> list = null;
            this.m_links.TryGetValue(primitive, out list);
            if (list == null)
            {
                return null;
            }
            MyNavigationPrimitive primitive2 = list[index];
            return PathEdge.GetEdge(primitive, primitive2);
        }

        public int GetLinkCount(MyNavigationPrimitive primitive)
        {
            List<MyNavigationPrimitive> list = null;
            this.m_links.TryGetValue(primitive, out list);
            return ((list == null) ? 0 : list.Count);
        }

        public MyNavigationPrimitive GetLinkedNeighbor(MyNavigationPrimitive primitive, int index)
        {
            List<MyNavigationPrimitive> list = null;
            this.m_links.TryGetValue(primitive, out list);
            return list?[index];
        }

        public List<MyNavigationPrimitive> GetLinks(MyNavigationPrimitive primitive)
        {
            List<MyNavigationPrimitive> list = null;
            this.m_links.TryGetValue(primitive, out list);
            return list;
        }

        public void RemoveAllLinks(MyNavigationPrimitive primitive)
        {
            List<MyNavigationPrimitive> list = null;
            this.m_links.TryGetValue(primitive, out list);
            if (list != null)
            {
                using (List<MyNavigationPrimitive>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyNavigationPrimitive current = enumerator.Current;
                        List<MyNavigationPrimitive> list2 = null;
                        this.m_links.TryGetValue(current, out list2);
                        if (list2 != null)
                        {
                            list2.Remove(primitive);
                            if (list2.Count != 0)
                            {
                                continue;
                            }
                            this.m_links.Remove(current);
                            continue;
                        }
                        return;
                    }
                }
                this.m_links.Remove(primitive);
            }
        }

        public void RemoveLink(MyNavigationPrimitive primitive1, MyNavigationPrimitive primitive2)
        {
            if (this.RemoveLinkInternal(primitive1, primitive2))
            {
                primitive1.HasExternalNeighbors = false;
            }
            if (this.RemoveLinkInternal(primitive2, primitive1))
            {
                primitive2.HasExternalNeighbors = false;
            }
        }

        private bool RemoveLinkInternal(MyNavigationPrimitive primitive1, MyNavigationPrimitive primitive2)
        {
            List<MyNavigationPrimitive> list = null;
            this.m_links.TryGetValue(primitive1, out list);
            if (list != null)
            {
                list.Remove(primitive2);
                if (list.Count == 0)
                {
                    this.m_links.Remove(primitive1);
                    return true;
                }
            }
            return false;
        }

        private class PathEdge : IMyPathEdge<MyNavigationPrimitive>
        {
            private static MyNavgroupLinks.PathEdge Static = new MyNavgroupLinks.PathEdge();
            private MyNavigationPrimitive m_primitive1;
            private MyNavigationPrimitive m_primitive2;

            public static MyNavgroupLinks.PathEdge GetEdge(MyNavigationPrimitive primitive1, MyNavigationPrimitive primitive2)
            {
                Static.m_primitive1 = primitive1;
                Static.m_primitive2 = primitive2;
                return Static;
            }

            public MyNavigationPrimitive GetOtherVertex(MyNavigationPrimitive vertex1) => 
                (!ReferenceEquals(vertex1, this.m_primitive1) ? this.m_primitive1 : this.m_primitive2);

            public float GetWeight() => 
                (!ReferenceEquals(this.m_primitive1.Group, this.m_primitive2.Group) ? ((float) Vector3D.Distance(this.m_primitive1.WorldPosition, this.m_primitive2.WorldPosition)) : Vector3.Distance(this.m_primitive1.Position, this.m_primitive2.Position));
        }
    }
}

