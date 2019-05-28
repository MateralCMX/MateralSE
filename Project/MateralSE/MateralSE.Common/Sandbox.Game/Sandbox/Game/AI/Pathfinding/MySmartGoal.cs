namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender;

    public class MySmartGoal : IMyHighLevelPrimitiveObserver
    {
        private MyNavigationPrimitive m_end;
        private MyHighLevelPrimitive m_hlEnd;
        private MyEntity m_endEntity;
        private bool m_hlEndIsApproximate;
        private IMyDestinationShape m_destination;
        private Vector3D m_destinationCenter;
        private Func<MyNavigationPrimitive, float> m_pathfindingHeuristic;
        private Func<MyNavigationPrimitive, float> m_terminationCriterion;
        private static Func<MyNavigationPrimitive, float> m_hlPathfindingHeuristic = new Func<MyNavigationPrimitive, float>(MySmartGoal.HlHeuristic);
        private static Func<MyNavigationPrimitive, float> m_hlTerminationCriterion = new Func<MyNavigationPrimitive, float>(MySmartGoal.HlCriterion);
        private static MySmartGoal m_pathfindingStatic = null;
        private HashSet<MyHighLevelPrimitive> m_ignoredPrimitives;

        public MySmartGoal(IMyDestinationShape goal, MyEntity entity = null)
        {
            this.m_destination = goal;
            this.m_destinationCenter = goal.GetDestination();
            this.m_endEntity = entity;
            if (this.m_endEntity != null)
            {
                this.m_destination.SetRelativeTransform(this.m_endEntity.PositionComp.WorldMatrixNormalizedInv);
                this.m_endEntity.OnClosing += new Action<MyEntity>(this.m_endEntity_OnClosing);
            }
            this.m_pathfindingHeuristic = new Func<MyNavigationPrimitive, float>(this.Heuristic);
            this.m_terminationCriterion = new Func<MyNavigationPrimitive, float>(this.Criterion);
            this.m_ignoredPrimitives = new HashSet<MyHighLevelPrimitive>();
            this.IsValid = true;
        }

        private float Criterion(MyNavigationPrimitive primitive) => 
            this.m_destination.PointAdmissibility(primitive.WorldPosition, 2f);

        public void DebugDraw()
        {
            this.m_destination.DebugDraw();
            using (HashSet<MyHighLevelPrimitive>.Enumerator enumerator = this.m_ignoredPrimitives.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyRenderProxy.DebugDrawSphere(enumerator.Current.WorldPosition, 0.5f, Color.Red, 1f, false, false, true, false);
                }
            }
        }

        public MyPath<MyNavigationPrimitive> FindHighLevelPath(MyPathfinding pathfinding, MyHighLevelPrimitive startPrimitive)
        {
            m_pathfindingStatic = this;
            pathfinding.LastHighLevelTimestamp = pathfinding.GetCurrentTimestamp();
            m_pathfindingStatic = null;
            return pathfinding.FindPath(startPrimitive, m_hlPathfindingHeuristic, m_hlTerminationCriterion, null, false);
        }

        public MyPath<MyNavigationPrimitive> FindPath(MyPathfinding pathfinding, MyNavigationPrimitive startPrimitive)
        {
            throw new NotImplementedException();
        }

        private float Heuristic(MyNavigationPrimitive primitive) => 
            ((float) Vector3D.Distance(primitive.WorldPosition, this.m_destinationCenter));

        private static float HlCriterion(MyNavigationPrimitive primitive)
        {
            MyHighLevelPrimitive item = primitive as MyHighLevelPrimitive;
            if ((item == null) || m_pathfindingStatic.m_ignoredPrimitives.Contains(item))
            {
                return float.PositiveInfinity;
            }
            float num = m_pathfindingStatic.m_destination.PointAdmissibility(primitive.WorldPosition, 8.7f);
            if (num < float.PositiveInfinity)
            {
                return (num * 4f);
            }
            IMyHighLevelComponent component = item.GetComponent();
            return ((component != null) ? (component.FullyExplored ? float.PositiveInfinity : (((float) Vector3D.RectangularDistance(primitive.WorldPosition, m_pathfindingStatic.m_destinationCenter)) * 8f)) : float.PositiveInfinity);
        }

        private static float HlHeuristic(MyNavigationPrimitive primitive) => 
            (((float) Vector3D.RectangularDistance(primitive.WorldPosition, m_pathfindingStatic.m_destinationCenter)) * 2f);

        public void IgnoreHighLevel(MyHighLevelPrimitive primitive)
        {
            if (!this.m_ignoredPrimitives.Contains(primitive))
            {
                primitive.Parent.ObservePrimitive(primitive, this);
                this.m_ignoredPrimitives.Add(primitive);
            }
        }

        public void Invalidate()
        {
            if (this.m_endEntity != null)
            {
                this.m_endEntity.OnClosing -= new Action<MyEntity>(this.m_endEntity_OnClosing);
                this.m_endEntity = null;
            }
            foreach (MyHighLevelPrimitive primitive in this.m_ignoredPrimitives)
            {
                primitive.Parent.StopObservingPrimitive(primitive, this);
            }
            this.m_ignoredPrimitives.Clear();
            this.IsValid = false;
        }

        private void m_endEntity_OnClosing(MyEntity obj)
        {
            this.m_endEntity = null;
            this.IsValid = false;
        }

        public void Reinit()
        {
            if (this.m_endEntity != null)
            {
                this.m_destination.UpdateWorldTransform(this.m_endEntity.WorldMatrix);
                this.m_destinationCenter = this.m_destination.GetDestination();
            }
        }

        public bool ShouldReinitPath() => 
            this.TargetMoved();

        private bool TargetMoved() => 
            (Vector3D.DistanceSquared(this.m_destinationCenter, this.m_destination.GetDestination()) > 4.0);

        public IMyDestinationShape Destination =>
            this.m_destination;

        public MyEntity EndEntity =>
            this.m_endEntity;

        public Func<MyNavigationPrimitive, float> PathfindingHeuristic =>
            this.m_pathfindingHeuristic;

        public Func<MyNavigationPrimitive, float> TerminationCriterion =>
            this.m_terminationCriterion;

        public bool IsValid { get; private set; }
    }
}

