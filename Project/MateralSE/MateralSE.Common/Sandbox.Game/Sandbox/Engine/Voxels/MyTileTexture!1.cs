namespace Sandbox.Engine.Voxels
{
    using SharpDX.Toolkit.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyTileTexture<TPixel> where TPixel: struct
    {
        private TPixel[] m_data;
        private int m_stride;
        private Vector2I m_cellSize;
        private static readonly Vector2B[] s_baseCellCoords;
        private Vector2I[] m_cellCoords;
        public static readonly MyTileTexture<TPixel> Default;

        static MyTileTexture()
        {
            Vector2B[] vectorbArray1 = new Vector2B[0x10];
            vectorbArray1[0] = new Vector2B(0, 0);
            vectorbArray1[1] = new Vector2B(1, 0);
            vectorbArray1[2] = new Vector2B(2, 0);
            vectorbArray1[3] = new Vector2B(3, 0);
            vectorbArray1[4] = new Vector2B(0, 1);
            vectorbArray1[5] = new Vector2B(1, 1);
            vectorbArray1[6] = new Vector2B(2, 1);
            vectorbArray1[7] = new Vector2B(3, 1);
            vectorbArray1[8] = new Vector2B(0, 2);
            vectorbArray1[9] = new Vector2B(1, 2);
            vectorbArray1[10] = new Vector2B(2, 2);
            vectorbArray1[11] = new Vector2B(3, 2);
            vectorbArray1[12] = new Vector2B(0, 3);
            vectorbArray1[13] = new Vector2B(1, 3);
            vectorbArray1[14] = new Vector2B(2, 3);
            vectorbArray1[15] = new Vector2B(3, 3);
            MyTileTexture<TPixel>.s_baseCellCoords = vectorbArray1;
            MyTileTexture<TPixel>.Default = new MyTileTexture<TPixel>();
        }

        public MyTileTexture()
        {
            this.m_cellCoords = new Vector2I[0x10];
            this.m_stride = 4;
            this.m_cellSize = new Vector2I(1);
            this.m_data = new TPixel[0x10];
            this.PrepareCellCoords();
        }

        public MyTileTexture(PixelBuffer image, int cellSize)
        {
            this.m_cellCoords = new Vector2I[0x10];
            this.m_stride = image.RowStride;
            this.m_cellSize = new Vector2I(cellSize);
            this.m_data = image.GetPixels<TPixel>(0);
            this.PrepareCellCoords();
        }

        public void GetValue(int corners, Vector2 coords, out TPixel value)
        {
            if (corners > 15)
            {
                value = default(TPixel);
            }
            else
            {
                Vector2I vectori = new Vector2I(coords * this.m_cellSize.X);
                vectori += this.m_cellCoords[corners];
                value = this.m_data[vectori.X + (vectori.Y * this.m_stride)];
            }
        }

        public void GetValue(int corners, Vector2I coords, out TPixel value)
        {
            if (corners > 15)
            {
                value = default(TPixel);
            }
            else
            {
                coords += this.m_cellCoords[corners];
                value = this.m_data[coords.X + (coords.Y * this.m_stride)];
            }
        }

        private void PrepareCellCoords()
        {
            for (int i = 0; i < 0x10; i++)
            {
                this.m_cellCoords[i] = MyTileTexture<TPixel>.s_baseCellCoords[i] * this.m_cellSize.X;
            }
        }
    }
}

