namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MySmartPath : IMyHighLevelPrimitiveObserver, IMyPath
    {
        private MyPathfinding m_pathfinding;
        private int m_lastInitTime;
        private bool m_usedWholePath;
        private bool m_valid;
        private List<MyHighLevelPrimitive> m_pathNodes;
        private List<Vector4D> m_expandedPath;
        private int m_pathNodePosition;
        private int m_expandedPathPosition;
        private MyNavigationPrimitive m_currentPrimitive;
        private MyHighLevelPrimitive m_hlBegin;
        private Vector3D m_startPoint;
        private MySmartGoal m_goal;
        private static MySmartPath m_pathfindingStatic;
        private const float TRANSITION_RADIUS = 1f;

        public MySmartPath(MyPathfinding pathfinding)
        {
            this.m_pathfinding = pathfinding;
            this.m_pathNodes = new List<MyHighLevelPrimitive>();
            this.m_expandedPath = new List<Vector4D>();
        }

        private void ClearFirstPathNode()
        {
            using (List<MyHighLevelPrimitive>.Enumerator enumerator = this.m_pathNodes.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    MyHighLevelPrimitive current = enumerator.Current;
                    if (!ReferenceEquals(current, this.m_hlBegin))
                    {
                        current.Parent.StopObservingPrimitive(current, this);
                    }
                }
            }
            this.m_pathNodes.RemoveAt(0);
            this.m_pathNodePosition--;
        }

        private void ClearPathNodes()
        {
            foreach (MyHighLevelPrimitive primitive in this.m_pathNodes)
            {
                if (!ReferenceEquals(primitive, this.m_hlBegin))
                {
                    primitive.Parent.StopObservingPrimitive(primitive, this);
                }
            }
            this.m_pathNodes.Clear();
            this.m_pathNodePosition = 0;
        }

        public void DebugDraw()
        {
            MatrixD viewMatrix = MySector.MainCamera.ViewMatrix;
            Vector3D? nullable = null;
            foreach (MyHighLevelPrimitive local1 in this.m_pathNodes)
            {
                Vector3D down = MyGravityProviderSystem.CalculateTotalGravityInPoint(local1.WorldPosition);
                if (Vector3D.IsZero(down, 0.001))
                {
                    down = Vector3D.Down;
                }
                down.Normalize();
                MyHighLevelPrimitive local2 = local1;
                Vector3D position = local2.WorldPosition + (down * -10.0);
                MyRenderProxy.DebugDrawSphere(position, 1f, Color.IndianRed, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawLine3D(local2.WorldPosition, position, Color.IndianRed, Color.IndianRed, false, false);
                if (nullable != null)
                {
                    MyRenderProxy.DebugDrawLine3D(position, nullable.Value, Color.IndianRed, Color.IndianRed, false, false);
                }
                nullable = new Vector3D?(position);
            }
            MyRenderProxy.DebugDrawSphere(this.m_startPoint, 0.5f, Color.HotPink, 1f, false, false, true, false);
            if (this.m_goal != null)
            {
                this.m_goal.DebugDraw();
            }
            if (MyFakes.DEBUG_DRAW_FOUND_PATH)
            {
                Vector3D? nullable2 = null;
                for (int i = 0; i < this.m_expandedPath.Count; i++)
                {
                    Vector3D position = new Vector3D(this.m_expandedPath[i]);
                    float w = (float) this.m_expandedPath[i].W;
                    Color color = (i == (this.m_expandedPath.Count - 1)) ? Color.OrangeRed : Color.Orange;
                    MyRenderProxy.DebugDrawPoint(position, color, false, false);
                    MyRenderProxy.DebugDrawText3D(position + (viewMatrix.Right * 0.10000000149011612), w.ToString(), color, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    if (nullable2 != null)
                    {
                        MyRenderProxy.DebugDrawLine3D(nullable2.Value, position, Color.Pink, Color.Pink, false, false);
                    }
                    nullable2 = new Vector3D?(position);
                }
            }
        }

        private void ExpandPath(Vector3D currentPosition)
        {
            if (this.m_pathNodePosition >= (this.m_pathNodes.Count - 1))
            {
                this.GenerateHighLevelPath();
            }
            if (this.m_pathNodePosition < this.m_pathNodes.Count)
            {
                MyPath<MyNavigationPrimitive> path = null;
                bool flag = false;
                this.m_expandedPath.Clear();
                if ((this.m_pathNodePosition + 1) < this.m_pathNodes.Count)
                {
                    if (this.m_pathNodes[this.m_pathNodePosition].IsExpanded && this.m_pathNodes[this.m_pathNodePosition + 1].IsExpanded)
                    {
                        IMyHighLevelComponent component = this.m_pathNodes[this.m_pathNodePosition].GetComponent();
                        IMyHighLevelComponent otherComponent = this.m_pathNodes[this.m_pathNodePosition + 1].GetComponent();
                        path = this.m_pathfinding.FindPath(this.m_currentPrimitive, this.m_goal.PathfindingHeuristic, prim => otherComponent.Contains(prim) ? 0f : float.PositiveInfinity, prim => component.Contains(prim) || otherComponent.Contains(prim), true);
                    }
                }
                else if (this.m_pathNodes[this.m_pathNodePosition].IsExpanded)
                {
                    IMyHighLevelComponent component1 = this.m_pathNodes[this.m_pathNodePosition].GetComponent();
                    path = this.m_pathfinding.FindPath(this.m_currentPrimitive, this.m_goal.PathfindingHeuristic, prim => component1.Contains(prim) ? this.m_goal.TerminationCriterion(prim) : 30f, prim => component1.Contains(prim), true);
                    if (path != null)
                    {
                        if ((path.Count == 0) || !component1.Contains(path[path.Count - 1].Vertex as MyNavigationPrimitive))
                        {
                            this.m_goal.IgnoreHighLevel(this.m_pathNodes[this.m_pathNodePosition]);
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                }
                if ((path != null) && (path.Count != 0))
                {
                    Vector3D end = new Vector3D();
                    MyNavigationPrimitive vertex = path[path.Count - 1].Vertex as MyNavigationPrimitive;
                    if (!flag)
                    {
                        end = vertex.WorldPosition;
                    }
                    else
                    {
                        Vector3 bestPoint = (Vector3) this.m_goal.Destination.GetBestPoint(vertex.WorldPosition);
                        Vector3 localPos = vertex.ProjectLocalPoint(vertex.Group.GlobalToLocal(bestPoint));
                        end = vertex.Group.LocalToGlobal(localPos);
                    }
                    this.RefineFoundPath(ref currentPosition, ref end, path);
                    if ((((this.m_pathNodes.Count <= 1) & flag) && ((this.m_expandedPath.Count > 0) && (path.Count <= 2))) && !this.m_goal.ShouldReinitPath())
                    {
                        Vector4D vectord2 = this.m_expandedPath[this.m_expandedPath.Count - 1];
                        if (Vector3D.DistanceSquared(currentPosition, end) < ((vectord2.W * vectord2.W) / 256.0))
                        {
                            this.m_expandedPath.Clear();
                        }
                    }
                }
            }
        }

        private void GenerateHighLevelPath()
        {
            this.ClearPathNodes();
            if (this.m_hlBegin != null)
            {
                MyPath<MyNavigationPrimitive> path = this.m_goal.FindHighLevelPath(this.m_pathfinding, this.m_hlBegin);
                if (path != null)
                {
                    using (IEnumerator<MyPath<MyNavigationPrimitive>.PathNode> enumerator = path.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyHighLevelPrimitive vertex = enumerator.Current.Vertex as MyHighLevelPrimitive;
                            this.m_pathNodes.Add(vertex);
                            if (!ReferenceEquals(vertex, this.m_hlBegin))
                            {
                                vertex.Parent.ObservePrimitive(vertex, this);
                            }
                        }
                    }
                    this.m_pathNodePosition = 0;
                }
            }
        }

        public bool GetNextTarget(Vector3D currentPosition, out Vector3D targetWorld, out float radius, out IMyEntity relativeEntity)
        {
            bool flag = false;
            targetWorld = new Vector3D();
            radius = 1f;
            relativeEntity = null;
            if (this.m_pathNodePosition > 1)
            {
                this.ClearFirstPathNode();
            }
            if (this.m_expandedPathPosition >= this.m_expandedPath.Count)
            {
                if (!this.m_usedWholePath)
                {
                    flag = this.ShouldReinitPath();
                }
                if (flag)
                {
                    this.Reinit(currentPosition);
                }
                if (!this.IsValid)
                {
                    return false;
                }
                this.ExpandPath(currentPosition);
                if (this.m_expandedPath.Count == 0)
                {
                    return false;
                }
            }
            if (this.m_expandedPathPosition >= this.m_expandedPath.Count)
            {
                return false;
            }
            Vector4D xyz = this.m_expandedPath[this.m_expandedPathPosition];
            targetWorld = new Vector3D(xyz);
            radius = (float) xyz.W;
            this.m_expandedPathPosition++;
            if ((this.m_expandedPathPosition == this.m_expandedPath.Count) && (this.m_pathNodePosition >= (this.m_pathNodes.Count - 1)))
            {
                this.m_usedWholePath = true;
            }
            relativeEntity = null;
            return true;
        }

        public void Init(Vector3D start, MySmartGoal goal)
        {
            this.m_lastInitTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.m_startPoint = start;
            this.m_goal = goal;
            this.m_currentPrimitive = this.m_pathfinding.FindClosestPrimitive(start, false, null);
            if (this.m_currentPrimitive != null)
            {
                this.m_hlBegin = this.m_currentPrimitive.GetHighLevelPrimitive();
                if ((this.m_hlBegin != null) && !this.m_pathNodes.Contains(this.m_hlBegin))
                {
                    this.m_hlBegin.Parent.ObservePrimitive(this.m_hlBegin, this);
                }
            }
            if (this.m_currentPrimitive == null)
            {
                this.m_currentPrimitive = null;
                this.Invalidate();
            }
            else
            {
                this.m_pathNodePosition = 0;
                this.m_expandedPathPosition = 0;
                this.m_expandedPath.Clear();
                this.m_pathNodes.Clear();
                this.m_usedWholePath = false;
                this.m_valid = true;
            }
        }

        public void Invalidate()
        {
            if (this.m_valid)
            {
                this.ClearPathNodes();
                this.m_expandedPath.Clear();
                this.m_expandedPathPosition = 0;
                this.m_currentPrimitive = null;
                if (this.m_goal.IsValid)
                {
                    this.m_goal.Invalidate();
                }
                if (this.m_hlBegin != null)
                {
                    this.m_hlBegin.Parent.StopObservingPrimitive(this.m_hlBegin, this);
                }
                this.m_hlBegin = null;
                this.m_valid = false;
            }
        }

        private void RefineFoundPath(ref Vector3D begin, ref Vector3D end, MyPath<MyNavigationPrimitive> path)
        {
            if (MyPerGameSettings.EnablePathfinding && (path != null))
            {
                this.m_currentPrimitive = path[path.Count - 1].Vertex as MyNavigationPrimitive;
                if ((this.m_hlBegin != null) && !this.m_pathNodes.Contains(this.m_hlBegin))
                {
                    this.m_hlBegin.Parent.StopObservingPrimitive(this.m_hlBegin, this);
                }
                this.m_hlBegin = this.m_currentPrimitive.GetHighLevelPrimitive();
                if ((this.m_hlBegin != null) && !this.m_pathNodes.Contains(this.m_hlBegin))
                {
                    this.m_hlBegin.Parent.ObservePrimitive(this.m_hlBegin, this);
                }
                IMyNavigationGroup objB = null;
                int num = 0;
                int num2 = 0;
                Vector3 startPoint = new Vector3();
                Vector3 endPoint = new Vector3();
                int num3 = 0;
                while (true)
                {
                    while (true)
                    {
                        if (num3 >= path.Count)
                        {
                            this.m_pathNodePosition++;
                            this.m_expandedPathPosition = 0;
                            return;
                        }
                        MyNavigationPrimitive vertex = path[num3].Vertex as MyNavigationPrimitive;
                        IMyNavigationGroup group = vertex.Group;
                        if (objB == null)
                        {
                            objB = group;
                            startPoint = objB.GlobalToLocal(begin);
                        }
                        bool flag = num3 == (path.Count - 1);
                        if (!ReferenceEquals(group, objB))
                        {
                            num2 = num3 - 1;
                            endPoint = objB.GlobalToLocal(vertex.WorldPosition);
                        }
                        else
                        {
                            if (!flag)
                            {
                                break;
                            }
                            num2 = num3;
                            endPoint = objB.GlobalToLocal(end);
                        }
                        objB.RefinePath(path, this.m_expandedPath, ref startPoint, ref endPoint, num, num2);
                        int count = this.m_expandedPath.Count;
                        int num5 = this.m_expandedPath.Count;
                        while (true)
                        {
                            if (num5 >= count)
                            {
                                if (flag && !ReferenceEquals(group, objB))
                                {
                                    this.m_expandedPath.Add(new Vector4D(vertex.WorldPosition, this.m_expandedPath[count - 1].W));
                                }
                                objB = group;
                                num = num3;
                                if (this.m_expandedPath.Count != 0)
                                {
                                    startPoint = group.GlobalToLocal(new Vector3D(this.m_expandedPath[this.m_expandedPath.Count - 1]));
                                }
                                break;
                            }
                            Vector3D vectord = new Vector3D(this.m_expandedPath[num5]);
                            vectord = objB.LocalToGlobal((Vector3) vectord);
                            this.m_expandedPath[num5] = new Vector4D(vectord, this.m_expandedPath[num5].W);
                            num5++;
                        }
                        break;
                    }
                    num3++;
                }
            }
        }

        public void Reinit(Vector3D newStart)
        {
            MySmartGoal goal = this.m_goal;
            MyEntity endEntity = goal.EndEntity;
            this.ClearPathNodes();
            this.m_expandedPath.Clear();
            this.m_expandedPathPosition = 0;
            this.m_currentPrimitive = null;
            if (this.m_hlBegin != null)
            {
                this.m_hlBegin.Parent.StopObservingPrimitive(this.m_hlBegin, this);
            }
            this.m_hlBegin = null;
            this.m_valid = false;
            this.m_goal.Reinit();
            this.Init(newStart, goal);
        }

        private bool ShouldReinitPath() => 
            (((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastInitTime) >= 0x3e8) ? this.m_goal.ShouldReinitPath() : false);

        public IMyDestinationShape Destination =>
            this.m_goal.Destination;

        public IMyEntity EndEntity =>
            this.m_goal.EndEntity;

        public bool IsValid
        {
            get
            {
                if (!this.m_goal.IsValid)
                {
                    if (this.m_valid)
                    {
                        this.Invalidate();
                    }
                    return false;
                }
                if (this.m_valid)
                {
                    return true;
                }
                this.m_goal.Invalidate();
                return false;
            }
        }

        public bool PathCompleted =>
            this.m_usedWholePath;
    }
}

