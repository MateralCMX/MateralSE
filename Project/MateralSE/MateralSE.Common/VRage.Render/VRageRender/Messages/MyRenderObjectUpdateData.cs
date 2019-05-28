namespace VRageRender.Messages
{
    using System;

    public class MyRenderObjectUpdateData
    {
        public MatrixD? WorldMatrix;
        public Matrix? LocalMatrix;
        public BoundingBox? LocalAABB;
        private static int m_allocated;

        public void Clean()
        {
            this.LocalAABB = null;
            this.LocalMatrix = null;
            this.WorldMatrix = null;
        }
    }
}

