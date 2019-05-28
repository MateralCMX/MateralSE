namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender;

    public class MyRDPath : IMyPath
    {
        private MyRDPathfinding m_pathfinding;
        private IMyDestinationShape m_destination;
        private bool m_isValid;
        private bool m_pathCompleted;
        private List<Vector3D> m_pathPoints = new List<Vector3D>();
        private int m_currentPointIndex;
        private MyPlanet m_planet;

        public MyRDPath(MyRDPathfinding pathfinding, Vector3D begin, IMyDestinationShape destination)
        {
            this.m_pathfinding = pathfinding;
            this.m_destination = destination;
            this.m_isValid = true;
            this.m_currentPointIndex = 0;
            this.m_planet = this.GetClosestPlanet(begin);
        }

        public void DebugDraw()
        {
            if (this.m_pathPoints.Count > 0)
            {
                for (int i = 0; i < (this.m_pathPoints.Count - 1); i++)
                {
                    Vector3D pointTo = this.m_pathPoints[i + 1];
                    MyRenderProxy.DebugDrawLine3D(this.m_pathPoints[i], pointTo, Color.Blue, Color.Red, true, false);
                    MyRenderProxy.DebugDrawSphere(pointTo, 0.3f, Color.Yellow, 1f, true, false, true, false);
                }
            }
        }

        private MyPlanet GetClosestPlanet(Vector3D position)
        {
            int num = 200;
            BoundingBoxD box = new BoundingBoxD(position - (num * 0.5f), position + (num * 0.5f));
            return MyGamePruningStructure.GetClosestPlanet(ref box);
        }

        public bool GetNextTarget(Vector3D position, out Vector3D target, out float targetRadius, out IMyEntity relativeEntity)
        {
            target = Vector3D.Zero;
            relativeEntity = null;
            targetRadius = 0.8f;
            if (!this.m_isValid)
            {
                return false;
            }
            if (((this.m_pathPoints.Count == 0) || this.m_pathCompleted) || !this.m_isValid)
            {
                this.m_pathPoints = this.m_pathfinding.GetPath(this.m_planet, position, this.m_destination.GetDestination());
                if (this.m_pathPoints.Count < 2)
                {
                    return false;
                }
                this.m_currentPointIndex = 1;
            }
            int num1 = this.m_pathPoints.Count - 1;
            int currentPointIndex = this.m_currentPointIndex;
            target = this.m_pathPoints[this.m_currentPointIndex];
            if (Math.Abs(Vector3.Distance((Vector3) target, (Vector3) position)) < targetRadius)
            {
                if (this.m_currentPointIndex == (this.m_pathPoints.Count - 1))
                {
                    this.m_pathCompleted = true;
                    return false;
                }
                this.m_currentPointIndex++;
                target = this.m_pathPoints[this.m_currentPointIndex];
            }
            return true;
        }

        public void Invalidate()
        {
            this.m_isValid = false;
        }

        public void Reinit(Vector3D position)
        {
        }

        public IMyDestinationShape Destination =>
            this.m_destination;

        public IMyEntity EndEntity =>
            null;

        public bool IsValid =>
            this.m_isValid;

        public bool PathCompleted =>
            this.m_pathCompleted;
    }
}

