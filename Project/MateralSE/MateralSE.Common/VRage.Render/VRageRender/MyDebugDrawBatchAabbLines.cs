namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageRender.Messages;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDebugDrawBatchAabbLines : IMyDebugDrawBatchAabb, IDisposable
    {
        private MyRenderMessageDebugDrawLine3DBatch m_msg;
        private Color m_color;
        internal MyDebugDrawBatchAabbLines(MyRenderMessageDebugDrawLine3DBatch msg, ref MatrixD worldMatrix, Color color, bool depthRead)
        {
            this.m_msg = msg;
            this.m_msg.WorldMatrix = worldMatrix;
            this.m_msg.DepthRead = depthRead;
            this.m_color = color;
        }

        public void Add(ref BoundingBoxD aabb, Color? color = new Color?())
        {
            Color? nullable = color;
            Color colorFrom = (nullable != null) ? nullable.GetValueOrDefault() : this.m_color;
            Vector3D pointFrom = new Vector3D(aabb.Min.X, aabb.Min.Y, aabb.Min.Z);
            Vector3D pointTo = new Vector3D(aabb.Max.X, aabb.Min.Y, aabb.Min.Z);
            Vector3D vectord3 = new Vector3D(aabb.Min.X, aabb.Min.Y, aabb.Max.Z);
            Vector3D vectord4 = new Vector3D(aabb.Max.X, aabb.Min.Y, aabb.Max.Z);
            Vector3D vectord5 = new Vector3D(aabb.Min.X, aabb.Max.Y, aabb.Min.Z);
            Vector3D vectord6 = new Vector3D(aabb.Max.X, aabb.Max.Y, aabb.Min.Z);
            Vector3D vectord7 = new Vector3D(aabb.Min.X, aabb.Max.Y, aabb.Max.Z);
            Vector3D vectord8 = new Vector3D(aabb.Max.X, aabb.Max.Y, aabb.Max.Z);
            this.m_msg.AddLine(pointFrom, colorFrom, pointTo, colorFrom);
            this.m_msg.AddLine(vectord3, colorFrom, vectord4, colorFrom);
            this.m_msg.AddLine(vectord5, colorFrom, vectord6, colorFrom);
            this.m_msg.AddLine(vectord7, colorFrom, vectord8, colorFrom);
            this.m_msg.AddLine(pointFrom, colorFrom, vectord3, colorFrom);
            this.m_msg.AddLine(pointTo, colorFrom, vectord4, colorFrom);
            this.m_msg.AddLine(vectord5, colorFrom, vectord7, colorFrom);
            this.m_msg.AddLine(vectord6, colorFrom, vectord8, colorFrom);
            this.m_msg.AddLine(pointFrom, colorFrom, vectord5, colorFrom);
            this.m_msg.AddLine(pointTo, colorFrom, vectord6, colorFrom);
            this.m_msg.AddLine(vectord3, colorFrom, vectord7, colorFrom);
            this.m_msg.AddLine(vectord4, colorFrom, vectord8, colorFrom);
        }

        public void Dispose()
        {
            MyRenderProxy.DebugDrawLine3DSubmitBatch(this.m_msg);
        }
    }
}

