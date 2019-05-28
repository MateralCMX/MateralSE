namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageRender.Messages;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDebugDrawBatchAabbShaded : IMyDebugDrawBatchAabb, IDisposable
    {
        private MyRenderMessageDebugDrawTriangles m_msg;
        private MatrixD m_worldMatrix;
        private bool m_depthRead;
        internal MyDebugDrawBatchAabbShaded(MyRenderMessageDebugDrawTriangles msg, ref MatrixD worldMatrix, Color color, bool depthRead)
        {
            this.m_msg = msg;
            this.m_worldMatrix = worldMatrix;
            this.m_msg.Color = color;
            this.m_depthRead = depthRead;
        }

        public void Add(ref BoundingBoxD aabb, Color? color = new Color?())
        {
            Color? nullable = color;
            if (nullable == null)
            {
                Color color1 = this.m_msg.Color;
            }
            else
            {
                nullable.GetValueOrDefault();
            }
            int vertexCount = this.m_msg.VertexCount;
            this.m_msg.AddVertex(new Vector3D(aabb.Min.X, aabb.Min.Y, aabb.Min.Z));
            this.m_msg.AddVertex(new Vector3D(aabb.Max.X, aabb.Min.Y, aabb.Min.Z));
            this.m_msg.AddVertex(new Vector3D(aabb.Min.X, aabb.Min.Y, aabb.Max.Z));
            this.m_msg.AddVertex(new Vector3D(aabb.Max.X, aabb.Min.Y, aabb.Max.Z));
            this.m_msg.AddVertex(new Vector3D(aabb.Min.X, aabb.Max.Y, aabb.Min.Z));
            this.m_msg.AddVertex(new Vector3D(aabb.Max.X, aabb.Max.Y, aabb.Min.Z));
            this.m_msg.AddVertex(new Vector3D(aabb.Min.X, aabb.Max.Y, aabb.Max.Z));
            this.m_msg.AddVertex(new Vector3D(aabb.Max.X, aabb.Max.Y, aabb.Max.Z));
            this.m_msg.AddIndex(vertexCount + 1);
            this.m_msg.AddIndex(vertexCount);
            this.m_msg.AddIndex(vertexCount + 2);
            this.m_msg.AddIndex(vertexCount + 1);
            this.m_msg.AddIndex(vertexCount + 2);
            this.m_msg.AddIndex(vertexCount + 3);
            this.m_msg.AddIndex(vertexCount + 4);
            this.m_msg.AddIndex(vertexCount + 5);
            this.m_msg.AddIndex(vertexCount + 6);
            this.m_msg.AddIndex(vertexCount + 6);
            this.m_msg.AddIndex(vertexCount + 5);
            this.m_msg.AddIndex(vertexCount + 7);
            this.m_msg.AddIndex(vertexCount);
            this.m_msg.AddIndex(vertexCount + 1);
            this.m_msg.AddIndex(vertexCount + 4);
            this.m_msg.AddIndex(vertexCount + 4);
            this.m_msg.AddIndex(vertexCount + 1);
            this.m_msg.AddIndex(vertexCount + 5);
            this.m_msg.AddIndex(vertexCount + 3);
            this.m_msg.AddIndex(vertexCount + 2);
            this.m_msg.AddIndex(vertexCount + 6);
            this.m_msg.AddIndex(vertexCount + 3);
            this.m_msg.AddIndex(vertexCount + 6);
            this.m_msg.AddIndex(vertexCount + 7);
            this.m_msg.AddIndex(vertexCount + 1);
            this.m_msg.AddIndex(vertexCount + 3);
            this.m_msg.AddIndex(vertexCount + 5);
            this.m_msg.AddIndex(vertexCount + 5);
            this.m_msg.AddIndex(vertexCount + 3);
            this.m_msg.AddIndex(vertexCount + 7);
            this.m_msg.AddIndex(vertexCount + 4);
            this.m_msg.AddIndex(vertexCount + 2);
            this.m_msg.AddIndex(vertexCount);
            this.m_msg.AddIndex(vertexCount + 4);
            this.m_msg.AddIndex(vertexCount + 6);
            this.m_msg.AddIndex(vertexCount + 2);
        }

        public void Dispose()
        {
            MyRenderProxy.DebugDrawTriangles(this.m_msg, new MatrixD?(this.m_worldMatrix), this.m_depthRead, true, false, false);
        }
    }
}

