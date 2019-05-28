namespace Sandbox.Engine.Voxels.Planet
{
    using Sandbox.Engine.Voxels;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyHeightmapFace : IMyWrappedCubemapFace
    {
        private int m_realResolution;
        private float m_pixelSizeFour;
        private float m_pixelSize;
        private Box2I m_bounds;
        public static readonly MyHeightmapFace Default = new MyHeightmapFace(HeightmapNode.HEIGHTMAP_LEAF_SIZE);
        public ushort[] Data;
        public MyHeightmapNormal[] NormalData;
        private static float HEIGHTMAP_BRANCH_LOG_RECIP = ((float) (1.0 / -Math.Log((double) HeightmapNode.HEIGHTMAP_BRANCH_FACTOR)));
        public HeightmapNode Root;
        public HeightmapLevel[] PruningTree;
        [ThreadStatic]
        private static SEntry[] m_queryStack;

        static MyHeightmapFace()
        {
            Default.Zero();
        }

        public MyHeightmapFace(int resolution)
        {
            this.m_realResolution = resolution + 2;
            this.Resolution = resolution;
            this.ResolutionMinusOne = this.Resolution - 1;
            this.Data = new ushort[this.m_realResolution * this.m_realResolution];
            this.m_pixelSizeFour = 4f / ((float) this.Resolution);
            this.m_pixelSize = 1f / ((float) this.Resolution);
            this.m_bounds = new Box2I(Vector2I.Zero, new Vector2I(this.Resolution - 1));
        }

        public void CopyRange(Vector2I start, Vector2I end, IMyWrappedCubemapFace other, Vector2I oStart, Vector2I oEnd)
        {
            this.CopyRange(start, end, other as MyHeightmapFace, oStart, oEnd);
        }

        public void CopyRange(Vector2I start, Vector2I end, MyHeightmapFace other, Vector2I oStart, Vector2I oEnd)
        {
            ushort num;
            Vector2I step = MyCubemapHelpers.GetStep(ref start, ref end);
            Vector2I vectori2 = MyCubemapHelpers.GetStep(ref oStart, ref oEnd);
            while (start != end)
            {
                other.GetValue(oStart.X, oStart.Y, out num);
                this.SetValue(start.X, start.Y, num);
                start += step;
                oStart += vectori2;
            }
            other.GetValue(oStart.X, oStart.Y, out num);
            this.SetValue(start.X, start.Y, num);
        }

        public void CreatePruningTree(string mapName)
        {
            int num = 0;
            int num2 = this.Resolution / HeightmapNode.HEIGHTMAP_LEAF_SIZE;
            while (num2 != 1)
            {
                if ((num2 % HeightmapNode.HEIGHTMAP_BRANCH_FACTOR) != 0)
                {
                    object[] args = new object[] { mapName };
                    MyLog.Default.Error("Cannot build prunning tree for heightmap face {0}!", args);
                    object[] objArray2 = new object[] { HeightmapNode.HEIGHTMAP_BRANCH_FACTOR, HeightmapNode.HEIGHTMAP_LEAF_SIZE };
                    MyLog.Default.Error("Heightmap resolution must be divisible by {1}, and after that a power of {0}. Failing to achieve so will disable several important optimizations!!", objArray2);
                    return;
                }
                num++;
                num2 /= HeightmapNode.HEIGHTMAP_BRANCH_FACTOR;
            }
            this.PruningTree = new HeightmapLevel[num];
            int rowStart = this.GetRowStart(0);
            if (num == 0)
            {
                float positiveInfinity = float.PositiveInfinity;
                float negativeInfinity = float.NegativeInfinity;
                int num10 = rowStart;
                int num11 = 0;
                while (num11 < HeightmapNode.HEIGHTMAP_LEAF_SIZE)
                {
                    int num12 = 0;
                    while (true)
                    {
                        if (num12 >= HeightmapNode.HEIGHTMAP_LEAF_SIZE)
                        {
                            num10 += this.m_realResolution;
                            num11++;
                            break;
                        }
                        float num13 = this.Data[num10 + num12] * 1.525902E-05f;
                        if (positiveInfinity > num13)
                        {
                            positiveInfinity = num13;
                        }
                        if (negativeInfinity < num13)
                        {
                            negativeInfinity = num13;
                        }
                        num12++;
                    }
                }
                this.Root.Max = negativeInfinity;
                this.Root.Min = positiveInfinity;
            }
            else
            {
                int num1 = HeightmapNode.HEIGHTMAP_BRANCH_FACTOR;
                num2 = this.Resolution / HeightmapNode.HEIGHTMAP_LEAF_SIZE;
                this.PruningTree[0].Nodes = new HeightmapNode[num2 * num2];
                this.PruningTree[0].Res = (uint) num2;
                int index = 0;
                int num14 = 0;
                while (num14 < num2)
                {
                    int num15 = rowStart;
                    int num16 = 0;
                    while (true)
                    {
                        if (num16 >= num2)
                        {
                            rowStart += HeightmapNode.HEIGHTMAP_LEAF_SIZE * this.m_realResolution;
                            num14++;
                            break;
                        }
                        float num17 = float.PositiveInfinity;
                        float num18 = float.NegativeInfinity;
                        int num19 = num15 - this.m_realResolution;
                        int num20 = -1;
                        while (true)
                        {
                            if (num20 > HeightmapNode.HEIGHTMAP_LEAF_SIZE)
                            {
                                HeightmapNode node = new HeightmapNode {
                                    Max = num18,
                                    Min = num17
                                };
                                this.PruningTree[0].Nodes[index] = node;
                                index++;
                                num15 += HeightmapNode.HEIGHTMAP_LEAF_SIZE;
                                num16++;
                                break;
                            }
                            int num21 = -1;
                            while (true)
                            {
                                if (num21 > HeightmapNode.HEIGHTMAP_LEAF_SIZE)
                                {
                                    num19 += this.m_realResolution;
                                    num20++;
                                    break;
                                }
                                float num22 = this.Data[num19 + num21] * 1.525902E-05f;
                                if (num17 > num22)
                                {
                                    num17 = num22;
                                }
                                if (num18 < num22)
                                {
                                    num18 = num22;
                                }
                                num21++;
                            }
                        }
                    }
                }
                int num5 = 0;
                int num23 = 1;
                while (num23 < num)
                {
                    rowStart = 0;
                    int num24 = num2 / HeightmapNode.HEIGHTMAP_BRANCH_FACTOR;
                    this.PruningTree[num23].Nodes = new HeightmapNode[num24 * num24];
                    this.PruningTree[num23].Res = (uint) num24;
                    index = 0;
                    int num25 = 0;
                    while (true)
                    {
                        if (num25 >= num24)
                        {
                            num5++;
                            num2 = num24;
                            num23++;
                            break;
                        }
                        int num26 = rowStart;
                        int num27 = 0;
                        while (true)
                        {
                            if (num27 >= num24)
                            {
                                rowStart += HeightmapNode.HEIGHTMAP_BRANCH_FACTOR * num2;
                                num25++;
                                break;
                            }
                            float min = float.PositiveInfinity;
                            float max = float.NegativeInfinity;
                            int num30 = num26;
                            int num31 = 0;
                            while (true)
                            {
                                if (num31 >= HeightmapNode.HEIGHTMAP_BRANCH_FACTOR)
                                {
                                    this.PruningTree[num23].Nodes[index] = new HeightmapNode { 
                                        Max = max,
                                        Min = min
                                    };
                                    index++;
                                    num26 += HeightmapNode.HEIGHTMAP_BRANCH_FACTOR;
                                    num27++;
                                    break;
                                }
                                int num32 = 0;
                                while (true)
                                {
                                    if (num32 >= HeightmapNode.HEIGHTMAP_BRANCH_FACTOR)
                                    {
                                        num30 += num2;
                                        num31++;
                                        break;
                                    }
                                    HeightmapNode node2 = this.PruningTree[num5].Nodes[num30 + num32];
                                    if (min > node2.Min)
                                    {
                                        min = node2.Min;
                                    }
                                    if (max < node2.Max)
                                    {
                                        max = node2.Max;
                                    }
                                    num32++;
                                }
                            }
                        }
                    }
                }
                float positiveInfinity = float.PositiveInfinity;
                float negativeInfinity = float.NegativeInfinity;
                rowStart = 0;
                int num33 = 0;
                while (num33 < HeightmapNode.HEIGHTMAP_BRANCH_FACTOR)
                {
                    int num34 = 0;
                    while (true)
                    {
                        if (num34 >= HeightmapNode.HEIGHTMAP_BRANCH_FACTOR)
                        {
                            num33++;
                            break;
                        }
                        rowStart++;
                        HeightmapNode node3 = this.PruningTree[num - 1].Nodes[rowStart];
                        if (positiveInfinity > node3.Min)
                        {
                            positiveInfinity = node3.Min;
                        }
                        if (negativeInfinity < node3.Max)
                        {
                            negativeInfinity = node3.Max;
                        }
                        num34++;
                    }
                }
                this.Root.Max = negativeInfinity;
                this.Root.Min = positiveInfinity;
            }
        }

        public void FinishFace(string faceName)
        {
            int x = this.Resolution - 1;
            int num = (this.GetValue(0, 0) + this.GetValue(-1, 0)) + this.GetValue(0, -1);
            this.SetValue(-1, -1, (ushort) (num / 3));
            num = (this.GetValue(x, 0) + this.GetValue(this.Resolution, 0)) + this.GetValue(x, -1);
            this.SetValue(this.Resolution, -1, (ushort) (num / 3));
            num = (this.GetValue(0, x) + this.GetValue(-1, x)) + this.GetValue(0, this.Resolution);
            this.SetValue(-1, this.Resolution, (ushort) (num / 3));
            num = (this.GetValue(x, x) + this.GetValue(this.Resolution, x)) + this.GetValue(x, this.Resolution);
            this.SetValue(this.Resolution, this.Resolution, (ushort) (num / 3));
            this.CreatePruningTree(faceName);
        }

        public unsafe void Get4Row(int linearOfft, float* values)
        {
            values[0] = this.Data[linearOfft] * 1.525902E-05f;
            values[4] = this.Data[linearOfft + 1] * 1.525902E-05f;
            values[2 * 4] = this.Data[linearOfft + 2] * 1.525902E-05f;
            values[3 * 4] = this.Data[linearOfft + 3] * 1.525902E-05f;
        }

        public unsafe void Get4Row(int linearOfft, float* values, ushort* map)
        {
            values[0] = map[linearOfft] * 1.525902E-05f;
            values[4] = map[linearOfft + 1] * 1.525902E-05f;
            values[2 * 4] = map[linearOfft + 2] * 1.525902E-05f;
            values[3 * 4] = map[linearOfft + 3] * 1.525902E-05f;
        }

        public unsafe void GetBounds(ref BoundingBox query)
        {
            float num = Math.Max(query.Width, query.Height);
            if (((num >= this.m_pixelSizeFour) && (this.PruningTree != null)) && (this.PruningTree.Length != 0))
            {
                double num2 = Math.Log((double) (((float) this.Resolution) / (num * HeightmapNode.HEIGHTMAP_LEAF_SIZE))) / Math.Log((double) HeightmapNode.HEIGHTMAP_BRANCH_FACTOR);
                uint index = ((uint) (this.PruningTree.Length - 1)) - ((uint) MathHelper.Clamp(num2, 0.0, (double) (this.PruningTree.Length - 1)));
                Box2I other = new Box2I(Vector2I.Zero, new Vector2I(((int) this.PruningTree[index].Res) - 1));
                Box2I boxi2 = new Box2I(ref query, this.PruningTree[index].Res);
                boxi2.Intersect(ref other);
                query.Min.Z = float.PositiveInfinity;
                query.Max.Z = float.NegativeInfinity;
                int res = (int) this.PruningTree[index].Res;
                int y = boxi2.Min.Y;
                while (y <= boxi2.Max.Y)
                {
                    int x = boxi2.Min.X;
                    while (true)
                    {
                        if (x > boxi2.Max.X)
                        {
                            y++;
                            break;
                        }
                        HeightmapNode node = this.PruningTree[index].Nodes[(y * res) + x];
                        if (query.Min.Z > node.Min)
                        {
                            query.Min.Z = node.Min;
                        }
                        if (query.Max.Z < node.Max)
                        {
                            query.Max.Z = node.Max;
                        }
                        x++;
                    }
                }
            }
            else
            {
                ushort num5;
                Box2I boxi3 = new Box2I(ref query, (uint) this.Resolution);
                Vector2I* vectoriPtr1 = (Vector2I*) ref boxi3.Min;
                vectoriPtr1[0] -= 1;
                Vector2I* vectoriPtr2 = (Vector2I*) ref boxi3.Max;
                vectoriPtr2[0] += 1;
                boxi3.Intersect(ref this.m_bounds);
                this.GetValue(boxi3.Min.X, boxi3.Min.Y, out num5);
                int num6 = 0xffff;
                int num7 = 0;
                int y = boxi3.Min.Y;
                while (y <= boxi3.Max.Y)
                {
                    int x = boxi3.Min.X;
                    while (true)
                    {
                        if (x > boxi3.Max.X)
                        {
                            y++;
                            break;
                        }
                        this.GetValue(x, y, out num5);
                        if (num5 > num7)
                        {
                            num7 = num5;
                        }
                        if (num5 < num6)
                        {
                            num6 = num5;
                        }
                        x++;
                    }
                }
                int num8 = ((num7 - num6) * 2) / 3;
                num7 += num8;
                num6 -= num8;
                query.Min.Z = num6 * 1.525902E-05f;
                query.Max.Z = num7 * 1.525902E-05f;
            }
        }

        public unsafe void GetHermiteSliceRow(int linearOfft, float* values)
        {
            values[0] = this.Data[linearOfft + 1] * 1.525902E-05f;
            values[4] = (this.Data[linearOfft + 2] - this.Data[linearOfft]) * 3.051804E-05f;
            values[2 * 4] = this.Data[linearOfft + 2] * 1.525902E-05f;
            values[3 * 4] = (this.Data[linearOfft + 3] - this.Data[linearOfft + 1]) * 3.051804E-05f;
        }

        public void GetNormal(int x, int y, out MyHeightmapNormal normal)
        {
            normal = this.NormalData[(y * this.Resolution) + x];
        }

        public int GetRowStart(int y) => 
            (((y + 1) * this.m_realResolution) + 1);

        public ushort GetValue(int x, int y)
        {
            if (x < 0)
            {
                x = 0;
            }
            else if (x >= this.Resolution)
            {
                x = this.Resolution - 1;
            }
            if (y < 0)
            {
                y = 0;
            }
            else if (y >= this.Resolution)
            {
                y = this.Resolution - 1;
            }
            return this.Data[((y + 1) * this.m_realResolution) + (x + 1)];
        }

        public void GetValue(int x, int y, out ushort value)
        {
            value = this.Data[((y + 1) * this.m_realResolution) + (x + 1)];
        }

        public float GetValuef(int x, int y) => 
            (this.Data[((y + 1) * this.m_realResolution) + (x + 1)] * 1.525902E-05f);

        public unsafe ContainmentType QueryHeight(ref BoundingBox query)
        {
            ContainmentType intersects;
            int num3;
            SEntry[] queryStack;
            SEntry entry;
            int y;
            int x;
            if (this.PruningTree == null)
            {
                return ContainmentType.Intersects;
            }
            if ((m_queryStack == null) || (m_queryStack.Length < this.PruningTree.Length))
            {
                m_queryStack = new SEntry[this.PruningTree.Length];
            }
            if (query.Min.Z > this.Root.Max)
            {
                return ContainmentType.Disjoint;
            }
            if (query.Max.Z < this.Root.Min)
            {
                return ContainmentType.Contains;
            }
            if (query.Max.X < 0f)
            {
                goto TR_0003;
            }
            else if (query.Max.Y < 0f)
            {
                goto TR_0003;
            }
            else if ((query.Min.X <= 1f) && (query.Min.Y <= 1f))
            {
                if (this.PruningTree.Length == 0)
                {
                    return ContainmentType.Intersects;
                }
                if (query.Max.X == 1.0)
                {
                    query.Max.X = 1f;
                }
                if (query.Max.Y == 1.0)
                {
                    query.Max.Y = 1f;
                }
                intersects = ContainmentType.Intersects;
                float num = Math.Max(query.Width, query.Height);
                if (num < this.m_pixelSizeFour)
                {
                    ushort num6;
                    Box2I boxi2 = new Box2I(ref query, (uint) this.Resolution);
                    Vector2I* vectoriPtr1 = (Vector2I*) ref boxi2.Min;
                    vectoriPtr1[0] -= 1;
                    Vector2I* vectoriPtr2 = (Vector2I*) ref boxi2.Max;
                    vectoriPtr2[0] += 1;
                    boxi2.Intersect(ref this.m_bounds);
                    int num4 = (int) (query.Min.Z * 65535f);
                    int num5 = (int) (query.Max.Z * 65535f);
                    this.GetValue(boxi2.Min.X, boxi2.Min.Y, out num6);
                    if (num6 > num5)
                    {
                        intersects = ContainmentType.Contains;
                    }
                    else
                    {
                        if (num6 >= num4)
                        {
                            return ContainmentType.Intersects;
                        }
                        intersects = ContainmentType.Disjoint;
                    }
                    int num7 = 0xffff;
                    int num8 = 0;
                    int y = boxi2.Min.Y;
                    while (y <= boxi2.Max.Y)
                    {
                        int x = boxi2.Min.X;
                        while (true)
                        {
                            if (x > boxi2.Max.X)
                            {
                                y++;
                                break;
                            }
                            this.GetValue(x, y, out num6);
                            if (num6 > num8)
                            {
                                num8 = num6;
                            }
                            if (num6 < num7)
                            {
                                num7 = num6;
                            }
                            x++;
                        }
                    }
                    int num9 = num8 - num7;
                    num9 += num9 >> 1;
                    num7 -= num9;
                    return ((num4 <= (num8 + num9)) ? ((num5 >= num7) ? ContainmentType.Intersects : ContainmentType.Contains) : ContainmentType.Disjoint);
                }
                uint index = (uint) MathHelper.Clamp(Math.Log((double) (num * (this.Resolution / HeightmapNode.HEIGHTMAP_LEAF_SIZE))) / Math.Log((double) HeightmapNode.HEIGHTMAP_BRANCH_FACTOR), 0.0, (double) (this.PruningTree.Length - 1));
                num3 = 0;
                queryStack = m_queryStack;
                Box2I other = new Box2I(Vector2I.Zero, new Vector2I(((int) this.PruningTree[index].Res) - 1));
                queryStack[0].Bounds = new Box2I(ref query, this.PruningTree[index].Res);
                queryStack[0].Bounds.Intersect(ref other);
                queryStack[0].Next = queryStack[0].Bounds.Min;
                queryStack[0].Level = index;
                queryStack[0].Result = intersects;
                queryStack[0].Continue = false;
            }
            else
            {
                goto TR_0003;
            }
            goto TR_0035;
        TR_0003:
            return ContainmentType.Disjoint;
        TR_001A:
            intersects = entry.Result;
            num3--;
            if (num3 >= 0)
            {
                queryStack[num3].Intersection = intersects;
            }
            goto TR_0035;
        TR_001E:
            x++;
            goto TR_002E;
        TR_0026:
            switch (entry.Intersection)
            {
                case ContainmentType.Disjoint:
                    if (entry.Result != ContainmentType.Contains)
                    {
                        entry.Result = ContainmentType.Disjoint;
                        goto TR_001E;
                    }
                    else
                    {
                        entry.Result = ContainmentType.Intersects;
                    }
                    break;

                case ContainmentType.Contains:
                    if (entry.Result != ContainmentType.Disjoint)
                    {
                        entry.Result = ContainmentType.Contains;
                        goto TR_001E;
                    }
                    else
                    {
                        entry.Result = ContainmentType.Intersects;
                    }
                    break;

                case ContainmentType.Intersects:
                    entry.Result = ContainmentType.Intersects;
                    break;

                default:
                    goto TR_001E;
            }
            goto TR_001A;
        TR_002E:
            while (true)
            {
                if (x <= entry.Bounds.Max.X)
                {
                    if (entry.Continue)
                    {
                        entry.Continue = false;
                        x = entry.Next.X;
                        goto TR_0026;
                    }
                    else
                    {
                        SEntry* entryPtr1 = (SEntry*) ref entry;
                        entryPtr1->Intersection = this.PruningTree[entry.Level].Intersect(x, y, ref query);
                        if (entry.Intersection != ContainmentType.Intersects)
                        {
                            goto TR_0026;
                        }
                        else
                        {
                            if (this.PruningTree[entry.Level].IsCellNotContained(x, y, ref query) && (entry.Level != 0))
                            {
                                entry.Next = new Vector2I(x, y);
                                entry.Continue = true;
                                queryStack[num3] = entry;
                                num3++;
                                queryStack[num3] = new SEntry(ref query, this.PruningTree[((int) entry.Level) - 1].Res, new Vector2I(x, y), entry.Result, entry.Level - 1);
                                break;
                            }
                            goto TR_0026;
                        }
                    }
                }
                else
                {
                    y++;
                    goto TR_0031;
                }
                break;
            }
            goto TR_0035;
        TR_0031:
            while (true)
            {
                if (y <= entry.Bounds.Max.Y)
                {
                    x = entry.Bounds.Min.X;
                }
                else
                {
                    goto TR_001A;
                }
                break;
            }
            goto TR_002E;
        TR_0035:
            while (true)
            {
                if (num3 == -1)
                {
                    return intersects;
                }
                entry = queryStack[num3];
                y = entry.Next.Y;
                break;
            }
            goto TR_0031;
        }

        public bool QueryLine(ref Vector3 from, ref Vector3 to, out float startOffset, out float endOffset)
        {
            if (this.PruningTree == null)
            {
                startOffset = 0f;
                endOffset = 1f;
                return true;
            }
            Vector2 vector = new Vector2(from.X, from.Y);
            Vector2 vector2 = new Vector2(to.X, to.Y);
            vector *= this.ResolutionMinusOne;
            vector2 *= this.ResolutionMinusOne;
            int num = (int) Math.Ceiling((double) (vector2 - vector).Length());
            Vector3 vector3 = new Vector3(vector - vector2, (to.Z - from.Z) * 65535f);
            float z = vector3.Z;
            vector3 *= 1f / ((float) num);
            Vector3 vector4 = new Vector3(vector, from.Z * 65535f);
            float num4 = vector4.Z;
            int num5 = 0;
            while (num5 < num)
            {
                int num6 = (int) Math.Round((double) vector4.X);
                int num7 = (int) Math.Round((double) vector4.Y);
                int num8 = (int) vector4.Z;
                int num9 = (int) ((vector4.Z + vector3.Z) + 0.5f);
                if (num8 > num9)
                {
                    num9 = num8;
                    num8 = num9;
                }
                int num10 = 0x7fffffff;
                int num11 = -2147483648;
                int num12 = -1;
                while (true)
                {
                    if (num12 >= 2)
                    {
                        if ((num9 < num10) || (num8 > num11))
                        {
                            vector4 += vector3;
                            num5++;
                            break;
                        }
                        startOffset = (vector3.Z >= 0f) ? Math.Max((float) ((num10 - num4) / z), (float) 0f) : Math.Max((float) (-(num4 - num11) / z), (float) 0f);
                        endOffset = 1f;
                        return (startOffset < endOffset);
                    }
                    int num13 = -1;
                    while (true)
                    {
                        if (num13 >= 2)
                        {
                            num12++;
                            break;
                        }
                        ushort num1 = this.GetValue(num6 + num12, num7 + num13);
                        num10 = Math.Min(num1, num10);
                        num11 = Math.Max(num1, num11);
                        num13++;
                    }
                }
            }
            startOffset = 0f;
            endOffset = 1f;
            return false;
        }

        public void SetValue(int x, int y, ushort value)
        {
            this.Data[((y + 1) * this.m_realResolution) + (x + 1)] = value;
        }

        public void Zero()
        {
            for (int i = 0; i < this.Data.Length; i++)
            {
                this.Data[i] = 0;
            }
        }

        public int Resolution { get; set; }

        public int ResolutionMinusOne { get; set; }

        public int RowStride =>
            this.m_realResolution;

        public ushort this[int x, int y]
        {
            get
            {
                if (x < 0)
                {
                    x = 0;
                }
                else if (x >= this.Resolution)
                {
                    x = this.Resolution - 1;
                }
                if (y < 0)
                {
                    y = 0;
                }
                else if (y >= this.Resolution)
                {
                    y = this.Resolution - 1;
                }
                return this.Data[((y + 1) * this.m_realResolution) + (x + 1)];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Box2I
        {
            public Vector2I Min;
            public Vector2I Max;
            public Box2I(ref BoundingBox bb, uint scale)
            {
                this.Min = new Vector2I((int) (bb.Min.X * scale), (int) (bb.Min.Y * scale));
                this.Max = new Vector2I((int) (bb.Max.X * scale), (int) (bb.Max.Y * scale));
            }

            public Box2I(Vector2I min, Vector2I max)
            {
                this.Min = min;
                this.Max = max;
            }

            public void Intersect(ref MyHeightmapFace.Box2I other)
            {
                this.Min.X = Math.Max(this.Min.X, other.Min.X);
                this.Min.Y = Math.Max(this.Min.Y, other.Min.Y);
                this.Max.X = Math.Min(this.Max.X, other.Max.X);
                this.Max.Y = Math.Min(this.Max.Y, other.Max.Y);
            }

            public override string ToString() => 
                $"[({this.Min}), ({this.Max})]";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HeightmapLevel
        {
            public MyHeightmapFace.HeightmapNode[] Nodes;
            private uint m_res;
            public float Recip;
            public uint Res
            {
                get => 
                    this.m_res;
                set
                {
                    this.m_res = value;
                    this.Recip = 1f / ((float) this.m_res);
                }
            }
            public ContainmentType Intersect(int x, int y, ref BoundingBox query) => 
                this.Nodes[(int) ((IntPtr) ((y * this.Res) + x))].Intersect(ref query);

            public bool IsCellContained(int x, int y, ref BoundingBox box)
            {
                Vector2 vector = new Vector2((float) x, (float) y) * this.Recip;
                Vector2 vector2 = vector + this.Recip;
                return ((box.Min.X <= vector.X) && ((box.Min.Y <= vector.Y) && ((box.Max.X >= vector2.X) && (box.Max.Y >= vector2.Y))));
            }

            public bool IsCellNotContained(int x, int y, ref BoundingBox box)
            {
                Vector2 vector = new Vector2((float) x, (float) y) * this.Recip;
                Vector2 vector2 = vector + this.Recip;
                return ((box.Min.X > vector.X) || ((box.Min.Y > vector.Y) || ((box.Max.X < vector2.X) || (box.Max.Y > vector2.Y))));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HeightmapNode
        {
            public static readonly int HEIGHTMAP_BRANCH_FACTOR;
            public static readonly int HEIGHTMAP_LEAF_SIZE;
            public float Min;
            public float Max;
            internal ContainmentType Intersect(ref BoundingBox query) => 
                ((query.Min.Z <= this.Max) ? ((query.Max.Z >= this.Min) ? ContainmentType.Intersects : ContainmentType.Contains) : ContainmentType.Disjoint);

            static HeightmapNode()
            {
                HEIGHTMAP_BRANCH_FACTOR = 4;
                HEIGHTMAP_LEAF_SIZE = 8;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SEntry
        {
            public MyHeightmapFace.Box2I Bounds;
            public Vector2I Next;
            public ContainmentType Result;
            public ContainmentType Intersection;
            public uint Level;
            public bool Continue;
            public SEntry(ref BoundingBox query, uint res, Vector2I cell, ContainmentType result, uint level)
            {
                MyHeightmapFace.Box2I boxi = new MyHeightmapFace.Box2I(ref query, res);
                cell *= MyHeightmapFace.HeightmapNode.HEIGHTMAP_BRANCH_FACTOR;
                MyHeightmapFace.Box2I other = new MyHeightmapFace.Box2I(cell, (cell + MyHeightmapFace.HeightmapNode.HEIGHTMAP_BRANCH_FACTOR) - 1);
                boxi.Intersect(ref other);
                this.Bounds = boxi;
                this.Next = boxi.Min;
                this.Level = level;
                this.Result = result;
                this.Intersection = result;
                this.Continue = false;
            }
        }
    }
}

