namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRage.Algorithms;

    public class MyNavigationEdge : IMyPathEdge<MyNavigationPrimitive>
    {
        public static MyNavigationEdge Static = new MyNavigationEdge();
        private MyNavigationPrimitive m_triA;
        private MyNavigationPrimitive m_triB;
        private int m_index;

        public MyNavigationPrimitive GetOtherVertex(MyNavigationPrimitive vertex1) => 
            (!ReferenceEquals(vertex1, this.m_triA) ? this.m_triA : this.m_triB);

        public float GetWeight() => 
            ((this.m_triA.Position - this.m_triB.Position).Length() * 1f);

        public void Init(MyNavigationPrimitive triA, MyNavigationPrimitive triB, int index)
        {
            this.m_triA = triA;
            this.m_triB = triB;
            this.m_index = index;
        }

        public int Index =>
            this.m_index;
    }
}

