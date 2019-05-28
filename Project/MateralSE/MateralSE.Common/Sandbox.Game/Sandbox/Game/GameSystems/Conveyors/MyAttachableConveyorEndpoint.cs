namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Algorithms;

    public class MyAttachableConveyorEndpoint : MyMultilineConveyorEndpoint
    {
        private List<MyAttachableLine> m_lines;

        public MyAttachableConveyorEndpoint(MyCubeBlock block) : base(block)
        {
            this.m_lines = new List<MyAttachableLine>();
        }

        private void AddAttachableLine(MyAttachableLine line)
        {
            this.m_lines.Add(line);
        }

        public bool AlreadyAttached() => 
            (this.m_lines.Count != 0);

        public bool AlreadyAttachedTo(MyAttachableConveyorEndpoint other)
        {
            using (List<MyAttachableLine>.Enumerator enumerator = this.m_lines.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (ReferenceEquals(enumerator.Current.GetOtherVertex(this), other))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Attach(MyAttachableConveyorEndpoint other)
        {
            MyAttachableLine line = new MyAttachableLine(this, other);
            this.AddAttachableLine(line);
            other.AddAttachableLine(line);
        }

        public void Detach(MyAttachableConveyorEndpoint other)
        {
            for (int i = 0; i < this.m_lines.Count; i++)
            {
                MyAttachableLine line = this.m_lines[i];
                if (line.Contains(other))
                {
                    this.RemoveAttachableLine(line);
                    other.RemoveAttachableLine(line);
                    return;
                }
            }
        }

        public void DetachAll()
        {
            for (int i = 0; i < this.m_lines.Count; i++)
            {
                MyAttachableLine line = this.m_lines[i];
                (line.GetOtherVertex(this) as MyAttachableConveyorEndpoint).RemoveAttachableLine(line);
            }
            this.m_lines.Clear();
        }

        protected override IMyPathEdge<IMyConveyorEndpoint> GetEdge(int index)
        {
            int neighborCount = base.GetNeighborCount();
            return ((index >= neighborCount) ? ((IMyPathEdge<IMyConveyorEndpoint>) this.m_lines[index - neighborCount]) : base.GetEdge(index));
        }

        protected override IMyPathVertex<IMyConveyorEndpoint> GetNeighbor(int index)
        {
            int neighborCount = base.GetNeighborCount();
            return ((index >= neighborCount) ? this.m_lines[index - neighborCount].GetOtherVertex(this) : base.GetNeighbor(index));
        }

        protected override int GetNeighborCount() => 
            (base.GetNeighborCount() + this.m_lines.Count);

        private void RemoveAttachableLine(MyAttachableLine line)
        {
            this.m_lines.Remove(line);
        }

        private class MyAttachableLine : IMyPathEdge<IMyConveyorEndpoint>
        {
            private MyAttachableConveyorEndpoint m_endpoint1;
            private MyAttachableConveyorEndpoint m_endpoint2;

            public MyAttachableLine(MyAttachableConveyorEndpoint endpoint1, MyAttachableConveyorEndpoint endpoint2)
            {
                this.m_endpoint1 = endpoint1;
                this.m_endpoint2 = endpoint2;
            }

            public bool Contains(MyAttachableConveyorEndpoint endpoint) => 
                (ReferenceEquals(endpoint, this.m_endpoint1) || ReferenceEquals(endpoint, this.m_endpoint2));

            public IMyConveyorEndpoint GetOtherVertex(IMyConveyorEndpoint vertex1) => 
                (!ReferenceEquals(vertex1, this.m_endpoint1) ? this.m_endpoint1 : this.m_endpoint2);

            public float GetWeight() => 
                2f;
        }
    }
}

