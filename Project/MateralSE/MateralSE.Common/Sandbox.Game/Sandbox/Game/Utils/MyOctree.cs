namespace Sandbox.Game.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;
    using VRageMath;

    internal class MyOctree
    {
        private const int NODE_COUNT = 0x49;
        private byte[] m_childEmpty = new byte[9];
        private short[] m_firstTriangleIndex = new short[0x49];
        private byte[] m_triangleCount = new byte[0x49];
        private Vector3 m_bbMin;
        private Vector3 m_bbInvScale;
        private const float CHILD_SIZE = 0.65f;
        private static readonly BoundingBox[] box = new BoundingBox[0x49];

        static MyOctree()
        {
            int index = 0;
            int num2 = 0;
            while (num2 < 1)
            {
                box[index].Min = Vector3.Zero;
                box[index].Max = Vector3.One;
                num2++;
                index++;
            }
            int num3 = 0;
            while (num3 < 8)
            {
                if ((num3 & 4) == 0)
                {
                    box[index].Min.Z = 0f;
                    box[index].Max.Z = 0.65f;
                }
                else
                {
                    box[index].Min.Z = 0.35f;
                    box[index].Max.Z = 1f;
                }
                if ((num3 & 2) == 0)
                {
                    box[index].Min.Y = 0f;
                    box[index].Max.Y = 0.65f;
                }
                else
                {
                    box[index].Min.Y = 0.35f;
                    box[index].Max.Y = 1f;
                }
                if ((num3 & 1) == 0)
                {
                    box[index].Min.X = 0f;
                    box[index].Max.X = 0.65f;
                }
                else
                {
                    box[index].Min.X = 0.35f;
                    box[index].Max.X = 1f;
                }
                num3++;
                index++;
            }
            int num4 = 0;
            while (num4 < 0x40)
            {
                if ((num4 & 0x20) == 0)
                {
                    box[index].Min.Z = 0f;
                    box[index].Max.Z = 0.65f;
                }
                else
                {
                    box[index].Min.Z = 0.35f;
                    box[index].Max.Z = 1f;
                }
                if ((num4 & 0x10) == 0)
                {
                    box[index].Min.Y = 0f;
                    box[index].Max.Y = 0.65f;
                }
                else
                {
                    box[index].Min.Y = 0.35f;
                    box[index].Max.Y = 1f;
                }
                if ((num4 & 8) == 0)
                {
                    box[index].Min.X = 0f;
                    box[index].Max.X = 0.65f;
                }
                else
                {
                    box[index].Min.X = 0.35f;
                    box[index].Max.X = 1f;
                }
                if ((num4 & 4) == 0)
                {
                    box[index].Max.Z = box[index].Min.Z + ((box[index].Max.Z - box[index].Min.Z) * 0.65f);
                }
                else
                {
                    box[index].Min.Z += (box[index].Max.Z - box[index].Min.Z) * 0.35f;
                }
                if ((num4 & 2) == 0)
                {
                    box[index].Max.Y = box[index].Min.Y + ((box[index].Max.Y - box[index].Min.Y) * 0.65f);
                }
                else
                {
                    box[index].Min.Y += (box[index].Max.Y - box[index].Min.Y) * 0.35f;
                }
                if ((num4 & 1) == 0)
                {
                    box[index].Max.X = box[index].Min.X + ((box[index].Max.X - box[index].Min.X) * 0.65f);
                }
                else
                {
                    box[index].Min.X += (box[index].Max.X - box[index].Min.X) * 0.35f;
                }
                num4++;
                index++;
            }
        }

        public void BoxQuery(ref BoundingBox bbox, List<int> triangleIndices)
        {
            bool flag;
            BoundingBox box = new BoundingBox((bbox.Min - this.m_bbMin) * this.m_bbInvScale, (bbox.Max - this.m_bbMin) * this.m_bbInvScale);
            MyOctree.box[0].Intersects(ref box, out flag);
            if (flag)
            {
                int num = 0;
                while (true)
                {
                    if (num >= this.m_triangleCount[0])
                    {
                        int index = 1;
                        for (int i = 1; index < 9; i = i << 1)
                        {
                            if ((this.m_childEmpty[0] & i) == 0)
                            {
                                MyOctree.box[index].Intersects(ref box, out flag);
                                if (flag)
                                {
                                    int num4 = 0;
                                    while (true)
                                    {
                                        if (num4 >= this.m_triangleCount[index])
                                        {
                                            int num5 = (index * 8) + 1;
                                            for (int j = 1; num5 < ((index * 8) + 9); j = j << 1)
                                            {
                                                if ((this.m_childEmpty[index] & j) == 0)
                                                {
                                                    MyOctree.box[num5].Intersects(ref box, out flag);
                                                    if (flag)
                                                    {
                                                        for (int k = 0; k < this.m_triangleCount[num5]; k++)
                                                        {
                                                            triangleIndices.Add(this.m_firstTriangleIndex[num5] + k);
                                                        }
                                                    }
                                                }
                                                num5++;
                                            }
                                            break;
                                        }
                                        triangleIndices.Add(this.m_firstTriangleIndex[index] + num4);
                                        num4++;
                                    }
                                }
                            }
                            index++;
                        }
                        break;
                    }
                    triangleIndices.Add(this.m_firstTriangleIndex[0] + num);
                    num++;
                }
            }
        }

        public void GetIntersectionWithLine(ref Ray ray, List<int> triangleIndices)
        {
            float? nullable;
            Ray ray2 = new Ray((ray.Position - this.m_bbMin) * this.m_bbInvScale, ray.Direction * this.m_bbInvScale);
            box[0].Intersects(ref ray2, out nullable);
            if (nullable != null)
            {
                int num = 0;
                while (true)
                {
                    if (num >= this.m_triangleCount[0])
                    {
                        int index = 1;
                        for (int i = 1; index < 9; i = i << 1)
                        {
                            if ((this.m_childEmpty[0] & i) == 0)
                            {
                                box[index].Intersects(ref ray2, out nullable);
                                if (nullable != null)
                                {
                                    int num4 = 0;
                                    while (true)
                                    {
                                        if (num4 >= this.m_triangleCount[index])
                                        {
                                            int num5 = (index * 8) + 1;
                                            for (int j = 1; num5 < ((index * 8) + 9); j = j << 1)
                                            {
                                                if ((this.m_childEmpty[index] & j) == 0)
                                                {
                                                    box[num5].Intersects(ref ray2, out nullable);
                                                    if (nullable != null)
                                                    {
                                                        for (int k = 0; k < this.m_triangleCount[num5]; k++)
                                                        {
                                                            triangleIndices.Add(this.m_firstTriangleIndex[num5] + k);
                                                        }
                                                    }
                                                }
                                                num5++;
                                            }
                                            break;
                                        }
                                        triangleIndices.Add(this.m_firstTriangleIndex[index] + num4);
                                        num4++;
                                    }
                                }
                            }
                            index++;
                        }
                        break;
                    }
                    triangleIndices.Add(this.m_firstTriangleIndex[0] + num);
                    num++;
                }
            }
        }

        private int GetNode(ref BoundingBox triangleAabb)
        {
            int num3;
            BoundingBox box = new BoundingBox((triangleAabb.Min - this.m_bbMin) * this.m_bbInvScale, (triangleAabb.Max - this.m_bbMin) * this.m_bbInvScale);
            int num = 0;
            int num2 = 0;
            goto TR_000D;
        TR_0001:
            num = num3;
            num2++;
        TR_000D:
            while (true)
            {
                if (num2 < 2)
                {
                    num3 = (num * 8) + 1;
                    if (box.Min.X > MyOctree.box[num3 + 1].Min.X)
                    {
                        num3++;
                    }
                    else if (box.Max.X >= MyOctree.box[num3].Max.X)
                    {
                        break;
                    }
                    if (box.Min.Y > MyOctree.box[num3 + 2].Min.Y)
                    {
                        num3 += 2;
                    }
                    else if (box.Max.Y >= MyOctree.box[num3].Max.Y)
                    {
                        break;
                    }
                    if (box.Min.Z <= MyOctree.box[num3 + 4].Min.Z)
                    {
                        if (box.Max.Z < MyOctree.box[num3].Max.Z)
                        {
                            goto TR_0001;
                        }
                    }
                    else
                    {
                        num3 += 4;
                        goto TR_0001;
                    }
                }
                break;
            }
            return num;
        }

        public unsafe void Init(Vector3[] positions, int vertexCount, MyVoxelTriangle[] triangles, int triangleCount, out MyVoxelTriangle[] sortedTriangles)
        {
            for (int i = 0; i < 0x49; i++)
            {
                this.m_firstTriangleIndex[i] = 0;
                this.m_triangleCount[i] = 0;
            }
            for (int j = 0; j < 9; j++)
            {
                this.m_childEmpty[j] = 0;
            }
            BoundingBox box = BoundingBox.CreateInvalid();
            for (int k = 0; k < vertexCount; k++)
            {
                box.Include(ref positions[k]);
            }
            this.m_bbMin = box.Min;
            Vector3 vector = box.Max - box.Min;
            this.m_bbInvScale = Vector3.One;
            if (vector.X > 1f)
            {
                this.m_bbInvScale.X = 1f / vector.X;
            }
            if (vector.Y > 1f)
            {
                this.m_bbInvScale.Y = 1f / vector.Y;
            }
            if (vector.Z > 1f)
            {
                this.m_bbInvScale.Z = 1f / vector.Z;
            }
            for (int m = 0; m < triangleCount; m++)
            {
                MyVoxelTriangle triangle = triangles[m];
                BoundingBox triangleAabb = BoundingBox.CreateInvalid();
                triangleAabb.Include(ref positions[triangle.V0], ref positions[triangle.V1], ref positions[triangle.V2]);
                byte* numPtr1 = (byte*) ref this.m_triangleCount[this.GetNode(ref triangleAabb)];
                byte num5 = numPtr1[0];
                numPtr1[0] = (byte) (num5 + 1);
            }
            this.m_firstTriangleIndex[0] = this.m_triangleCount[0];
            for (int n = 1; n < 0x49; n++)
            {
                this.m_firstTriangleIndex[n] = (short) (this.m_firstTriangleIndex[n - 1] + this.m_triangleCount[n]);
            }
            MyVoxelTriangle[] triangleArray = new MyVoxelTriangle[triangleCount];
            for (int num7 = 0; num7 < triangleCount; num7++)
            {
                MyVoxelTriangle triangle2 = triangles[num7];
                BoundingBox triangleAabb = BoundingBox.CreateInvalid();
                triangleAabb.Include(ref positions[triangle2.V0], ref positions[triangle2.V1], ref positions[triangle2.V2]);
                short* numPtr2 = (short*) ref this.m_firstTriangleIndex[this.GetNode(ref triangleAabb)];
                short index = (short) (numPtr2[0] - 1);
                numPtr2[0] = index;
                triangleArray[index] = triangle2;
            }
            sortedTriangles = triangleArray;
            for (int num9 = 0x48; num9 > 0; num9--)
            {
                if ((this.m_triangleCount[num9] == 0) && ((num9 > 8) || (this.m_childEmpty[num9] == 0xff)))
                {
                    byte* numPtr3 = (byte*) ref this.m_childEmpty[(num9 - 1) >> 3];
                    numPtr3[0] = (byte) (numPtr3[0] | ((byte) (1 << (((num9 - 1) & 7) & 0x1f))));
                }
            }
        }
    }
}

