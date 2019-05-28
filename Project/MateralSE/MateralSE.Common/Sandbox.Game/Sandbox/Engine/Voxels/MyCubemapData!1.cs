namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyCubemapData<TPixel> : IMyWrappedCubemapFace where TPixel: struct
    {
        private readonly int m_realResolution;
        public TPixel[] Data;
        public IEqualityComparer<TPixel> m_comparer;

        public MyCubemapData(int resolution, IEqualityComparer<TPixel> comparer = null)
        {
            this.m_realResolution = resolution + 2;
            this.Resolution = resolution;
            this.ResolutionMinusOne = resolution - 1;
            this.Data = new TPixel[this.m_realResolution * this.m_realResolution];
            this.m_comparer = comparer ?? EqualityComparer<TPixel>.Default;
        }

        public void CopyRange(Vector2I start, Vector2I end, IMyWrappedCubemapFace other, Vector2I oStart, Vector2I oEnd)
        {
            MyCubemapData<TPixel> data = other as MyCubemapData<TPixel>;
            if (data != null)
            {
                this.CopyRange(start, end, data, oStart, oEnd);
            }
        }

        public void CopyRange(Vector2I start, Vector2I end, MyCubemapData<TPixel> other, Vector2I oStart, Vector2I oEnd)
        {
            TPixel local;
            Vector2I step = MyCubemapHelpers.GetStep(ref start, ref end);
            Vector2I vectori2 = MyCubemapHelpers.GetStep(ref oStart, ref oEnd);
            while (start != end)
            {
                other.GetValue(oStart.X, oStart.Y, out local);
                this.SetValue(start.X, start.Y, ref local);
                start += step;
                oStart += vectori2;
            }
            other.GetValue(oStart.X, oStart.Y, out local);
            this.SetValue(start.X, start.Y, ref local);
        }

        public void FinishFace(string name)
        {
            TPixel pixel = default(TPixel);
            this.SetPixel(-1, -1, ref pixel);
            this.SetPixel(this.Resolution, -1, ref pixel);
            this.SetPixel(-1, this.Resolution, ref pixel);
            this.SetPixel(this.Resolution, this.Resolution, ref pixel);
        }

        private TPixel GetMostFrequentValue(TPixel[] bytes)
        {
            switch (bytes.Length)
            {
                case 2:
                    return bytes[0];

                case 3:
                    if (this.m_comparer.Equals(bytes[0], bytes[1]) || this.m_comparer.Equals(bytes[0], bytes[2]))
                    {
                        return bytes[0];
                    }
                    return (!this.m_comparer.Equals(bytes[1], bytes[2]) ? bytes[2] : bytes[1]);

                case 4:
                    if ((this.m_comparer.Equals(bytes[0], bytes[1]) || this.m_comparer.Equals(bytes[0], bytes[2])) || this.m_comparer.Equals(bytes[0], bytes[3]))
                    {
                        return bytes[0];
                    }
                    if (this.m_comparer.Equals(bytes[1], bytes[2]) || this.m_comparer.Equals(bytes[1], bytes[3]))
                    {
                        return bytes[1];
                    }
                    return (!this.m_comparer.Equals(bytes[2], bytes[3]) ? bytes[3] : bytes[2]);
            }
            return bytes[0];
        }

        public int GetRowStart(int y) => 
            (((y + 1) * this.m_realResolution) + 1);

        public TPixel GetValue(float x, float y)
        {
            int num = (int) (this.Resolution * x);
            int num2 = (int) (this.Resolution * y);
            return this.Data[((num2 + 1) * this.m_realResolution) + (num + 1)];
        }

        public void GetValue(int x, int y, out TPixel value)
        {
            value = this.Data[((y + 1) * this.m_realResolution) + (x + 1)];
        }

        public void SetMaterial(int x, int y, TPixel value)
        {
            this.Data[((y + 1) * this.m_realResolution) + (x + 1)] = value;
        }

        internal void SetPixel(int y, int x, ref TPixel pixel)
        {
            this.Data[((y + 1) * this.m_realResolution) + (x + 1)] = pixel;
        }

        public void SetValue(int x, int y, ref TPixel value)
        {
            int index = ((y + 1) * this.m_realResolution) + (x + 1);
            this.Data[index] = value;
        }

        public int RowStride =>
            this.m_realResolution;

        public int Resolution { get; set; }

        public int ResolutionMinusOne { get; set; }
    }
}

