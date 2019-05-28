namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRageMath;
    using VRageRender.Utils;

    public class MyNavigationTriangle : MyNavigationPrimitive
    {
        private MyNavigationMesh m_navMesh;
        private int m_triIndex;
        public bool Registered;

        public void FindDangerousVertices(List<int> output)
        {
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = this.m_navMesh.Mesh.GetFace(this.m_triIndex).GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            int currentIndex = vertexEnumerator.CurrentIndex;
            vertexEnumerator.MoveNext();
            int num2 = vertexEnumerator.CurrentIndex;
            vertexEnumerator.MoveNext();
            int num3 = vertexEnumerator.CurrentIndex;
        }

        public int GetEdgeIndex(int index)
        {
            MyWingedEdgeMesh.FaceEdgeEnumerator enumerator = new MyWingedEdgeMesh.FaceEdgeEnumerator(this.m_navMesh.Mesh, this.m_triIndex);
            enumerator.MoveNext();
            while (index != 0)
            {
                enumerator.MoveNext();
                index--;
            }
            return enumerator.Current;
        }

        public void GetEdgeVertices(int index, out Vector3 pred, out Vector3 succ)
        {
            MyWingedEdgeMesh mesh = this.m_navMesh.Mesh;
            MyWingedEdgeMesh.Edge edge = mesh.GetEdge(this.GetEdgeIndex(index));
            pred = mesh.GetVertexPosition(edge.GetFacePredVertex(this.m_triIndex));
            succ = mesh.GetVertexPosition(edge.GetFaceSuccVertex(this.m_triIndex));
        }

        public override MyHighLevelPrimitive GetHighLevelPrimitive() => 
            this.m_navMesh.GetHighLevelPrimitive(this);

        public MyNavigationEdge GetNavigationEdge(int index)
        {
            MyWingedEdgeMesh mesh = this.m_navMesh.Mesh;
            int edgeIndex = this.GetEdgeIndex(index);
            MyWingedEdgeMesh.Edge edge = mesh.GetEdge(edgeIndex);
            MyNavigationTriangle triA = null;
            MyNavigationTriangle triB = null;
            if (edge.LeftFace != -1)
            {
                triA = mesh.GetFace(edge.LeftFace).GetUserData<MyNavigationTriangle>();
            }
            if (edge.RightFace != -1)
            {
                triB = mesh.GetFace(edge.RightFace).GetUserData<MyNavigationTriangle>();
            }
            MyNavigationEdge.Static.Init(triA, triB, edgeIndex);
            return MyNavigationEdge.Static;
        }

        public override IMyPathEdge<MyNavigationPrimitive> GetOwnEdge(int index) => 
            this.GetNavigationEdge(index);

        public override IMyPathVertex<MyNavigationPrimitive> GetOwnNeighbor(int index)
        {
            int edgeIndex = this.GetEdgeIndex(index);
            int faceIndex = this.m_navMesh.Mesh.GetEdge(edgeIndex).OtherFace(this.m_triIndex);
            if (faceIndex != -1)
            {
                return this.m_navMesh.Mesh.GetFace(faceIndex).GetUserData<MyNavigationPrimitive>();
            }
            return null;
        }

        public override int GetOwnNeighborCount() => 
            3;

        public void GetTransformed(ref MatrixI tform, out Vector3 newA, out Vector3 newB, out Vector3 newC)
        {
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = this.m_navMesh.Mesh.GetFace(this.m_triIndex).GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            newA = vertexEnumerator.Current;
            Vector3.Transform(ref newA, ref tform, out newA);
            vertexEnumerator.MoveNext();
            newB = vertexEnumerator.Current;
            Vector3.Transform(ref newB, ref tform, out newB);
            vertexEnumerator.MoveNext();
            newC = vertexEnumerator.Current;
            Vector3.Transform(ref newC, ref tform, out newC);
        }

        public MyWingedEdgeMesh.FaceVertexEnumerator GetVertexEnumerator() => 
            this.m_navMesh.Mesh.GetFace(this.m_triIndex).GetVertexEnumerator();

        public void GetVertices(out Vector3 a, out Vector3 b, out Vector3 c)
        {
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = this.m_navMesh.Mesh.GetFace(this.m_triIndex).GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            a = vertexEnumerator.Current;
            vertexEnumerator.MoveNext();
            b = vertexEnumerator.Current;
            vertexEnumerator.MoveNext();
            c = vertexEnumerator.Current;
        }

        public void GetVertices(out int indA, out int indB, out int indC, out Vector3 a, out Vector3 b, out Vector3 c)
        {
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = this.m_navMesh.Mesh.GetFace(this.m_triIndex).GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            indA = vertexEnumerator.CurrentIndex;
            a = vertexEnumerator.Current;
            vertexEnumerator.MoveNext();
            indB = vertexEnumerator.CurrentIndex;
            b = vertexEnumerator.Current;
            vertexEnumerator.MoveNext();
            indC = vertexEnumerator.CurrentIndex;
            c = vertexEnumerator.Current;
        }

        public void Init(MyNavigationMesh mesh, int triangleIndex)
        {
            this.m_navMesh = mesh;
            this.m_triIndex = triangleIndex;
            this.ComponentIndex = -1;
            this.Registered = false;
            base.HasExternalNeighbors = false;
        }

        public bool IsEdgeVertexDangerous(int index, bool predVertex)
        {
            MyWingedEdgeMesh mesh = this.m_navMesh.Mesh;
            int edgeIndex = this.GetEdgeIndex(index);
            MyWingedEdgeMesh.Edge edge = mesh.GetEdge(edgeIndex);
            int vertexIndex = predVertex ? edge.GetFacePredVertex(this.m_triIndex) : edge.GetFaceSuccVertex(this.m_triIndex);
            while (!IsTriangleDangerous(edge.VertexLeftFace(vertexIndex)))
            {
                int nextVertexEdge = edge.GetNextVertexEdge(vertexIndex);
                edge = mesh.GetEdge(nextVertexEdge);
                if (nextVertexEdge == edgeIndex)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsTriangleDangerous(int triIndex) => 
            (triIndex == -1);

        public override Vector3 ProjectLocalPoint(Vector3 point)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            Vector3 vector4;
            Vector3 vector5;
            Vector3 vector6;
            Vector3 vector7;
            Vector3 vector8;
            Vector3 vector9;
            this.GetVertices(out vector, out vector2, out vector3);
            Vector3.Subtract(ref vector2, ref vector, out vector4);
            Vector3.Subtract(ref vector3, ref vector, out vector5);
            Vector3.Subtract(ref point, ref vector, out vector6);
            Vector3.Cross(ref vector4, ref vector5, out vector7);
            Vector3.Cross(ref vector4, ref vector6, out vector8);
            Vector3.Cross(ref vector6, ref vector5, out vector9);
            float num = 1f / vector7.LengthSquared();
            float num2 = Vector3.Dot(vector9, vector7) * num;
            float num3 = Vector3.Dot(vector8, vector7) * num;
            float num4 = (1f - num2) - num3;
            if (num4 < 0f)
            {
                if (num2 < 0f)
                {
                    return vector3;
                }
                if (num3 < 0f)
                {
                    return vector2;
                }
                float num5 = 1f / (1f - num4);
                num4 = 0f;
                num2 *= num5;
                num3 *= num5;
            }
            else if (num2 >= 0f)
            {
                if (num3 < 0f)
                {
                    float num7 = 1f / (1f - num3);
                    num3 = 0f;
                    num4 *= num7;
                    num2 *= num7;
                }
            }
            else
            {
                if (num3 < 0f)
                {
                    return vector;
                }
                float num6 = 1f / (1f - num2);
                num2 = 0f;
                num4 *= num6;
                num3 *= num6;
            }
            return (((vector * num4) + (vector2 * num2)) + (vector3 * num3));
        }

        public override string ToString() => 
            (this.m_navMesh.ToString() + "; Tri: " + this.Index);

        public MyNavigationMesh Parent =>
            this.m_navMesh;

        public int Index =>
            this.m_triIndex;

        public int ComponentIndex { get; set; }

        public Vector3 Center
        {
            get
            {
                int num = 0;
                Vector3 zero = Vector3.Zero;
                MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = this.m_navMesh.Mesh.GetFace(this.m_triIndex).GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    zero += vertexEnumerator.Current;
                    num++;
                }
                return (zero / ((float) num));
            }
        }

        public Vector3 Normal
        {
            get
            {
                MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = this.m_navMesh.Mesh.GetFace(this.m_triIndex).GetVertexEnumerator();
                vertexEnumerator.MoveNext();
                Vector3 current = vertexEnumerator.Current;
                vertexEnumerator.MoveNext();
                vertexEnumerator.MoveNext();
                Vector3 vector3 = (vertexEnumerator.Current - current).Cross(vertexEnumerator.Current - current);
                vector3.Normalize();
                return vector3;
            }
        }

        public override Vector3 Position =>
            this.Center;

        public override Vector3D WorldPosition
        {
            get
            {
                Vector3D vectord2;
                MatrixD worldMatrix = this.m_navMesh.GetWorldMatrix();
                Vector3D.Transform(ref (ref Vector3D) ref this.Center, ref worldMatrix, out vectord2);
                return vectord2;
            }
        }

        public override IMyNavigationGroup Group =>
            this.m_navMesh;
    }
}

