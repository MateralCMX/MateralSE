namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Algorithms;
    using VRageMath;

    public class MyHighLevelPrimitive : MyNavigationPrimitive
    {
        private MyHighLevelGroup m_parent;
        private List<int> m_neighbors;
        private int m_index;
        private Vector3 m_position;

        public MyHighLevelPrimitive(MyHighLevelGroup parent, int index, Vector3 position)
        {
            this.m_parent = parent;
            this.m_neighbors = new List<int>(4);
            this.m_index = index;
            this.m_position = position;
            this.IsExpanded = false;
        }

        public void Connect(int other)
        {
            this.m_neighbors.Add(other);
        }

        public void Disconnect(int other)
        {
            this.m_neighbors.Remove(other);
        }

        public IMyHighLevelComponent GetComponent() => 
            this.m_parent.LowLevelGroup.GetComponent(this);

        public override MyHighLevelPrimitive GetHighLevelPrimitive() => 
            null;

        public void GetNeighbours(List<int> output)
        {
            output.Clear();
            output.AddList<int>(this.m_neighbors);
        }

        public override IMyPathEdge<MyNavigationPrimitive> GetOwnEdge(int index)
        {
            MyNavigationEdge.Static.Init(this, this.GetOwnNeighbor(index) as MyNavigationPrimitive, 0);
            return MyNavigationEdge.Static;
        }

        public override IMyPathVertex<MyNavigationPrimitive> GetOwnNeighbor(int index) => 
            this.m_parent.GetPrimitive(this.m_neighbors[index]);

        public override int GetOwnNeighborCount() => 
            this.m_neighbors.Count;

        public override string ToString()
        {
            object[] objArray1 = new object[] { "(", this.m_parent.ToString(), ")[", this.m_index, "]" };
            return string.Concat(objArray1);
        }

        public void UpdatePosition(Vector3 position)
        {
            this.m_position = position;
        }

        public bool IsExpanded { get; set; }

        public int Index =>
            this.m_index;

        public override Vector3 Position =>
            this.m_position;

        public override Vector3D WorldPosition =>
            this.m_parent.LocalToGlobal(this.m_position);

        public MyHighLevelGroup Parent =>
            this.m_parent;

        public override IMyNavigationGroup Group =>
            this.m_parent;
    }
}

