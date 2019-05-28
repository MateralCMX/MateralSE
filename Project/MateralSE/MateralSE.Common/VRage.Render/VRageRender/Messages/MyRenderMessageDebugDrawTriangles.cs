namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;
    using VRageMath;
    using VRageRender;

    public class MyRenderMessageDebugDrawTriangles : MyDebugRenderMessage, IDrawTrianglesMessage
    {
        public VRageMath.Color Color;
        public MatrixD WorldMatrix;
        public bool DepthRead;
        public bool Shaded;
        public bool Edges;
        public List<int> Indices = new List<int>();
        public List<MyFormatPositionColor> Vertices = new List<MyFormatPositionColor>();

        public void AddIndex(int index)
        {
            this.Indices.Add(index);
        }

        public void AddTriangle(ref Vector3D v0, ref Vector3D v1, ref Vector3D v2)
        {
            int count = this.Vertices.Count;
            this.Indices.Add(count);
            this.Indices.Add(count + 1);
            this.Indices.Add(count + 2);
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v0, this.Color));
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v1, this.Color));
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v2, this.Color));
        }

        public void AddTriangle(Vector3D v0, Vector3D v1, Vector3D v2)
        {
            int count = this.Vertices.Count;
            this.Indices.Add(count);
            this.Indices.Add(count + 1);
            this.Indices.Add(count + 2);
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v0, this.Color));
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v1, this.Color));
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v2, this.Color));
        }

        public void AddTriangle(Vector3D v0, VRageMath.Color c0, Vector3D v1, VRageMath.Color c1, Vector3D v2, VRageMath.Color c2)
        {
            int count = this.Vertices.Count;
            this.Indices.Add(count);
            this.Indices.Add(count + 1);
            this.Indices.Add(count + 2);
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v0, c0));
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v1, c1));
            this.Vertices.Add(new MyFormatPositionColor((Vector3) v2, c2));
        }

        public void AddVertex(Vector3D position)
        {
            this.Vertices.Add(new MyFormatPositionColor((Vector3) position, this.Color));
        }

        public void AddVertex(Vector3D position, VRageMath.Color color)
        {
            this.Vertices.Add(new MyFormatPositionColor((Vector3) position, color));
        }

        public int VertexCount =>
            this.Vertices.Count;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawTriangles;
    }
}

