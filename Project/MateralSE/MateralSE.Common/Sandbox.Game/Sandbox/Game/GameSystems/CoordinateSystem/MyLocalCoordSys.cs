namespace Sandbox.Game.GameSystems.CoordinateSystem
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyLocalCoordSys
    {
        private static readonly MyStringId ID_SQUARE = MyStringId.GetOrCompute("Square");
        private const float COLOR_ALPHA = 0.4f;
        private const int LOCAL_COORD_SIZE = 0x3e8;
        private const float BBOX_BORDER_THICKNESS_MODIF = 0.0015f;
        private MyTransformD m_origin;
        private MyOrientedBoundingBoxD m_boundingBox;
        private Vector3D[] m_corners;
        internal Color DebugColor;

        public MyLocalCoordSys(int size = 0x3e8)
        {
            this.m_corners = new Vector3D[8];
            this.m_origin = new MyTransformD(MatrixD.Identity);
            float x = ((float) size) / 2f;
            Vector3 max = new Vector3(x, x, x);
            BoundingBoxD box = new BoundingBoxD(-max, max);
            this.m_boundingBox = new MyOrientedBoundingBoxD(box, this.m_origin.TransformMatrix);
            this.m_boundingBox.GetCorners(this.m_corners, 0);
            this.RenderColor = this.GenerateRandomColor();
            this.DebugColor = this.GenerateDebugColor(this.RenderColor);
        }

        public MyLocalCoordSys(MyTransformD origin, int size = 0x3e8)
        {
            this.m_corners = new Vector3D[8];
            this.m_origin = origin;
            Vector3 max = new Vector3((float) (size / 2), (float) (size / 2), (float) (size / 2));
            BoundingBoxD box = new BoundingBoxD(-max, max);
            this.m_boundingBox = new MyOrientedBoundingBoxD(box, this.m_origin.TransformMatrix);
            this.m_boundingBox.GetCorners(this.m_corners, 0);
            this.RenderColor = this.GenerateRandomColor();
            this.DebugColor = this.GenerateDebugColor(this.RenderColor);
        }

        public bool Contains(ref Vector3D vec) => 
            this.m_boundingBox.Contains(ref vec);

        public void Draw()
        {
            MatrixD transformMatrix = this.Origin.TransformMatrix;
            Vector3D one = Vector3D.One;
            Vector3D zero = Vector3D.Zero;
            for (int i = 0; i < 8; i++)
            {
                Vector3D vectord4 = MySector.MainCamera.WorldToScreen(ref this.m_corners[i]);
                one = Vector3D.Min(one, vectord4);
                zero = Vector3D.Max(zero, vectord4);
            }
            Vector3D vectord3 = zero - one;
            float lineWidth = 0.0015f / ((float) MathHelper.Clamp(vectord3.Length(), 0.01, 1.0));
            Color color = MyFakes.ENABLE_DEBUG_DRAW_COORD_SYS ? this.DebugColor : this.RenderColor;
            BoundingBoxD localbox = new BoundingBoxD(-this.m_boundingBox.HalfExtent, this.m_boundingBox.HalfExtent);
            MySimpleObjectDraw.DrawTransparentBox(ref transformMatrix, ref localbox, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 1, lineWidth, new MyStringId?(ID_SQUARE), new MyStringId?(ID_SQUARE), false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
            if (MyFakes.ENABLE_DEBUG_DRAW_COORD_SYS)
            {
                Vector3D vectord5 = transformMatrix.Translation - MySector.MainCamera.Position;
                MyRenderProxy.DebugDrawText3D(this.Origin.Position, $"LCS Id:{this.Id} Distance:{vectord5.Length():###.00}m", color, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                int num3 = -10;
                while (true)
                {
                    if (num3 >= 11)
                    {
                        for (int j = -10; j < 11; j++)
                        {
                            Vector3D vectord7 = (this.Origin.Position - (transformMatrix.Right * 20.0)) + (transformMatrix.Forward * (j * 2.5));
                            MyRenderProxy.DebugDrawLine3D((this.Origin.Position + (transformMatrix.Right * 20.0)) + (transformMatrix.Forward * (j * 2.5)), vectord7, color, color, false, false);
                        }
                        break;
                    }
                    Vector3D pointTo = (this.Origin.Position - (transformMatrix.Forward * 20.0)) + (transformMatrix.Right * (num3 * 2.5));
                    MyRenderProxy.DebugDrawLine3D((this.Origin.Position + (transformMatrix.Forward * 20.0)) + (transformMatrix.Right * (num3 * 2.5)), pointTo, color, color, false, false);
                    num3++;
                }
            }
        }

        private Color GenerateDebugColor(Color original)
        {
            Vector3 hSV = new Color(original, 1f).ColorToHSV();
            hSV.Y = 0.8f;
            hSV.Z = 0.8f;
            return hSV.HSVtoColor();
        }

        private Color GenerateRandomColor() => 
            new Vector4((((float) MyRandom.Instance.Next(0, 100)) / 100f) * 0.4f, (((float) MyRandom.Instance.Next(0, 100)) / 100f) * 0.4f, (((float) MyRandom.Instance.Next(0, 100)) / 100f) * 0.4f, 0.4f);

        public MyTransformD Origin =>
            this.m_origin;

        public long EntityCounter { get; set; }

        internal MyOrientedBoundingBoxD BoundingBox =>
            this.m_boundingBox;

        public Color RenderColor { get; set; }

        public long Id { get; set; }
    }
}

