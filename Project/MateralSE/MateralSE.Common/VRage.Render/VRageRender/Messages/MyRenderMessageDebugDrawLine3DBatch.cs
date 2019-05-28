namespace VRageRender.Messages
{
    using System;
    using System.Collections.Generic;
    using VRageMath;
    using VRageRender;

    public class MyRenderMessageDebugDrawLine3DBatch : MyDebugRenderMessage, IDisposable
    {
        public MatrixD WorldMatrix;
        public List<MyFormatPositionColor> Lines = new List<MyFormatPositionColor>();
        public bool DepthRead;

        public void AddLine(Vector3D pointFrom, Color colorFrom, Vector3D pointTo, Color colorTo)
        {
            MyFormatPositionColor item = new MyFormatPositionColor {
                Color = colorFrom,
                Position = (Vector3) pointFrom
            };
            this.Lines.Add(item);
            item = new MyFormatPositionColor {
                Color = colorTo,
                Position = (Vector3) pointTo
            };
            this.Lines.Add(item);
        }

        public override void Close()
        {
            this.Lines.Clear();
        }

        public void Dispose()
        {
            MyRenderProxy.DebugDrawLine3DSubmitBatch(this);
        }

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.DebugDrawLine3DBatch;
    }
}

