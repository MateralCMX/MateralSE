namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using VRage.Algorithms;
    using VRageMath;

    public abstract class MyNavigationPrimitive : IMyPathVertex<MyNavigationPrimitive>, IEnumerable<IMyPathEdge<MyNavigationPrimitive>>, IEnumerable
    {
        private MyPathfindingData m_pathfindingData;
        private bool m_externalNeighbors;

        protected MyNavigationPrimitive()
        {
            this.m_pathfindingData = new MyPathfindingData(this);
        }

        public IEnumerator<IMyPathEdge<MyNavigationPrimitive>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public abstract MyHighLevelPrimitive GetHighLevelPrimitive();
        public abstract IMyPathEdge<MyNavigationPrimitive> GetOwnEdge(int index);
        public abstract IMyPathVertex<MyNavigationPrimitive> GetOwnNeighbor(int index);
        public abstract int GetOwnNeighborCount();
        public virtual Vector3 ProjectLocalPoint(Vector3 point) => 
            this.Position;

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        float IMyPathVertex<MyNavigationPrimitive>.EstimateDistanceTo(IMyPathVertex<MyNavigationPrimitive> other)
        {
            MyNavigationPrimitive primitive = other as MyNavigationPrimitive;
            return (!ReferenceEquals(this.Group, primitive.Group) ? ((float) Vector3D.Distance(this.WorldPosition, primitive.WorldPosition)) : Vector3.Distance(this.Position, primitive.Position));
        }

        IMyPathEdge<MyNavigationPrimitive> IMyPathVertex<MyNavigationPrimitive>.GetEdge(int index)
        {
            int ownNeighborCount = this.GetOwnNeighborCount();
            return ((index >= ownNeighborCount) ? this.Group.GetExternalEdge(this, index - ownNeighborCount) : this.GetOwnEdge(index));
        }

        IMyPathVertex<MyNavigationPrimitive> IMyPathVertex<MyNavigationPrimitive>.GetNeighbor(int index)
        {
            int ownNeighborCount = this.GetOwnNeighborCount();
            return ((index >= ownNeighborCount) ? this.Group.GetExternalNeighbor(this, index - ownNeighborCount) : this.GetOwnNeighbor(index));
        }

        int IMyPathVertex<MyNavigationPrimitive>.GetNeighborCount()
        {
            int ownNeighborCount = this.GetOwnNeighborCount();
            if (!this.m_externalNeighbors)
            {
                return ownNeighborCount;
            }
            return (ownNeighborCount + this.Group.GetExternalNeighborCount(this));
        }

        public MyPathfindingData PathfindingData =>
            this.m_pathfindingData;

        public bool HasExternalNeighbors
        {
            set => 
                (this.m_externalNeighbors = value);
        }

        public abstract Vector3 Position { get; }

        public abstract Vector3D WorldPosition { get; }

        public abstract IMyNavigationGroup Group { get; }
    }
}

