namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyLayoutTable
    {
        private IMyGuiControlsParent m_parent;
        private Vector2 m_parentTopLeft;
        private Vector2 m_size;
        private float[] m_prefixScanX;
        private float[] m_prefixScanY;
        private const float BORDER = 0.005f;
        public unsafe Vector2 GetCellSize(int row, int col, int colSpan = 1, int rowSpan = 1)
        {
            Vector2 vector = new Vector2(this.m_prefixScanX[col], this.m_prefixScanY[row]);
            Vector2 vector2 = new Vector2(this.m_prefixScanX[col + colSpan], this.m_prefixScanY[row + rowSpan]) - vector;
            float* singlePtr1 = (float*) ref vector2.X;
            singlePtr1[0] -= 0.01f;
            float* singlePtr2 = (float*) ref vector2.Y;
            singlePtr2[0] -= 0.01f;
            return vector2;
        }

        public int LastRow =>
            ((this.m_prefixScanY != null) ? (this.m_prefixScanY.Length - 2) : 0);
        public int LastColumn =>
            ((this.m_prefixScanX != null) ? (this.m_prefixScanX.Length - 2) : 0);
        public MyLayoutTable(IMyGuiControlsParent parent)
        {
            this.m_parent = parent;
            Vector2? size = this.m_parent.GetSize();
            this.m_size = (size != null) ? size.GetValueOrDefault() : Vector2.One;
            this.m_parentTopLeft = (Vector2) (-0.5f * this.m_size);
            this.m_prefixScanX = null;
            this.m_prefixScanY = null;
        }

        public MyLayoutTable(IMyGuiControlsParent parent, Vector2 topLeft, Vector2 size)
        {
            this.m_parent = parent;
            this.m_parentTopLeft = topLeft;
            this.m_size = size;
            this.m_prefixScanX = null;
            this.m_prefixScanY = null;
        }

        public void SetColumnWidths(params float[] widthsPx)
        {
            this.m_prefixScanX = new float[widthsPx.Length + 1];
            this.m_prefixScanX[0] = this.m_parentTopLeft.X;
            float x = MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            for (int i = 0; i < widthsPx.Length; i++)
            {
                float num3 = widthsPx[i] / x;
                this.m_prefixScanX[i + 1] = this.m_prefixScanX[i] + num3;
            }
        }

        public unsafe void SetColumnWidthsNormalized(params float[] widthsPx)
        {
            float x = this.m_size.X;
            float num2 = 0f;
            for (int i = 0; i < widthsPx.Length; i++)
            {
                num2 += widthsPx[i];
            }
            for (int j = 0; j < widthsPx.Length; j++)
            {
                float* singlePtr1 = (float*) ref widthsPx[j];
                singlePtr1[0] *= (MyGuiConstants.GUI_OPTIMAL_SIZE.X / num2) * x;
            }
            this.SetColumnWidths(widthsPx);
        }

        public void SetRowHeights(params float[] heightsPx)
        {
            this.m_prefixScanY = new float[heightsPx.Length + 1];
            this.m_prefixScanY[0] = this.m_parentTopLeft.Y;
            float y = MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            for (int i = 0; i < heightsPx.Length; i++)
            {
                float num3 = heightsPx[i] / y;
                this.m_prefixScanY[i + 1] = this.m_prefixScanY[i] + num3;
            }
        }

        public unsafe void SetRowHeightsNormalized(params float[] heightsPx)
        {
            float y = this.m_size.Y;
            float num2 = 0f;
            for (int i = 0; i < heightsPx.Length; i++)
            {
                num2 += heightsPx[i];
            }
            for (int j = 0; j < heightsPx.Length; j++)
            {
                float* singlePtr1 = (float*) ref heightsPx[j];
                singlePtr1[0] *= (MyGuiConstants.GUI_OPTIMAL_SIZE.Y / num2) * y;
            }
            this.SetRowHeights(heightsPx);
        }

        public void Add(MyGuiControlBase control, MyAlignH alignH, MyAlignV alignV, int row, int col, int rowSpan = 1, int colSpan = 1)
        {
            Vector2 vector = new Vector2(this.m_prefixScanX[col], this.m_prefixScanY[row]);
            Vector2 vector2 = new Vector2(this.m_prefixScanX[col + colSpan], this.m_prefixScanY[row + rowSpan]) - vector;
            control.Position = new Vector2(vector.X + ((vector2.X * 0.5f) * ((float) alignH)), vector.Y + ((vector2.Y * 0.5f) * ((float) alignV)));
            control.OriginAlign = (MyGuiDrawAlignEnum) (((MyAlignH.Right | MyAlignH.Center) * alignH) + ((MyAlignH) ((int) alignV)));
            this.m_parent.Controls.Add(control);
        }

        public unsafe void AddWithSize(MyGuiControlBase control, MyAlignH alignH, MyAlignV alignV, int row, int col, int rowSpan = 1, int colSpan = 1)
        {
            Vector2 vector = new Vector2(this.m_prefixScanX[col], this.m_prefixScanY[row]);
            Vector2 vector2 = new Vector2(this.m_prefixScanX[col + colSpan], this.m_prefixScanY[row + rowSpan]) - vector;
            float* singlePtr1 = (float*) ref vector2.X;
            singlePtr1[0] -= 0.01f;
            float* singlePtr2 = (float*) ref vector2.Y;
            singlePtr2[0] -= 0.01f;
            control.Size = vector2;
            control.Position = new Vector2((vector.X + ((vector2.X * 0.5f) * ((float) alignH))) + 0.005f, (vector.Y + ((vector2.Y * 0.5f) * ((float) alignV))) + 0.005f);
            control.OriginAlign = (MyGuiDrawAlignEnum) (((MyAlignH.Right | MyAlignH.Center) * alignH) + ((MyAlignH) ((int) alignV)));
            this.m_parent.Controls.Add(control);
        }
    }
}

