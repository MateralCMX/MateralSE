namespace VRageMath
{
    using System;

    [Serializable]
    internal class GjkD
    {
        private static int[] BitsToIndices = new int[] { 0, 1, 2, 0x11, 3, 0x19, 0x1a, 0xd1, 4, 0x21, 0x22, 0x111, 0x23, 0x119, 0x11a, 0x8d1 };
        private Vector3D closestPoint;
        private Vector3D[] y = new Vector3D[4];
        private double[] yLengthSq = new double[4];
        private Vector3D[][] edges = new Vector3D[][] { new Vector3D[4], new Vector3D[4], new Vector3D[4], new Vector3D[4] };
        private double[][] edgeLengthSq = new double[][] { new double[4], new double[4], new double[4], new double[4] };
        private double[][] det = new double[0x10][];
        private int simplexBits;
        private double maxLengthSq;

        public GjkD()
        {
            for (int i = 0; i < 0x10; i++)
            {
                this.det[i] = new double[4];
            }
        }

        public bool AddSupportPoint(ref Vector3D newPoint)
        {
            int index = (BitsToIndices[this.simplexBits ^ 15] & 7) - 1;
            this.y[index] = newPoint;
            this.yLengthSq[index] = newPoint.LengthSquared();
            for (int i = BitsToIndices[this.simplexBits]; i != 0; i = i >> 3)
            {
                int num3 = (i & 7) - 1;
                Vector3D vectord = this.y[num3] - newPoint;
                this.edges[num3][index] = vectord;
                this.edges[index][num3] = -vectord;
                this.edgeLengthSq[index][num3] = this.edgeLengthSq[num3][index] = vectord.LengthSquared();
            }
            this.UpdateDeterminant(index);
            return this.UpdateSimplex(index);
        }

        private Vector3D ComputeClosestPoint()
        {
            double num = 0.0;
            Vector3D zero = Vector3D.Zero;
            this.maxLengthSq = 0.0;
            for (int i = BitsToIndices[this.simplexBits]; i != 0; i = i >> 3)
            {
                int index = (i & 7) - 1;
                double num4 = this.det[this.simplexBits][index];
                num += num4;
                zero += this.y[index] * num4;
                this.maxLengthSq = MathHelper.Max(this.maxLengthSq, this.yLengthSq[index]);
            }
            return (zero / num);
        }

        private static double Dot(ref Vector3D a, ref Vector3D b) => 
            (((a.X * b.X) + (a.Y * b.Y)) + (a.Z * b.Z));

        private bool IsSatisfiesRule(int xBits, int yBits)
        {
            bool flag = true;
            for (int i = BitsToIndices[yBits]; i != 0; i = i >> 3)
            {
                int index = (i & 7) - 1;
                int num3 = 1 << (index & 0x1f);
                if ((num3 & xBits) != 0)
                {
                    if (this.det[xBits][index] <= 0.0)
                    {
                        flag = false;
                        break;
                    }
                }
                else if (this.det[xBits | num3][index] > 0.0)
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        public void Reset()
        {
            this.simplexBits = 0;
            this.maxLengthSq = 0.0;
        }

        private void UpdateDeterminant(int xmIdx)
        {
            int index = 1 << (xmIdx & 0x1f);
            this.det[index][xmIdx] = 1.0;
            int num2 = BitsToIndices[this.simplexBits];
            int num3 = num2;
            int num4 = 0;
            while (num3 != 0)
            {
                int num9 = (num3 & 7) - 1;
                int num10 = 1 << (num9 & 0x1f);
                int num11 = num10 | index;
                this.det[num11][num9] = Dot(ref this.edges[xmIdx][num9], ref this.y[xmIdx]);
                this.det[num11][xmIdx] = Dot(ref this.edges[num9][xmIdx], ref this.y[num9]);
                int num12 = num2;
                int num13 = 0;
                while (true)
                {
                    if (num13 >= num4)
                    {
                        num3 = num3 >> 3;
                        num4++;
                        break;
                    }
                    int num14 = (num12 & 7) - 1;
                    int num15 = 1 << (num14 & 0x1f);
                    int num16 = num11 | num15;
                    int num17 = (this.edgeLengthSq[num9][num14] < this.edgeLengthSq[xmIdx][num14]) ? num9 : xmIdx;
                    this.det[num16][num14] = (this.det[num11][num9] * Dot(ref this.edges[num17][num14], ref this.y[num9])) + (this.det[num11][xmIdx] * Dot(ref this.edges[num17][num14], ref this.y[xmIdx]));
                    int num18 = (this.edgeLengthSq[num14][num9] < this.edgeLengthSq[xmIdx][num9]) ? num14 : xmIdx;
                    this.det[num16][num9] = (this.det[num15 | index][num14] * Dot(ref this.edges[num18][num9], ref this.y[num14])) + (this.det[num15 | index][xmIdx] * Dot(ref this.edges[num18][num9], ref this.y[xmIdx]));
                    int num19 = (this.edgeLengthSq[num9][xmIdx] < this.edgeLengthSq[num14][xmIdx]) ? num9 : num14;
                    this.det[num16][xmIdx] = (this.det[num10 | num15][num14] * Dot(ref this.edges[num19][xmIdx], ref this.y[num14])) + (this.det[num10 | num15][num9] * Dot(ref this.edges[num19][xmIdx], ref this.y[num9]));
                    num12 = num12 >> 3;
                    num13++;
                }
            }
            if ((this.simplexBits | index) == 15)
            {
                int num5 = (this.edgeLengthSq[1][0] < this.edgeLengthSq[2][0]) ? ((this.edgeLengthSq[1][0] < this.edgeLengthSq[3][0]) ? 1 : 3) : ((this.edgeLengthSq[2][0] < this.edgeLengthSq[3][0]) ? 2 : 3);
                this.det[15][0] = ((this.det[14][1] * Dot(ref this.edges[num5][0], ref this.y[1])) + (this.det[14][2] * Dot(ref this.edges[num5][0], ref this.y[2]))) + (this.det[14][3] * Dot(ref this.edges[num5][0], ref this.y[3]));
                int num6 = (this.edgeLengthSq[0][1] < this.edgeLengthSq[2][1]) ? ((this.edgeLengthSq[0][1] < this.edgeLengthSq[3][1]) ? 0 : 3) : ((this.edgeLengthSq[2][1] < this.edgeLengthSq[3][1]) ? 2 : 3);
                this.det[15][1] = ((this.det[13][0] * Dot(ref this.edges[num6][1], ref this.y[0])) + (this.det[13][2] * Dot(ref this.edges[num6][1], ref this.y[2]))) + (this.det[13][3] * Dot(ref this.edges[num6][1], ref this.y[3]));
                int num7 = (this.edgeLengthSq[0][2] < this.edgeLengthSq[1][2]) ? ((this.edgeLengthSq[0][2] < this.edgeLengthSq[3][2]) ? 0 : 3) : ((this.edgeLengthSq[1][2] < this.edgeLengthSq[3][2]) ? 1 : 3);
                this.det[15][2] = ((this.det[11][0] * Dot(ref this.edges[num7][2], ref this.y[0])) + (this.det[11][1] * Dot(ref this.edges[num7][2], ref this.y[1]))) + (this.det[11][3] * Dot(ref this.edges[num7][2], ref this.y[3]));
                int num8 = (this.edgeLengthSq[0][3] < this.edgeLengthSq[1][3]) ? ((this.edgeLengthSq[0][3] < this.edgeLengthSq[2][3]) ? 0 : 2) : ((this.edgeLengthSq[1][3] < this.edgeLengthSq[2][3]) ? 1 : 2);
                this.det[15][3] = ((this.det[7][0] * Dot(ref this.edges[num8][3], ref this.y[0])) + (this.det[7][1] * Dot(ref this.edges[num8][3], ref this.y[1]))) + (this.det[7][2] * Dot(ref this.edges[num8][3], ref this.y[2]));
            }
        }

        private bool UpdateSimplex(int newIndex)
        {
            int yBits = this.simplexBits | (1 << (newIndex & 0x1f));
            int xBits = 1 << (newIndex & 0x1f);
            for (int i = this.simplexBits; i != 0; i--)
            {
                if (((i & yBits) == i) && this.IsSatisfiesRule(i | xBits, yBits))
                {
                    this.simplexBits = i | xBits;
                    this.closestPoint = this.ComputeClosestPoint();
                    return true;
                }
            }
            bool flag = false;
            if (this.IsSatisfiesRule(xBits, yBits))
            {
                this.simplexBits = xBits;
                this.closestPoint = this.y[newIndex];
                this.maxLengthSq = this.yLengthSq[newIndex];
                flag = true;
            }
            return flag;
        }

        public bool FullSimplex =>
            (this.simplexBits == 15);

        public double MaxLengthSquared =>
            this.maxLengthSq;

        public Vector3D ClosestPoint =>
            this.closestPoint;
    }
}

