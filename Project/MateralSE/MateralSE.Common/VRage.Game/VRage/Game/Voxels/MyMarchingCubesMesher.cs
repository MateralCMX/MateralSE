namespace VRage.Game.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyMarchingCubesMesher : IMyIsoMesher
    {
        private const int POLYCUBE_EDGES = 12;
        private readonly MyEdge[] m_edges = new MyEdge[12];
        private const int CELL_EDGES_SIZE = 90;
        private MyEdgeVertex[][][][] m_edgeVertex;
        private int m_edgeVertexCalcCounter;
        private readonly MyVoxelVertex[] m_resultVertices = new MyVoxelVertex[0x1e00];
        private int m_resultVerticesCounter;
        private readonly MyVoxelTriangle[] m_resultTriangles = new MyVoxelTriangle[0x6400];
        private int m_resultTrianglesCounter;
        private Vector3I m_polygCubes;
        private Vector3I m_voxelStart;
        private const int COPY_TABLE_SIZE = 11;
        private int m_temporaryVoxelsCounter;
        private readonly MyTemporaryVoxel[] m_temporaryVoxels = new MyTemporaryVoxel[0x33fe];
        private const int m_sX = 1;
        private const int m_sY = 11;
        private const int m_sZ = 0x79;
        private readonly MyStorageData m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
        private Vector3I m_sizeMinusOne;
        private float m_voxelSizeInMeters;
        private Vector3 m_originPosition;

        public MyMarchingCubesMesher()
        {
            for (int i = 0; i < this.m_edges.Length; i++)
            {
                this.m_edges[i] = new MyEdge();
            }
            for (int j = 0; j < this.m_temporaryVoxels.Length; j++)
            {
                this.m_temporaryVoxels[j] = new MyTemporaryVoxel();
            }
            this.m_edgeVertexCalcCounter = 0;
            this.m_edgeVertex = new MyEdgeVertex[90][][][];
            int index = 0;
            while (index < 90)
            {
                this.m_edgeVertex[index] = new MyEdgeVertex[90][][];
                int num4 = 0;
                while (true)
                {
                    if (num4 >= 90)
                    {
                        index++;
                        break;
                    }
                    this.m_edgeVertex[index][num4] = new MyEdgeVertex[90][];
                    int num5 = 0;
                    while (true)
                    {
                        if (num5 >= 90)
                        {
                            num4++;
                            break;
                        }
                        this.m_edgeVertex[index][num4][num5] = new MyEdgeVertex[3];
                        int num6 = 0;
                        while (true)
                        {
                            if (num6 >= 3)
                            {
                                num5++;
                                break;
                            }
                            this.m_edgeVertex[index][num4][num5][num6] = new MyEdgeVertex();
                            this.m_edgeVertex[index][num4][num5][num6].CalcCounter = 0;
                            num6++;
                        }
                    }
                }
            }
        }

        private void CalcPolygCubeSize(int lodIdx, Vector3I storageSize)
        {
            Vector3I vectori = storageSize >> lodIdx;
            this.m_polygCubes.X = ((this.m_voxelStart.X + 8) >= vectori.X) ? 8 : 9;
            this.m_polygCubes.Y = ((this.m_voxelStart.Y + 8) >= vectori.Y) ? 8 : 9;
            this.m_polygCubes.Z = ((this.m_voxelStart.Z + 8) >= vectori.Z) ? 8 : 9;
        }

        private void ComputeSizeAndOrigin(int lodIdx, Vector3I storageSize)
        {
            this.m_voxelSizeInMeters = 1f * (1 << (lodIdx & 0x1f));
            this.m_sizeMinusOne = (storageSize >> lodIdx) - 1;
            this.m_originPosition = (this.m_voxelStart * this.m_voxelSizeInMeters) + (0.5f * this.m_voxelSizeInMeters);
        }

        private Vector3I ComputeTemporaryVoxelData(MyStorageData cache, ref Vector3I coord0, int cubeIndex, int lod)
        {
            int index = (coord0.X + (coord0.Y * 11)) + (coord0.Z * 0x79);
            MyTemporaryVoxel temporaryVoxel = this.m_temporaryVoxels[index];
            MyTemporaryVoxel voxel2 = this.m_temporaryVoxels[index + 1];
            MyTemporaryVoxel voxel3 = this.m_temporaryVoxels[(index + 1) + 0x79];
            MyTemporaryVoxel voxel4 = this.m_temporaryVoxels[index + 0x79];
            MyTemporaryVoxel voxel5 = this.m_temporaryVoxels[index + 11];
            MyTemporaryVoxel voxel6 = this.m_temporaryVoxels[(index + 1) + 11];
            MyTemporaryVoxel voxel7 = this.m_temporaryVoxels[((index + 1) + 11) + 0x79];
            MyTemporaryVoxel voxel8 = this.m_temporaryVoxels[(index + 11) + 0x79];
            Vector3I coord = new Vector3I(coord0.X + 1, coord0.Y, coord0.Z);
            Vector3I vectori2 = new Vector3I(coord0.X + 1, coord0.Y, coord0.Z + 1);
            Vector3I vectori3 = new Vector3I(coord0.X, coord0.Y, coord0.Z + 1);
            Vector3I vectori4 = new Vector3I(coord0.X, coord0.Y + 1, coord0.Z);
            Vector3I vectori5 = new Vector3I(coord0.X + 1, coord0.Y + 1, coord0.Z);
            Vector3I vectori6 = new Vector3I(coord0.X + 1, coord0.Y + 1, coord0.Z + 1);
            Vector3I vectori7 = new Vector3I(coord0.X, coord0.Y + 1, coord0.Z + 1);
            Vector3I p = coord0;
            Vector3I vectori9 = coord;
            Vector3I vectori10 = vectori2;
            Vector3I vectori11 = vectori3;
            Vector3I vectori12 = vectori4;
            Vector3I vectori13 = vectori5;
            Vector3I vectori14 = vectori6;
            Vector3I vectori15 = vectori7;
            temporaryVoxel.IdxInCache = cache.ComputeLinear(ref p);
            voxel2.IdxInCache = cache.ComputeLinear(ref vectori9);
            voxel3.IdxInCache = cache.ComputeLinear(ref vectori10);
            voxel4.IdxInCache = cache.ComputeLinear(ref vectori11);
            voxel5.IdxInCache = cache.ComputeLinear(ref vectori12);
            voxel6.IdxInCache = cache.ComputeLinear(ref vectori13);
            voxel7.IdxInCache = cache.ComputeLinear(ref vectori14);
            voxel8.IdxInCache = cache.ComputeLinear(ref vectori15);
            temporaryVoxel.Position.X = (this.m_voxelStart.X + coord0.X) * this.m_voxelSizeInMeters;
            temporaryVoxel.Position.Y = (this.m_voxelStart.Y + coord0.Y) * this.m_voxelSizeInMeters;
            temporaryVoxel.Position.Z = (this.m_voxelStart.Z + coord0.Z) * this.m_voxelSizeInMeters;
            voxel2.Position.X = temporaryVoxel.Position.X + this.m_voxelSizeInMeters;
            voxel2.Position.Y = temporaryVoxel.Position.Y;
            voxel2.Position.Z = temporaryVoxel.Position.Z;
            voxel3.Position.X = temporaryVoxel.Position.X + this.m_voxelSizeInMeters;
            voxel3.Position.Y = temporaryVoxel.Position.Y;
            voxel3.Position.Z = temporaryVoxel.Position.Z + this.m_voxelSizeInMeters;
            voxel4.Position.X = temporaryVoxel.Position.X;
            voxel4.Position.Y = temporaryVoxel.Position.Y;
            voxel4.Position.Z = temporaryVoxel.Position.Z + this.m_voxelSizeInMeters;
            voxel5.Position.X = temporaryVoxel.Position.X;
            voxel5.Position.Y = temporaryVoxel.Position.Y + this.m_voxelSizeInMeters;
            voxel5.Position.Z = temporaryVoxel.Position.Z;
            voxel6.Position.X = temporaryVoxel.Position.X + this.m_voxelSizeInMeters;
            voxel6.Position.Y = temporaryVoxel.Position.Y + this.m_voxelSizeInMeters;
            voxel6.Position.Z = temporaryVoxel.Position.Z;
            voxel7.Position.X = temporaryVoxel.Position.X + this.m_voxelSizeInMeters;
            voxel7.Position.Y = temporaryVoxel.Position.Y + this.m_voxelSizeInMeters;
            voxel7.Position.Z = temporaryVoxel.Position.Z + this.m_voxelSizeInMeters;
            voxel8.Position.X = temporaryVoxel.Position.X;
            voxel8.Position.Y = temporaryVoxel.Position.Y + this.m_voxelSizeInMeters;
            voxel8.Position.Z = temporaryVoxel.Position.Z + this.m_voxelSizeInMeters;
            this.GetVoxelNormal(temporaryVoxel, ref coord0, ref p, temporaryVoxel);
            this.GetVoxelNormal(voxel2, ref coord, ref vectori9, temporaryVoxel);
            this.GetVoxelNormal(voxel3, ref vectori2, ref vectori10, temporaryVoxel);
            this.GetVoxelNormal(voxel4, ref vectori3, ref vectori11, temporaryVoxel);
            this.GetVoxelNormal(voxel5, ref vectori4, ref vectori12, temporaryVoxel);
            this.GetVoxelNormal(voxel6, ref vectori5, ref vectori13, temporaryVoxel);
            this.GetVoxelNormal(voxel7, ref vectori6, ref vectori14, temporaryVoxel);
            this.GetVoxelNormal(voxel8, ref vectori7, ref vectori15, temporaryVoxel);
            this.GetVoxelAmbient(temporaryVoxel, ref coord0, ref p);
            this.GetVoxelAmbient(voxel2, ref coord, ref vectori9);
            this.GetVoxelAmbient(voxel3, ref vectori2, ref vectori10);
            this.GetVoxelAmbient(voxel4, ref vectori3, ref vectori11);
            this.GetVoxelAmbient(voxel5, ref vectori4, ref vectori12);
            this.GetVoxelAmbient(voxel6, ref vectori5, ref vectori13);
            this.GetVoxelAmbient(voxel7, ref vectori6, ref vectori14);
            this.GetVoxelAmbient(voxel8, ref vectori7, ref vectori15);
            int num1 = MyMarchingCubesConstants.EdgeTable[cubeIndex];
            if ((num1 & 1) == 1)
            {
                this.GetVertexInterpolation(cache, temporaryVoxel, voxel2, 0);
            }
            int local1 = num1;
            if ((local1 & 2) == 2)
            {
                this.GetVertexInterpolation(cache, voxel2, voxel3, 1);
            }
            int local2 = local1;
            if ((local2 & 4) == 4)
            {
                this.GetVertexInterpolation(cache, voxel3, voxel4, 2);
            }
            int local3 = local2;
            if ((local3 & 8) == 8)
            {
                this.GetVertexInterpolation(cache, voxel4, temporaryVoxel, 3);
            }
            int local4 = local3;
            if ((local4 & 0x10) == 0x10)
            {
                this.GetVertexInterpolation(cache, voxel5, voxel6, 4);
            }
            int local5 = local4;
            if ((local5 & 0x20) == 0x20)
            {
                this.GetVertexInterpolation(cache, voxel6, voxel7, 5);
            }
            int local6 = local5;
            if ((local6 & 0x40) == 0x40)
            {
                this.GetVertexInterpolation(cache, voxel7, voxel8, 6);
            }
            int local7 = local6;
            if ((local7 & 0x80) == 0x80)
            {
                this.GetVertexInterpolation(cache, voxel8, voxel5, 7);
            }
            int local8 = local7;
            if ((local8 & 0x100) == 0x100)
            {
                this.GetVertexInterpolation(cache, temporaryVoxel, voxel5, 8);
            }
            int local9 = local8;
            if ((local9 & 0x200) == 0x200)
            {
                this.GetVertexInterpolation(cache, voxel2, voxel6, 9);
            }
            int local10 = local9;
            if ((local10 & 0x400) == 0x400)
            {
                this.GetVertexInterpolation(cache, voxel3, voxel7, 10);
            }
            if ((local10 & 0x800) == 0x800)
            {
                this.GetVertexInterpolation(cache, voxel4, voxel8, 11);
            }
            return p;
        }

        private unsafe Vector3 ComputeVertexNormal(ref Vector3 position)
        {
            Vector3 vector2;
            Vector3 vector = ((position - this.m_originPosition) / this.m_voxelSizeInMeters) + 1f;
            vector2.X = this.SampleContent(vector.X - 0.01f, vector.Y, vector.Z) - this.SampleContent(vector.X + 0.01f, vector.Y, vector.Z);
            vector2.Y = this.SampleContent(vector.X, vector.Y - 0.01f, vector.Z) - this.SampleContent(vector.X, vector.Y + 0.01f, vector.Z);
            vector2.Z = this.SampleContent(vector.X, vector.Y, vector.Z - 0.01f) - this.SampleContent(vector.X, vector.Y, vector.Z + 0.01f);
            Vector3* vectorPtr1 = (Vector3*) ref vector2;
            Vector3.Normalize(ref (Vector3) ref vectorPtr1, out vector2);
            return vector2;
        }

        private void CreateTriangles(ref Vector3I coord0, int cubeIndex, ref Vector3I tempVoxelCoord0)
        {
            MyVoxelVertex vertex = new MyVoxelVertex();
            Vector3I vectori = new Vector3I(coord0.X, coord0.Y, coord0.Z);
            for (int i = 0; MyMarchingCubesConstants.TriangleTable[cubeIndex, i] != -1; i += 3)
            {
                int index = MyMarchingCubesConstants.TriangleTable[cubeIndex, i];
                int num3 = MyMarchingCubesConstants.TriangleTable[cubeIndex, i + 1];
                int num4 = MyMarchingCubesConstants.TriangleTable[cubeIndex, i + 2];
                MyEdge edge = this.m_edges[index];
                MyEdge edge2 = this.m_edges[num3];
                MyEdge edge3 = this.m_edges[num4];
                Vector4I vectori2 = MyMarchingCubesConstants.EdgeConversion[index];
                Vector4I vectori3 = MyMarchingCubesConstants.EdgeConversion[num3];
                Vector4I vectori4 = MyMarchingCubesConstants.EdgeConversion[num4];
                MyEdgeVertex vertex2 = this.m_edgeVertex[vectori.X + vectori2.X][vectori.Y + vectori2.Y][vectori.Z + vectori2.Z][vectori2.W];
                MyEdgeVertex vertex3 = this.m_edgeVertex[vectori.X + vectori3.X][vectori.Y + vectori3.Y][vectori.Z + vectori3.Z][vectori3.W];
                MyEdgeVertex vertex4 = this.m_edgeVertex[vectori.X + vectori4.X][vectori.Y + vectori4.Y][vectori.Z + vectori4.Z][vectori4.W];
                MyVoxelVertex vertex5 = new MyVoxelVertex {
                    Position = edge.Position
                };
                MyVoxelVertex vertex6 = new MyVoxelVertex {
                    Position = edge2.Position
                };
                MyVoxelVertex vertex7 = new MyVoxelVertex {
                    Position = edge3.Position
                };
                if (!this.IsWrongTriangle(ref vertex5, ref vertex6, ref vertex7))
                {
                    if (vertex2.CalcCounter != this.m_edgeVertexCalcCounter)
                    {
                        vertex2.CalcCounter = this.m_edgeVertexCalcCounter;
                        vertex2.VertexIndex = (ushort) this.m_resultVerticesCounter;
                        vertex.Position = edge.Position;
                        vertex.Normal = edge.Normal;
                        vertex.Material = edge.Material;
                        this.m_resultVertices[this.m_resultVerticesCounter] = vertex;
                        this.m_resultVerticesCounter++;
                    }
                    if (vertex3.CalcCounter != this.m_edgeVertexCalcCounter)
                    {
                        vertex3.CalcCounter = this.m_edgeVertexCalcCounter;
                        vertex3.VertexIndex = (ushort) this.m_resultVerticesCounter;
                        vertex.Position = edge2.Position;
                        vertex.Normal = edge2.Normal;
                        vertex.Material = edge2.Material;
                        this.m_resultVertices[this.m_resultVerticesCounter] = vertex;
                        this.m_resultVerticesCounter++;
                    }
                    if (vertex4.CalcCounter != this.m_edgeVertexCalcCounter)
                    {
                        vertex4.CalcCounter = this.m_edgeVertexCalcCounter;
                        vertex4.VertexIndex = (ushort) this.m_resultVerticesCounter;
                        vertex.Position = edge3.Position;
                        vertex.Normal = edge3.Normal;
                        vertex.Material = edge3.Material;
                        vertex.Cell = coord0;
                        this.m_resultVertices[this.m_resultVerticesCounter] = vertex;
                        this.m_resultVerticesCounter++;
                    }
                    this.m_resultTriangles[this.m_resultTrianglesCounter].V0 = vertex2.VertexIndex;
                    this.m_resultTriangles[this.m_resultTrianglesCounter].V1 = vertex3.VertexIndex;
                    this.m_resultTriangles[this.m_resultTrianglesCounter].V2 = vertex4.VertexIndex;
                    this.m_resultTrianglesCounter++;
                }
            }
        }

        private void GetVertexInterpolation(MyStorageData cache, MyTemporaryVoxel inputVoxelA, MyTemporaryVoxel inputVoxelB, int edgeIndex)
        {
            MyEdge edge = this.m_edges[edgeIndex];
            byte num = cache.Content(inputVoxelA.IdxInCache);
            byte num2 = cache.Content(inputVoxelB.IdxInCache);
            byte num3 = cache.Material(inputVoxelA.IdxInCache);
            byte num4 = cache.Material(inputVoxelB.IdxInCache);
            if (Math.Abs((int) (0x7f - num)) < 1E-05f)
            {
                edge.Position = inputVoxelA.Position;
                edge.Normal = inputVoxelA.Normal;
                edge.Material = num3;
                edge.Ambient = inputVoxelA.Ambient;
            }
            else if (Math.Abs((int) (0x7f - num2)) < 1E-05f)
            {
                edge.Position = inputVoxelB.Position;
                edge.Normal = inputVoxelB.Normal;
                edge.Material = num4;
                edge.Ambient = inputVoxelB.Ambient;
            }
            else
            {
                float num5 = ((float) (0x7f - num)) / ((float) (num2 - num));
                edge.Position.X = inputVoxelA.Position.X + (num5 * (inputVoxelB.Position.X - inputVoxelA.Position.X));
                edge.Position.Y = inputVoxelA.Position.Y + (num5 * (inputVoxelB.Position.Y - inputVoxelA.Position.Y));
                edge.Position.Z = inputVoxelA.Position.Z + (num5 * (inputVoxelB.Position.Z - inputVoxelA.Position.Z));
                edge.Normal.X = inputVoxelA.Normal.X + (num5 * (inputVoxelB.Normal.X - inputVoxelA.Normal.X));
                edge.Normal.Y = inputVoxelA.Normal.Y + (num5 * (inputVoxelB.Normal.Y - inputVoxelA.Normal.Y));
                edge.Normal.Z = inputVoxelA.Normal.Z + (num5 * (inputVoxelB.Normal.Z - inputVoxelA.Normal.Z));
                edge.Normal = !MathHelper.IsZero(edge.Normal, 1E-05f) ? MyUtils.Normalize(edge.Normal) : inputVoxelA.Normal;
                if (MathHelper.IsZero(edge.Normal, 1E-05f))
                {
                    edge.Normal = inputVoxelA.Normal;
                }
                float num6 = ((float) num2) / (num + num2);
                edge.Material = (num6 <= 0.5f) ? num3 : num4;
                edge.Ambient = inputVoxelA.Ambient + (num6 * (inputVoxelB.Ambient - inputVoxelA.Ambient));
            }
        }

        private void GetVoxelAmbient(MyTemporaryVoxel temporaryVoxel, ref Vector3I coord, ref Vector3I tempVoxelCoord)
        {
            if (temporaryVoxel.Ambient_CalcCounter != this.m_temporaryVoxelsCounter)
            {
                MyStorageData cache = this.m_cache;
                float num = 0f;
                int num2 = -1;
                while (true)
                {
                    if (num2 > 1)
                    {
                        num = MathHelper.Clamp((float) (1f - (num / 6885f)), (float) 0.4f, (float) 0.9f);
                        temporaryVoxel.Ambient = num;
                        temporaryVoxel.Ambient_CalcCounter = this.m_temporaryVoxelsCounter;
                        break;
                    }
                    int num3 = -1;
                    while (true)
                    {
                        if (num3 > 1)
                        {
                            num2++;
                            break;
                        }
                        int num4 = -1;
                        while (true)
                        {
                            if (num4 > 1)
                            {
                                num3++;
                                break;
                            }
                            Vector3I vectori = new Vector3I((coord.X + num2) - 1, (coord.Y + num3) - 1, (coord.Z + num4) - 1);
                            if (((vectori.X >= 0) && ((vectori.X <= this.m_sizeMinusOne.X) && ((vectori.Y >= 0) && ((vectori.Y <= this.m_sizeMinusOne.Y) && (vectori.Z >= 0))))) && (vectori.Z <= this.m_sizeMinusOne.Z))
                            {
                                num += cache.Content(coord.X + num2, coord.Y + num3, coord.Z + num4);
                            }
                            num4++;
                        }
                    }
                }
            }
        }

        private byte GetVoxelContent(int x, int y, int z) => 
            this.m_cache.Content(x, y, z);

        private unsafe void GetVoxelNormal(MyTemporaryVoxel temporaryVoxel, ref Vector3I coord, ref Vector3I voxelCoord, MyTemporaryVoxel centerVoxel)
        {
            if (temporaryVoxel.Normal_CalcCounter != this.m_temporaryVoxelsCounter)
            {
                Vector3I result = coord - 1;
                Vector3I vectori2 = (Vector3I) (coord + 1);
                MyStorageData cache = this.m_cache;
                Vector3I vectori3 = cache.Size3D - 1;
                Vector3I* vectoriPtr1 = (Vector3I*) ref result;
                Vector3I.Max(ref (Vector3I) ref vectoriPtr1, ref Vector3I.Zero, out result);
                Vector3I* vectoriPtr2 = (Vector3I*) ref vectori2;
                Vector3I.Min(ref (Vector3I) ref vectoriPtr2, ref vectori3, out vectori2);
                Vector3 vec = new Vector3(((float) (cache.Content(result.X, coord.Y, coord.Z) - cache.Content(vectori2.X, coord.Y, coord.Z))) / 255f, ((float) (cache.Content(coord.X, result.Y, coord.Z) - cache.Content(coord.X, vectori2.Y, coord.Z))) / 255f, ((float) (cache.Content(coord.X, coord.Y, result.Z) - cache.Content(coord.X, coord.Y, vectori2.Z))) / 255f);
                if (vec.LengthSquared() <= 1E-06f)
                {
                    temporaryVoxel.Normal = centerVoxel.Normal;
                }
                else
                {
                    MyUtils.Normalize(ref vec, out temporaryVoxel.Normal);
                }
                temporaryVoxel.Normal_CalcCounter = this.m_temporaryVoxelsCounter;
            }
        }

        private bool IsWrongTriangle(ref MyVoxelVertex edge0, ref MyVoxelVertex edge1, ref MyVoxelVertex edge2) => 
            MyUtils.IsWrongTriangle(edge0.Position, edge1.Position, edge2.Position);

        public unsafe MyIsoMesh Precalc(IMyStorage storage, int lod, Vector3I voxelStart, Vector3I voxelEnd, MyStorageDataTypeFlags properties = 3, MyVoxelRequestFlags flags = 0)
        {
            this.m_resultVerticesCounter = 0;
            this.m_resultTrianglesCounter = 0;
            this.m_edgeVertexCalcCounter++;
            this.m_temporaryVoxelsCounter++;
            this.CalcPolygCubeSize(lod, storage.Size);
            this.m_voxelStart = voxelStart;
            Vector3I size = storage.Size;
            this.m_cache.Resize(voxelStart, voxelEnd);
            storage.ReadRange(this.m_cache, MyStorageDataTypeFlags.Content, lod, voxelStart, voxelEnd);
            if (!this.m_cache.ContainsIsoSurface())
            {
                return null;
            }
            storage.ReadRange(this.m_cache, MyStorageDataTypeFlags.Material, lod, voxelStart, voxelEnd);
            this.ComputeSizeAndOrigin(lod, storage.Size);
            Vector3I zero = Vector3I.Zero;
            Vector3I end = (voxelEnd - voxelStart) - 3;
            Vector3I vectori4 = zero;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref zero, ref end);
            while (iterator.IsValid())
            {
                int index = 0;
                if (this.m_cache.Content(vectori4.X, vectori4.Y, vectori4.Z) < 0x7f)
                {
                    index |= 1;
                }
                if (this.m_cache.Content(vectori4.X + 1, vectori4.Y, vectori4.Z) < 0x7f)
                {
                    index |= 2;
                }
                if (this.m_cache.Content(vectori4.X + 1, vectori4.Y, vectori4.Z + 1) < 0x7f)
                {
                    index |= 4;
                }
                if (this.m_cache.Content(vectori4.X, vectori4.Y, vectori4.Z + 1) < 0x7f)
                {
                    index |= 8;
                }
                if (this.m_cache.Content(vectori4.X, vectori4.Y + 1, vectori4.Z) < 0x7f)
                {
                    index |= 0x10;
                }
                if (this.m_cache.Content(vectori4.X + 1, vectori4.Y + 1, vectori4.Z) < 0x7f)
                {
                    index |= 0x20;
                }
                if (this.m_cache.Content(vectori4.X + 1, vectori4.Y + 1, vectori4.Z + 1) < 0x7f)
                {
                    index |= 0x40;
                }
                if (this.m_cache.Content(vectori4.X, vectori4.Y + 1, vectori4.Z + 1) < 0x7f)
                {
                    index |= 0x80;
                }
                if (MyMarchingCubesConstants.EdgeTable[index] != 0)
                {
                    Vector3I vectori5 = this.ComputeTemporaryVoxelData(this.m_cache, ref vectori4, index, lod);
                    this.CreateTriangles(ref vectori4, index, ref vectori5);
                }
                iterator.GetNext(out vectori4);
            }
            Vector3I vectori6 = this.m_cache.Size3D;
            Vector3I vectori = voxelStart - this.AffectedRangeOffset;
            MyIsoMesh mesh = new MyIsoMesh();
            for (int i = 0; i < this.m_resultVerticesCounter; i++)
            {
                Vector3 vector = (this.m_resultVertices[i].Position - (storage.Size / 2f)) / storage.Size;
                this.m_resultVertices[i].Position = vector;
            }
            for (int j = 0; j < this.m_resultVerticesCounter; j++)
            {
                mesh.WriteVertex(ref this.m_resultVertices[j].Cell, ref this.m_resultVertices[j].Position, ref this.m_resultVertices[j].Normal, (byte) this.m_resultVertices[j].Material, 0);
            }
            for (int k = 0; k < this.m_resultTrianglesCounter; k++)
            {
                mesh.WriteTriangle(this.m_resultTriangles[k].V0, this.m_resultTriangles[k].V1, this.m_resultTriangles[k].V2);
            }
            MyIsoMesh mesh2 = mesh;
            mesh2.PositionOffset = storage.Size / 2;
            mesh2.PositionScale = (Vector3) storage.Size;
            mesh2.CellStart = voxelStart;
            mesh2.CellEnd = voxelEnd;
            Vector3I[] internalArray = mesh2.Cells.GetInternalArray<Vector3I>();
            for (int m = 0; m < mesh2.VerticesCount; m++)
            {
                Vector3I* vectoriPtr1 = (Vector3I*) ref internalArray[m];
                vectoriPtr1[0] = (Vector3I) (vectoriPtr1[0] + vectori);
            }
            return mesh;
        }

        private float SampleContent(float x, float y, float z)
        {
            Vector3 vector = new Vector3(x, y, z);
            Vector3I vectori = Vector3I.Floor(vector);
            vector -= vectori;
            float num = this.m_cache.Content(vectori.X, vectori.Y, vectori.Z);
            float num2 = this.m_cache.Content(vectori.X + 1, vectori.Y, vectori.Z);
            float num3 = this.m_cache.Content(vectori.X, vectori.Y + 1, vectori.Z);
            float num4 = this.m_cache.Content(vectori.X + 1, vectori.Y + 1, vectori.Z);
            float num5 = this.m_cache.Content(vectori.X, vectori.Y, vectori.Z + 1);
            float num6 = this.m_cache.Content(vectori.X + 1, vectori.Y, vectori.Z + 1);
            float num7 = this.m_cache.Content(vectori.X, vectori.Y + 1, vectori.Z + 1);
            float num8 = this.m_cache.Content(vectori.X + 1, vectori.Y + 1, vectori.Z + 1);
            num += vector.X * (num2 - num);
            num3 += vector.X * (num4 - num3);
            num5 += vector.X * (num6 - num5);
            num7 += vector.X * (num8 - num7);
            num += vector.Y * (num3 - num);
            num5 += vector.Y * (num7 - num5);
            return (num + (vector.Z * (num5 - num)));
        }

        public int AffectedRangeOffset =>
            -1;

        public int AffectedRangeSizeChange =>
            3;

        public int InvalidatedRangeInflate =>
            2;

        public int VertexPositionRangeSizeChange =>
            0;

        public float VertexPositionOffsetChange =>
            0.5f;

        private class MyEdge
        {
            public Vector3 Position;
            public Vector3 Normal;
            public float Ambient;
            public byte Material;
        }

        private class MyEdgeVertex
        {
            public ushort VertexIndex;
            public int CalcCounter;
        }

        private class MyTemporaryVoxel
        {
            public int IdxInCache;
            public Vector3 Position;
            public Vector3 Normal;
            public float Ambient;
            public int Normal_CalcCounter;
            public int Ambient_CalcCounter;
        }
    }
}

