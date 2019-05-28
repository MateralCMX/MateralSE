namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public class MyRenderMessageDebugDrawMesh : MyRenderMessageBase
    {
        public uint ID;
        public MatrixD WorldMatrix;
        public bool DepthRead;
        public bool Shaded;
        public List<MyFormatPositionColor> Vertices;

        public override void Close()
        {
            base.Close();
            this.Vertices = null;
        }

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawMesh;

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.DebugDraw;
    }
}

