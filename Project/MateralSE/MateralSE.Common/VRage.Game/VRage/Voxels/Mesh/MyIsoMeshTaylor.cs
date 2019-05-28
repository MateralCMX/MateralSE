namespace VRage.Voxels.Mesh
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Voxels;
    using VRage.Library.Collections;
    using VRage.Voxels;
    using VRage.Voxels.DualContouring;
    using VRage.Voxels.Sewing;
    using VRageMath;

    public class MyIsoMeshTaylor
    {
        [ThreadStatic]
        private static MyIsoMeshTaylor m_instance;
        [ThreadStatic]
        private static VrTailor m_nativeInstance;
        private static readonly Vector3I[] Axes = new Vector3I[] { new Vector3I(0, 1, 2), new Vector3I(2, 0, 1), new Vector3I(1, 2, 0) };
        private static readonly Vector3I[] InverseAxes = new Vector3I[] { new Vector3I(0, 1, 2), new Vector3I(1, 2, 0), new Vector3I(2, 0, 1) };
        private static readonly int[] FaceOffsets = new int[] { 1, 2, 4 };
        private Vector3I m_startOffset;
        private VertexGenerator m_generator;
        private int m_minRelativeLod;
        private Dictionary<Vx, ushort> m_addedVertices = new Dictionary<Vx, ushort>(Vx.Comparer);
        private Vector3I m_min;
        private Vector3I m_max;
        private List<MyVoxelQuad> m_tmpQuads = new List<MyVoxelQuad>(3);
        private int[] m_cornerIndices = new int[8];
        private ushort[] m_cornerOffsets = new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        private Vx[] m_buffer;
        private Vector3I m_maxes;
        private Vector3I m_bufferMin;
        private int m_coordinateIndex;

        public MyIsoMeshTaylor()
        {
            this.m_generator = new VertexGenerator(this);
        }

        private BoundingBoxI BoundsInLod(int index)
        {
            MyIsoMesh mesh = this.Meshes[index].Mesh;
            int num = mesh.Lod - this.Lod;
            return new BoundingBoxI(mesh.CellStart << num, mesh.CellEnd << num);
        }

        private void CalculateRange(ref BoundingBoxI? range)
        {
            if (range != 0)
            {
                this.m_min = range.Value.Min;
                this.m_max = range.Value.Max;
            }
            else
            {
                int lod = this.Meshes[0].Mesh.Lod;
                if (lod <= this.Lod)
                {
                    this.m_min = Vector3I.Zero;
                    this.m_max = this.Meshes[0].Mesh.Size - 1;
                }
                else
                {
                    BoundingBoxI xi = this.BoundsInLod(0);
                    Vector3I min = xi.Min;
                    if ((this.Meshes[1] != null) && (this.Meshes[1].Mesh.Lod < lod))
                    {
                        BoundingBoxI xi2 = this.BoundsInLod(1);
                        xi.Min.Y = Math.Max(xi.Min.Y, xi2.Min.Y);
                        xi.Min.Z = Math.Max(xi.Min.Z, xi2.Min.Z);
                        xi.Max.Y = Math.Min(xi.Max.Y, xi2.Max.Y);
                        xi.Max.Z = Math.Min(xi.Max.Z, xi2.Max.Z);
                    }
                    if ((this.Meshes[2] != null) && (this.Meshes[2].Mesh.Lod < lod))
                    {
                        BoundingBoxI xi3 = this.BoundsInLod(2);
                        xi.Min.X = Math.Max(xi.Min.X, xi3.Min.X);
                        xi.Min.Z = Math.Max(xi.Min.Z, xi3.Min.Z);
                        xi.Max.X = Math.Min(xi.Max.X, xi3.Max.X);
                        xi.Max.Z = Math.Min(xi.Max.Z, xi3.Max.Z);
                    }
                    if ((this.Meshes[4] != null) && (this.Meshes[4].Mesh.Lod < lod))
                    {
                        BoundingBoxI xi4 = this.BoundsInLod(4);
                        xi.Min.X = Math.Max(xi.Min.X, xi4.Min.X);
                        xi.Min.Y = Math.Max(xi.Min.Y, xi4.Min.Y);
                        xi.Max.X = Math.Min(xi.Max.X, xi4.Max.X);
                        xi.Max.Y = Math.Min(xi.Max.Y, xi4.Max.Y);
                    }
                    if ((this.Meshes[3] != null) && (this.Meshes[3].Mesh.Lod < lod))
                    {
                        BoundingBoxI xi5 = this.BoundsInLod(3);
                        xi.Max.X = Math.Min(xi.Max.X, xi5.Min.X);
                        xi.Max.Y = Math.Min(xi.Max.Y, xi5.Min.Y);
                        xi.Min.Z = Math.Max(xi.Min.Z, xi5.Min.Z);
                        xi.Max.Z = Math.Min(xi.Max.Z, xi5.Max.Z);
                    }
                    if ((this.Meshes[5] != null) && (this.Meshes[5].Mesh.Lod < lod))
                    {
                        BoundingBoxI xi6 = this.BoundsInLod(5);
                        xi.Max.X = Math.Min(xi.Max.X, xi6.Min.X);
                        xi.Max.Z = Math.Min(xi.Max.Z, xi6.Min.Z);
                        xi.Min.Y = Math.Max(xi.Min.Y, xi6.Min.Y);
                        xi.Max.Y = Math.Min(xi.Max.Y, xi6.Max.Y);
                    }
                    if ((this.Meshes[6] != null) && (this.Meshes[6].Mesh.Lod < lod))
                    {
                        BoundingBoxI xi7 = this.BoundsInLod(6);
                        xi.Max.Y = Math.Min(xi.Max.Y, xi7.Min.Y);
                        xi.Max.Z = Math.Min(xi.Max.Z, xi7.Min.Z);
                        xi.Min.X = Math.Max(xi.Min.X, xi7.Min.X);
                        xi.Max.X = Math.Min(xi.Max.X, xi7.Max.X);
                    }
                    if ((this.Meshes[7] != null) && (this.Meshes[7].Mesh.Lod < lod))
                    {
                        BoundingBoxI xi8 = this.BoundsInLod(7);
                        xi.Max.X = Math.Min(xi.Max.X, xi8.Min.X);
                        xi.Max.Y = Math.Min(xi.Max.Y, xi8.Min.Y);
                        xi.Max.Z = Math.Min(xi.Max.Z, xi8.Min.Z);
                    }
                    Vector3I size = xi.Size;
                    if (size.Size == 0)
                    {
                        Debugger.Break();
                    }
                    if (((size.X != size.Y) && (size.Y != size.Z)) && (size.X != size.Z))
                    {
                        Debugger.Break();
                    }
                    int num2 = this.Meshes[0].Mesh.Lod - this.Lod;
                    this.m_min = (xi.Min - min) >> num2;
                    this.m_max = (xi.Max - min) >> num2;
                }
            }
        }

        public static bool CheckVicinity(MyIsoMeshStitch[] meshes)
        {
            int num = (from x in meshes
                where x != null
                select x).Min<MyIsoMeshStitch>(x => x.Mesh.Lod);
            Vector3I vectori = meshes[0].Mesh.CellEnd << (meshes[0].Mesh.Lod - num);
            for (int i = 1; i < 8; i++)
            {
                if ((meshes[i] != null) && !ReferenceEquals(meshes[i], meshes[0]))
                {
                    Vector3I vectori2 = meshes[i].Mesh.CellStart << (meshes[i].Mesh.Lod - num);
                    if (((vectori.X != vectori2.X) && (vectori.Y != vectori2.Y)) && (vectori.Z != vectori2.Z))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool CheckVicinity(VrSewGuide[] meshes)
        {
            int num = (from x in meshes
                where x != null
                select x).Min<VrSewGuide>(x => x.Lod);
            Vector3I vectori = meshes[0].End << (meshes[0].Lod - num);
            for (int i = 1; i < 8; i++)
            {
                if ((meshes[i] != null) && !ReferenceEquals(meshes[i], meshes[0]))
                {
                    Vector3I vectori2 = meshes[i].Start << (meshes[i].Lod - num);
                    if (((vectori.X != vectori2.X) && (vectori.Y != vectori2.Y)) && (vectori.Z != vectori2.Z))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void ClearBuffer(int start = 0)
        {
            for (int i = start; i < this.m_buffer.Length; i++)
            {
                this.m_buffer[i] = Vx.Invalid;
            }
        }

        private unsafe bool CollectCorner()
        {
            ushort num;
            MyIsoMeshStitch mesh = this.Meshes[0];
            MyIsoMeshStitch stitch2 = this.Meshes[1];
            MyIsoMeshStitch stitch3 = this.Meshes[2];
            MyIsoMeshStitch stitch4 = this.Meshes[3];
            MyIsoMeshStitch stitch5 = this.Meshes[4];
            MyIsoMeshStitch stitch6 = this.Meshes[5];
            MyIsoMeshStitch stitch7 = this.Meshes[6];
            MyIsoMeshStitch stitch8 = this.Meshes[7];
            Vector3I min = (this.m_max << this.m_minRelativeLod) - 1;
            this.ResizeBuffer(min, (Vector3I) (min + 1), 0);
            Vector3I coord = min;
            if (this.TryGetVertex(mesh, coord, out num))
            {
                this.m_buffer[0] = new Vx(0, num);
            }
            this.m_buffer[0].OverIso = this.IsContentOverIso(mesh, coord);
            int* numPtr1 = (int*) ref coord.X;
            numPtr1[0]++;
            if (this.TryGetVertex(stitch2, coord, out num))
            {
                this.m_buffer[1] = new Vx(1, num);
            }
            this.m_buffer[1].OverIso = this.IsContentOverIso(stitch2, coord);
            int* numPtr2 = (int*) ref coord.Y;
            numPtr2[0]++;
            if (this.TryGetVertex(stitch4, coord, out num))
            {
                this.m_buffer[3] = new Vx(3, num);
            }
            this.m_buffer[3].OverIso = this.IsContentOverIso(stitch4, coord);
            int* numPtr3 = (int*) ref coord.X;
            numPtr3[0]--;
            if (this.TryGetVertex(stitch3, coord, out num))
            {
                this.m_buffer[2] = new Vx(2, num);
            }
            this.m_buffer[2].OverIso = this.IsContentOverIso(stitch3, coord);
            int* numPtr4 = (int*) ref coord.Z;
            numPtr4[0]++;
            if (this.TryGetVertex(stitch7, coord, out num))
            {
                this.m_buffer[6] = new Vx(6, num);
            }
            this.m_buffer[6].OverIso = this.IsContentOverIso(stitch7, coord);
            int* numPtr5 = (int*) ref coord.X;
            numPtr5[0]++;
            if (this.TryGetVertex(stitch8, coord, out num))
            {
                this.m_buffer[7] = new Vx(7, num);
            }
            this.m_buffer[7].OverIso = this.IsContentOverIso(stitch8, coord);
            int* numPtr6 = (int*) ref coord.Y;
            numPtr6[0]--;
            if (this.TryGetVertex(stitch6, coord, out num))
            {
                this.m_buffer[5] = new Vx(5, num);
            }
            this.m_buffer[5].OverIso = this.IsContentOverIso(stitch6, coord);
            int* numPtr7 = (int*) ref coord.X;
            numPtr7[0]--;
            if (this.TryGetVertex(stitch5, coord, out num))
            {
                this.m_buffer[4] = new Vx(4, num);
            }
            this.m_buffer[4].OverIso = this.IsContentOverIso(stitch5, coord);
            return true;
        }

        private unsafe bool CollectEdge(int coordIndex)
        {
            int x = Axes[coordIndex].X;
            int y = Axes[coordIndex].Y;
            int z = Axes[coordIndex].Z;
            sbyte index = (sbyte) FaceOffsets[x];
            sbyte num5 = (sbyte) FaceOffsets[y];
            sbyte num6 = (sbyte) (index + num5);
            MyIsoMeshStitch objA = this.Meshes[0];
            MyIsoMeshStitch objB = this.Meshes[index];
            MyIsoMeshStitch stitch3 = this.Meshes[num5];
            MyIsoMeshStitch stitch4 = this.Meshes[num6];
            Vector3I vectori = (this.m_max << this.m_minRelativeLod) - 1;
            Vector3I vectori2 = this.m_min << this.m_minRelativeLod;
            if ((ReferenceEquals(objA, objB) && ReferenceEquals(objB, stitch3)) && ReferenceEquals(stitch3, stitch4))
            {
                return false;
            }
            this.ResizeBuffer(new Vector3I(vectori2[z], vectori[x], vectori[y]), new Vector3I(vectori[z], vectori[x] + 1, vectori[y] + 1), (coordIndex + 1) % 3);
            Vector3I coord = vectori;
            coord[z] = vectori2[z];
            for (int i = 0; coord[z] <= vectori[z]; i++)
            {
                ushort num8;
                int num9 = i;
                int num10 = i + this.m_maxes.X;
                int num11 = i + this.m_maxes.Y;
                int num12 = (i + this.m_maxes.X) + this.m_maxes.Y;
                if (this.TryGetVertex(objA, coord, out num8))
                {
                    this.m_buffer[num9] = new Vx(0, num8);
                }
                this.m_buffer[num9].OverIso = this.IsContentOverIso(objA, coord);
                int num13 = x;
                Vector3I* vectoriPtr1 = (Vector3I*) ref coord;
                vectoriPtr1[num13]++;
                if (this.TryGetVertex(objB, coord, out num8))
                {
                    this.m_buffer[num10] = new Vx(index, num8);
                }
                this.m_buffer[num10].OverIso = this.IsContentOverIso(objB, coord);
                int num14 = y;
                Vector3I* vectoriPtr2 = (Vector3I*) ref coord;
                vectoriPtr2[num14]++;
                if (this.TryGetVertex(stitch4, coord, out num8))
                {
                    this.m_buffer[num12] = new Vx(num6, num8);
                }
                this.m_buffer[num12].OverIso = this.IsContentOverIso(stitch4, coord);
                num13 = x;
                Vector3I* vectoriPtr3 = (Vector3I*) ref coord;
                vectoriPtr3[num13]--;
                if (this.TryGetVertex(stitch3, coord, out num8))
                {
                    this.m_buffer[num11] = new Vx(num5, num8);
                }
                this.m_buffer[num11].OverIso = this.IsContentOverIso(stitch3, coord);
                num14 = y;
                Vector3I* vectoriPtr4 = (Vector3I*) ref coord;
                vectoriPtr4[num14]--;
                num13 = z;
                Vector3I* vectoriPtr5 = (Vector3I*) ref coord;
                vectoriPtr5[num13]++;
            }
            return true;
        }

        private unsafe bool CollectFace(int coordIndex)
        {
            int x = Axes[coordIndex].X;
            int y = Axes[coordIndex].Y;
            int z = Axes[coordIndex].Z;
            sbyte index = (sbyte) FaceOffsets[z];
            MyIsoMeshStitch objA = this.Meshes[0];
            MyIsoMeshStitch objB = this.Meshes[index];
            if (ReferenceEquals(objA, objB))
            {
                return false;
            }
            Vector3I max = (this.m_max << this.m_minRelativeLod) - 1;
            Vector3I min = this.m_min << this.m_minRelativeLod;
            Vector3I* vectoriPtr1 = (Vector3I*) ref min;
            vectoriPtr1 = (Vector3I*) new Vector3I(min[x], min[y], max[z]);
            Vector3I* vectoriPtr2 = (Vector3I*) ref max;
            vectoriPtr2 = (Vector3I*) new Vector3I(max[x], max[y], max[z] + 1);
            this.ResizeBuffer(min, max, coordIndex);
            Vector3I coord = new Vector3I {
                [z] = min.Z
            };
            int num5 = 0;
            coord[y] = min.Y;
            while (coord[y] <= max.Y)
            {
                int num6 = 0;
                coord[x] = min.X;
                while (true)
                {
                    ushort num8;
                    int num10;
                    if (coord[x] > max.X)
                    {
                        num10 = y;
                        Vector3I* vectoriPtr5 = (Vector3I*) ref coord;
                        vectoriPtr5[num10]++;
                        num5 += this.m_maxes.X;
                        break;
                    }
                    int num7 = num6 + num5;
                    if (this.TryGetVertex(objA, coord, out num8))
                    {
                        this.m_buffer[num7] = new Vx(0, num8);
                    }
                    this.m_buffer[num7].OverIso = this.IsContentOverIso(objA, coord);
                    Vector3I vectori4 = coord;
                    num10 = z;
                    Vector3I* vectoriPtr3 = (Vector3I*) ref vectori4;
                    vectoriPtr3[num10]++;
                    int num9 = num7 + this.m_maxes.Y;
                    if (this.TryGetVertex(objB, vectori4, out num8))
                    {
                        this.m_buffer[num9] = new Vx(index, num8);
                    }
                    this.m_buffer[num9].OverIso = this.IsContentOverIso(objB, vectori4);
                    int num11 = x;
                    Vector3I* vectoriPtr4 = (Vector3I*) ref coord;
                    vectoriPtr4[num11]++;
                    num6++;
                }
            }
            return true;
        }

        private void GenerateQuads()
        {
            Vector3I vectori = new Vector3I(1, this.m_maxes.X, this.m_maxes.Y);
            Vector3I vectori2 = this.m_maxes - vectori;
            int[] cornerIndices = this.m_cornerIndices;
            List<MyVoxelQuad> tmpQuads = this.m_tmpQuads;
            int num = 0;
            while (num < vectori2.Z)
            {
                int num2 = num + this.m_maxes.Y;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= vectori2.Y)
                    {
                        num += vectori.Z;
                        break;
                    }
                    int num4 = num3 + this.m_maxes.X;
                    byte index = 0;
                    if (this.m_buffer[num3 + num].OverIso)
                    {
                        index = (byte) (index | 2);
                    }
                    if (this.m_buffer[num4 + num].OverIso)
                    {
                        index = (byte) (index | 8);
                    }
                    if (this.m_buffer[num3 + num2].OverIso)
                    {
                        index = (byte) (index | 0x20);
                    }
                    if (this.m_buffer[num4 + num2].OverIso)
                    {
                        index = (byte) (index | 0x80);
                    }
                    cornerIndices[1] = (ushort) (num3 + num);
                    cornerIndices[3] = (ushort) (num4 + num);
                    cornerIndices[5] = (ushort) (num3 + num2);
                    cornerIndices[7] = (ushort) (num4 + num2);
                    uint num7 = 0;
                    while (true)
                    {
                        if (num7 >= vectori2.X)
                        {
                            num3 += this.m_maxes.X;
                            break;
                        }
                        uint num6 = num7 + 1;
                        index = (byte) (((byte) (index >> 1)) & 0x55);
                        if (this.m_buffer[(int) ((IntPtr) ((num6 + num3) + num))].OverIso)
                        {
                            index = (byte) (index | 2);
                        }
                        if (this.m_buffer[(int) ((IntPtr) ((num6 + num4) + num))].OverIso)
                        {
                            index = (byte) (index | 8);
                        }
                        if (this.m_buffer[(int) ((IntPtr) ((num6 + num3) + num2))].OverIso)
                        {
                            index = (byte) (index | 0x20);
                        }
                        if (this.m_buffer[(int) ((IntPtr) ((num6 + num4) + num2))].OverIso)
                        {
                            index = (byte) (index | 0x80);
                        }
                        this.LeftShift(cornerIndices);
                        cornerIndices[1] = (ushort) ((num6 + num3) + num);
                        cornerIndices[3] = (ushort) ((num6 + num4) + num);
                        cornerIndices[5] = (ushort) ((num6 + num3) + num2);
                        cornerIndices[7] = (ushort) ((num6 + num4) + num2);
                        if (MyDualContouringMesher.EdgeTable[index] != 0)
                        {
                            MyDualContouringMesher.GenerateQuads(index, this.m_cornerOffsets, tmpQuads);
                            int num8 = 0;
                            while (true)
                            {
                                if (num8 >= tmpQuads.Count)
                                {
                                    tmpQuads.Clear();
                                    break;
                                }
                                MyVoxelQuad quad = tmpQuads[num8];
                                Vx vertex = this.m_buffer[cornerIndices[quad.V0]];
                                Vx vx2 = this.m_buffer[cornerIndices[quad.V1]];
                                Vx vx3 = this.m_buffer[cornerIndices[quad.V2]];
                                Vx vx4 = this.m_buffer[cornerIndices[quad.V3]];
                                bool flag = false;
                                if ((!vertex.Valid || (!vx2.Valid || !vx3.Valid)) || !vx4.Valid)
                                {
                                    Vector3I baseIndex = (Vector3I) (this.m_bufferMin + (new Vector3I((float) num7, (float) num3, (float) num) / vectori));
                                    this.m_generator.GenerateVertex(ref vertex, baseIndex, ref quad, 0);
                                    this.m_generator.GenerateVertex(ref vx2, baseIndex, ref quad, 1);
                                    this.m_generator.GenerateVertex(ref vx3, baseIndex, ref quad, 2);
                                    this.m_generator.GenerateVertex(ref vx4, baseIndex, ref quad, 3);
                                    flag = true;
                                }
                                if (((vertex.Mesh != -1) && ((vx2.Mesh != -1) && (vx3.Mesh != -1))) && (vx4.Mesh != -1))
                                {
                                    this.TranslateVertex(ref vertex);
                                    this.TranslateVertex(ref vx2);
                                    this.TranslateVertex(ref vx3);
                                    this.TranslateVertex(ref vx4);
                                    if (((vertex != vx2) && (vx2 != vx3)) && (vx3 != vertex))
                                    {
                                        this.Meshes[this.Target].WriteTriangle(vx2.Index, vx3.Index, vertex.Index);
                                    }
                                    if (((vertex != vx4) && (vx4 != vx3)) && (vx3 != vertex))
                                    {
                                        this.Meshes[this.Target].WriteTriangle(vx3.Index, vx4.Index, vertex.Index);
                                    }
                                    if (flag)
                                    {
                                        this.m_generator.RegisterConnections(vertex.Index, vx2.Index, vx3.Index);
                                        this.m_generator.RegisterConnections(vx4.Index, vx3.Index, vertex.Index);
                                    }
                                }
                                num8++;
                            }
                        }
                        num7 = num6;
                    }
                }
            }
        }

        private bool IsContentOverIso(MyIsoMeshStitch mesh, Vector3I pos)
        {
            byte num2;
            byte num3;
            if (mesh == null)
            {
                mesh = this.Meshes[0];
            }
            pos = (Vector3I) (pos + this.m_startOffset);
            int num = mesh.Mesh.Lod - this.Lod;
            if (num > 0)
            {
                pos = pos >> num;
            }
            pos -= mesh.Mesh.CellStart;
            mesh.SampleEdge(pos, out num2, out num3);
            return ((num3 - 0x80) >= 0);
        }

        private void LeftShift(int[] corners)
        {
            corners[0] = corners[1];
            corners[2] = corners[3];
            corners[4] = corners[5];
            corners[6] = corners[7];
        }

        private static Vector3 RemapVertex(MyIsoMesh src, MyIsoMesh target, ushort index) => 
            ((((src.Positions[index] * src.PositionScale) + src.PositionOffset) - target.PositionOffset) / target.PositionScale);

        private void ResizeBuffer(Vector3I min, Vector3I max, int coordinateIndex)
        {
            this.m_bufferMin = min;
            this.m_coordinateIndex = coordinateIndex;
            Vector3I vectori = (Vector3I) ((max - min) + 1);
            this.m_maxes.X = vectori.X;
            this.m_maxes.Y = vectori.Y * this.m_maxes.X;
            this.m_maxes.Z = vectori.Z * this.m_maxes.Y;
            if ((this.m_buffer == null) || (this.m_maxes.Z > this.m_buffer.Length))
            {
                this.m_buffer = new Vx[this.m_maxes.Z];
            }
            this.ClearBuffer(0);
        }

        public void Stitch(MyIsoMeshStitch[] meshes, int primary = 0, VrSewOperation operation = 0xfe, BoundingBoxI? range = new BoundingBoxI?())
        {
            if (meshes == null)
            {
                throw new ArgumentNullException("meshes");
            }
            if (meshes.Length != 8)
            {
                throw new ArgumentException("Expecting exactly 8 neighboring mesh references.", "meshes");
            }
            if (meshes[primary] == null)
            {
                throw new ArgumentException("Primary mesh cannot be null");
            }
            if (!CheckVicinity(meshes))
            {
                throw new ArgumentException("The meshes to be stitched do not line up!", "meshes");
            }
            this.Lod = 0x7fffffff;
            foreach (MyIsoMeshStitch stitch in meshes)
            {
                if (stitch != null)
                {
                    stitch.IndexEdges();
                    this.Lod = Math.Min(stitch.Mesh.Lod, this.Lod);
                }
            }
            this.m_minRelativeLod = meshes[0].Mesh.Lod - this.Lod;
            this.m_startOffset = meshes[0].Mesh.CellStart << this.m_minRelativeLod;
            this.Meshes = meshes;
            this.Target = primary;
            this.CalculateRange(ref range);
            Vector3I startOffset = this.m_startOffset;
            int num = this.Meshes[primary].Mesh.Lod - this.Lod;
            if (num > 0)
            {
                startOffset = startOffset >> num;
            }
            startOffset -= this.Meshes[primary].Mesh.CellStart;
            this.m_generator.Prepare(this.Meshes[primary], (sbyte) primary, startOffset);
            for (int i = 0; i < 3; i++)
            {
                if (operation.Contains(((VrSewOperation) ((byte) (1 << ((3 - i) & 0x1f))))) && this.CollectFace(i))
                {
                    this.GenerateQuads();
                }
            }
            for (int j = 0; j < 3; j++)
            {
                if (operation.Contains(((VrSewOperation) ((byte) (1 << ((j + 4) & 0x1f))))) && this.CollectEdge(j))
                {
                    this.GenerateQuads();
                }
            }
            if (operation.Contains(VrSewOperation.XYZ) && this.CollectCorner())
            {
                this.GenerateQuads();
            }
            this.m_generator.FinalizeGeneratedVertices();
            this.m_generator.Clear();
            this.m_addedVertices.Clear();
        }

        private void TranslateVertex(ref Vx vertex)
        {
            if (vertex.Mesh != this.Target)
            {
                ushort num;
                if (!this.m_addedVertices.TryGetValue(vertex, out num))
                {
                    MyIsoMesh target = this.Meshes[this.Target].Mesh;
                    ushort index = vertex.Index;
                    Vector3I cell = new Vector3I(-1);
                    Vector3 pos = RemapVertex(this.Meshes[vertex.Mesh].Mesh, target, index);
                    Vector3 normal = this.Meshes[vertex.Mesh].Mesh.Normals[index];
                    num = this.Meshes[this.Target].WriteVertex(ref cell, ref pos, ref normal, this.Meshes[vertex.Mesh].Mesh.Materials[index], this.Meshes[vertex.Mesh].Mesh.ColorShiftHSV[index]);
                    this.m_addedVertices[vertex] = num;
                }
                vertex = new Vx((sbyte) this.Target, num);
            }
        }

        private bool TryGetVertex(MyIsoMeshStitch mesh, Vector3I coord, out ushort index)
        {
            if (mesh == null)
            {
                index = 0;
                return false;
            }
            coord = (Vector3I) (coord + this.m_startOffset);
            int num = mesh.Mesh.Lod - this.Lod;
            if (num > 0)
            {
                coord = coord >> num;
            }
            coord -= mesh.Mesh.CellStart;
            return mesh.TryGetVertex(coord, out index);
        }

        public static MyIsoMeshTaylor Instance =>
            (m_instance ?? (m_instance = new MyIsoMeshTaylor()));

        public static VrTailor NativeInstance =>
            (m_nativeInstance ?? (m_nativeInstance = new VrTailor()));

        internal MyIsoMeshStitch[] Meshes { get; private set; }

        internal int Target { get; private set; }

        internal int Lod { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyIsoMeshTaylor.<>c <>9 = new MyIsoMeshTaylor.<>c();
            public static Func<MyIsoMeshStitch, bool> <>9__29_0;
            public static Func<MyIsoMeshStitch, int> <>9__29_1;
            public static Func<VrSewGuide, bool> <>9__30_0;
            public static Func<VrSewGuide, int> <>9__30_1;

            internal bool <CheckVicinity>b__29_0(MyIsoMeshStitch x) => 
                (x != null);

            internal int <CheckVicinity>b__29_1(MyIsoMeshStitch x) => 
                x.Mesh.Lod;

            internal bool <CheckVicinity>b__30_0(VrSewGuide x) => 
                (x != null);

            internal int <CheckVicinity>b__30_1(VrSewGuide x) => 
                x.Lod;
        }

        private class VertexGenerator
        {
            private readonly MyIsoMeshTaylor m_taylor;
            private MyIsoMeshStitch m_target;
            private sbyte m_targetIndex;
            private Vector3I m_targetOffset;
            private readonly Dictionary<Vector3I, ushort> m_createdVertices = new Dictionary<Vector3I, ushort>(Vector3I.Comparer);
            private readonly Dictionary<ushort, MyIsoMeshTaylor.Vx> m_generatedPairs = new Dictionary<ushort, MyIsoMeshTaylor.Vx>();
            private readonly MyHashSetDictionary<ushort, ushort> m_adjacentVertices = new MyHashSetDictionary<ushort, ushort>();
            private Vector3I[] m_cornerPositions = new Vector3I[] { new Vector3I(0, 0, 0), new Vector3I(1, 0, 0), new Vector3I(0, 1, 0), new Vector3I(1, 1, 0), new Vector3I(0, 0, 1), new Vector3I(1, 0, 1), new Vector3I(0, 1, 1), new Vector3I(1, 1, 1) };
            private HashSet<uint> m_queued = new HashSet<uint>();

            public VertexGenerator(MyIsoMeshTaylor taylor)
            {
                this.m_taylor = taylor;
            }

            public void Clear()
            {
                this.m_createdVertices.Clear();
                this.m_adjacentVertices.Clear();
                this.m_generatedPairs.Clear();
                this.m_queued.Clear();
            }

            private int CountTriangles(int corner, int index, ref MyVoxelQuad quad)
            {
                MyVoxelQuad quad2 = quad;
                quad2[index] = (ushort) corner;
                if (!this.GetBufferVertex(corner).Valid)
                {
                    return 0;
                }
                MyTuple<MyIsoMeshTaylor.Vx, MyIsoMeshTaylor.Vx, MyIsoMeshTaylor.Vx, MyIsoMeshTaylor.Vx> tuple = new MyTuple<MyIsoMeshTaylor.Vx, MyIsoMeshTaylor.Vx, MyIsoMeshTaylor.Vx, MyIsoMeshTaylor.Vx>(this.GetBufferVertex(quad2.V0), this.GetBufferVertex(quad2.V1), this.GetBufferVertex(quad2.V2), this.GetBufferVertex(quad2.V3));
                if (tuple.Item1 == tuple.Item3)
                {
                    return 0;
                }
                if (tuple.Item2 == tuple.Item4)
                {
                    return 1;
                }
                if ((tuple.Item2 == tuple.Item1) || (tuple.Item2 == tuple.Item3))
                {
                    return 1;
                }
                if ((tuple.Item4 == tuple.Item1) || (tuple.Item4 == tuple.Item3))
                {
                    return 1;
                }
                return 2;
            }

            public void FinalizeGeneratedVertices()
            {
                foreach (ushort num in this.m_createdVertices.Values)
                {
                    this.m_target.AddVertexToIndex(num);
                    if (this.IsGenerated(num))
                    {
                        this.FinalizeVertex(num);
                    }
                }
                this.m_queued.Clear();
            }

            private void FinalizeVertex(ushort vx)
            {
                HashSet<ushort> set;
                if (this.m_queued.Add(vx) && this.m_adjacentVertices.TryGet(vx, out set))
                {
                    List<Vector3> list;
                    Dictionary<byte, int> dictionary;
                    Vector3 vector;
                    Vector3 vector2;
                    MyIsoMeshTaylor.Vx vx2;
                    PoolManager.Get<List<Vector3>>(out list);
                    PoolManager.Get<Dictionary<byte, int>>(out dictionary);
                    foreach (ushort num3 in set)
                    {
                        if (!this.IsGenerated(num3))
                        {
                            int num4;
                            list.Add(this.m_target.Mesh.Positions[num3]);
                            dictionary.TryGetValue(this.m_target.Mesh.Materials[num3], out num4);
                            dictionary[this.m_target.Mesh.Materials[num3]] = num4 + 1;
                        }
                    }
                    if (this.FitPosition(list, out vector, out vector2))
                    {
                        this.m_target.Mesh.Positions[vx] = vector;
                        this.m_target.Mesh.Normals[vx] = vector2;
                    }
                    int num = 0;
                    byte key = 0;
                    foreach (KeyValuePair<byte, int> pair in dictionary)
                    {
                        if (pair.Value > num)
                        {
                            key = pair.Key;
                            num = pair.Value;
                        }
                    }
                    this.m_target.Mesh.Materials[vx] = key;
                    if (this.m_generatedPairs.TryGetValue(vx, out vx2))
                    {
                        MyIsoMesh target = this.m_taylor.Meshes[vx2.Mesh].Mesh;
                        vector = MyIsoMeshTaylor.RemapVertex(this.m_target.Mesh, target, vx);
                        vector2 = this.m_target.Mesh.Normals[vx];
                        target.Positions[vx2.Index] = vector;
                        target.Normals[vx2.Index] = vector2;
                        target.Materials[vx2.Index] = key;
                    }
                    PoolManager.Return<List<Vector3>>(ref list);
                    PoolManager.Return<Dictionary<byte, int>>(ref dictionary);
                }
            }

            private MyIsoMeshTaylor.Vx FindGoodNeighbour(int index, ref MyVoxelQuad quad)
            {
                ushort num1 = quad[index];
                int corner = num1 ^ 1;
                int num2 = num1 ^ 2;
                int num3 = num1 ^ 4;
                int num4 = this.CountTriangles(corner, index, ref quad);
                int num5 = this.CountTriangles(num2, index, ref quad);
                int num6 = this.CountTriangles(num3, index, ref quad);
                if (((num4 == num5) && (num5 == num6)) && (num4 == 0))
                {
                    return MyIsoMeshTaylor.Vx.Invalid;
                }
                int cornerIndex = (num4 <= num5) ? ((num5 <= num6) ? num3 : num2) : ((num4 <= num6) ? num3 : corner);
                return this.GetBufferVertex(cornerIndex);
            }

            private bool FitPosition(List<Vector3> positions, out Vector3 pos, out Vector3 normal)
            {
                Vector3 zero = Vector3.Zero;
                if (positions.Count < 3)
                {
                    pos = new Vector3();
                    normal = new Vector3();
                    return false;
                }
                foreach (Vector3 vector3 in positions)
                {
                    zero += vector3;
                }
                Vector3 vector2 = zero / ((float) positions.Count);
                float num = 0f;
                float num2 = 0f;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = 0f;
                float num6 = 0f;
                using (List<Vector3>.Enumerator enumerator = positions.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Vector3 vector4 = enumerator.Current - vector2;
                        num += vector4.X * vector4.X;
                        num2 += vector4.X * vector4.Y;
                        num3 += vector4.X * vector4.Z;
                        num4 += vector4.Y * vector4.Y;
                        num5 += vector4.Y * vector4.Z;
                        num6 += vector4.Z * vector4.Z;
                    }
                }
                float num7 = (num4 * num6) - (num5 * num5);
                float num8 = (num * num6) - (num3 * num3);
                float num9 = (num * num4) - (num2 * num2);
                float num10 = num7;
                int num11 = 0;
                if (num8 > num10)
                {
                    num11 = 1;
                    num10 = num8;
                }
                if (num9 > num10)
                {
                    num11 = 2;
                    num10 = num9;
                }
                if (num10 < 0.0001f)
                {
                    pos = new Vector3();
                    normal = new Vector3();
                    return false;
                }
                switch (num11)
                {
                    case 1:
                    {
                        float x = ((num5 * num3) - (num2 * num6)) / num8;
                        normal = new Vector3(x, 1f, ((num2 * num3) - (num5 * num)) / num8);
                        break;
                    }
                    case 2:
                    {
                        float x = ((num5 * num2) - (num3 * num4)) / num9;
                        normal = new Vector3(x, ((num3 * num2) - (num5 * num)) / num9, 1f);
                        break;
                    }
                    default:
                    {
                        float y = ((num3 * num5) - (num2 * num6)) / num7;
                        normal = new Vector3(1f, y, ((num2 * num5) - (num3 * num4)) / num7);
                        break;
                    }
                }
                normal.Normalize();
                pos = vector2;
                return true;
            }

            public unsafe void GenerateVertex(ref MyIsoMeshTaylor.Vx vertex, Vector3I baseIndex, ref MyVoxelQuad quad, int index)
            {
                if (!vertex.Valid)
                {
                    ushort num2;
                    int coordinateIndex = this.m_taylor.m_coordinateIndex;
                    Vector3I vectori = MyIsoMeshTaylor.InverseAxes[coordinateIndex];
                    Vector3I vectori2 = this.m_cornerPositions[quad[index]];
                    Vector3I* vectoriPtr1 = (Vector3I*) ref vectori2;
                    vectoriPtr1 = (Vector3I*) new Vector3I(vectori2[vectori.X], vectori2[vectori.Y], vectori2[vectori.Z]);
                    baseIndex = new Vector3I(baseIndex[vectori.X], baseIndex[vectori.Y], baseIndex[vectori.Z]);
                    Vector3I key = (Vector3I) (baseIndex + vectori2);
                    if (!this.m_createdVertices.TryGetValue(key, out num2))
                    {
                        int num3 = this.m_target.Mesh.Lod - this.m_taylor.Lod;
                        Vector3I cell = key;
                        if (num3 > 0)
                        {
                            cell = cell >> num3;
                        }
                        cell = (Vector3I) (cell + this.m_targetOffset);
                        MyIsoMeshTaylor.Vx vx = this.FindGoodNeighbour(index, ref quad);
                        if (vx.Valid)
                        {
                            this.m_taylor.TranslateVertex(ref vx);
                            this.m_createdVertices[key] = vx.Index;
                            vertex = vx;
                            return;
                        }
                        Vector3 vector = cell + 0.5f;
                        Vector3 normal = Vector3.Normalize(vector);
                        if (!key.IsInsideInclusiveEnd(Vector3I.Zero, this.m_target.Mesh.Size - 2))
                        {
                            sbyte num4 = (sbyte) Vector3I.Dot(Vector3I.Sign(Vector3I.Max((Vector3I) ((key - this.m_target.Mesh.Size) + 2), Vector3I.Zero)), new Vector3I(1, 2, 4));
                            MyIsoMeshStitch stitch1 = this.m_taylor.Meshes[num4];
                        }
                        num2 = this.m_target.WriteVertex(ref cell, ref vector, ref normal, 0xff, 0);
                        this.m_createdVertices[key] = num2;
                    }
                    vertex = new MyIsoMeshTaylor.Vx(this.m_targetIndex, num2);
                }
            }

            private MyIsoMeshTaylor.Vx GetBufferVertex(int cornerIndex) => 
                this.m_taylor.m_buffer[this.m_taylor.m_cornerIndices[cornerIndex]];

            public bool IsGenerated(ushort index) => 
                (this.m_target.Mesh.Materials[index] == 0xff);

            public void Prepare(MyIsoMeshStitch target, sbyte targetIndex, Vector3I targetOffset)
            {
                this.m_target = target;
                this.m_targetIndex = targetIndex;
                this.m_targetOffset = targetOffset;
            }

            public void RegisterConnections(ushort v0, ushort v1, ushort v2)
            {
                if (this.IsGenerated(v0))
                {
                    this.m_adjacentVertices.Add(v0, v1, v2);
                }
                if (this.IsGenerated(v1))
                {
                    this.m_adjacentVertices.Add(v1, v2, v0);
                }
                if (this.IsGenerated(v2))
                {
                    this.m_adjacentVertices.Add(v2, v0, v1);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Vx
        {
            public sbyte Mesh;
            public bool OverIso;
            public ushort Index;
            public static MyIsoMeshTaylor.Vx Invalid;
            public static readonly IEqualityComparer<MyIsoMeshTaylor.Vx> Comparer;
            public bool Valid =>
                (this.Mesh != -1);
            public Vx(sbyte mesh, int index)
            {
                this.Mesh = mesh;
                this.Index = (ushort) index;
                this.OverIso = false;
            }

            public static bool operator ==(MyIsoMeshTaylor.Vx left, MyIsoMeshTaylor.Vx right) => 
                left.Equals(right);

            public static bool operator !=(MyIsoMeshTaylor.Vx left, MyIsoMeshTaylor.Vx right) => 
                !(left == right);

            public bool Equals(MyIsoMeshTaylor.Vx other) => 
                ((this.Mesh == other.Mesh) && (this.Index == other.Index));

            public override bool Equals(object obj) => 
                ((obj != null) ? ((obj is MyIsoMeshTaylor.Vx) && this.Equals((MyIsoMeshTaylor.Vx) obj)) : false);

            public override int GetHashCode() => 
                ((this.Mesh.GetHashCode() * 0x18d) ^ this.Index.GetHashCode());

            public override string ToString() => 
                (this.Valid ? $"[M{this.Mesh}: {this.Index}" : "[Invalid]");

            static Vx()
            {
                Invalid = new MyIsoMeshTaylor.Vx(-1, 0);
                Comparer = new MeshIndexEqualityComparer();
            }
            private sealed class MeshIndexEqualityComparer : IEqualityComparer<MyIsoMeshTaylor.Vx>
            {
                public bool Equals(MyIsoMeshTaylor.Vx x, MyIsoMeshTaylor.Vx y) => 
                    ((x.Mesh == y.Mesh) && (x.Index == y.Index));

                public int GetHashCode(MyIsoMeshTaylor.Vx obj) => 
                    ((obj.Mesh.GetHashCode() * 0x18d) ^ obj.Index.GetHashCode());
            }
        }
    }
}

