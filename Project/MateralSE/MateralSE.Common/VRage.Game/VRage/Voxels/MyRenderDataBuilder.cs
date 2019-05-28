namespace VRage.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Utils;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageRender.Voxels;

    public class MyRenderDataBuilder
    {
        [ThreadStatic]
        private static MyRenderDataBuilder m_instance;
        private static readonly MyConcurrentPool<Part> m_partPool = new MyConcurrentPool<Part>(0, null, 0x2710, null);
        private readonly SortedDictionary<int, Part> m_parts = new SortedDictionary<int, Part>();

        public unsafe void Build(VrVoxelMesh mesh, out MyVoxelRenderCellData data, IMyVoxelRenderDataProcessorProvider dataProcessorProvider)
        {
            data = new MyVoxelRenderCellData();
            if (mesh.TriangleCount != 0)
            {
                VrVoxelVertex* vertices = mesh.Vertices;
                VrVoxelTriangle* triangles = mesh.Triangles;
                data.CellBounds = BoundingBox.CreateInvalid();
                int vertexCount = mesh.VertexCount;
                for (int i = 0; i < vertexCount; i++)
                {
                    data.CellBounds.Include(vertices[i].Position);
                }
                int triangleCount = mesh.TriangleCount;
                for (int j = 0; j < triangleCount; j++)
                {
                    Part part;
                    VrVoxelTriangle triangle = triangles[j];
                    MaterialTriple material = new MaterialTriple(ref (VrVoxelVertex) ref (vertices + triangle.V0), ref (VrVoxelVertex) ref (vertices + triangle.V1), ref (VrVoxelVertex) ref (vertices + triangle.V2));
                    if (!this.m_parts.TryGetValue((int) material, out part))
                    {
                        part = m_partPool.Get();
                        part.Init(material);
                        this.m_parts[(int) material] = part;
                    }
                    part.AddTriangle(triangle, vertices);
                }
                vertexCount = 0;
                triangleCount = 0;
                foreach (Part part2 in this.m_parts.Values)
                {
                    vertexCount += part2.Vertices.Count;
                    triangleCount += part2.Triangles.Count;
                }
                IMyVoxelRenderDataProcessor processor = dataProcessorProvider.GetRenderDataProcessor(vertexCount, triangleCount * 3, this.m_parts.Count);
                foreach (Part part3 in this.m_parts.Values)
                {
                    VrVoxelTriangle[] pinned triangleArray;
                    try
                    {
                        VrVoxelTriangle* trianglePtr2;
                        if (((triangleArray = part3.Triangles.GetInternalArray<VrVoxelTriangle>()) == null) || (triangleArray.Length == 0))
                        {
                            trianglePtr2 = null;
                        }
                        else
                        {
                            trianglePtr2 = triangleArray;
                        }
                        ushort* indices = (ushort*) trianglePtr2;
                        processor.AddPart(part3.Vertices, indices, part3.Triangles.Count * 3, (MyVoxelMaterialTriple) part3.Material);
                    }
                    finally
                    {
                        triangleArray = null;
                    }
                    part3.Clear();
                    m_partPool.Return(part3);
                }
                data.VertexCount = vertexCount;
                data.IndexCount = triangleCount * 3;
                processor.GetDataAndDispose(ref data);
                this.m_parts.Clear();
            }
        }

        public static MyRenderDataBuilder Instance =>
            (m_instance ?? (m_instance = new MyRenderDataBuilder()));

        [StructLayout(LayoutKind.Sequential)]
        private struct MaterialTriple
        {
            public readonly byte M0;
            public readonly byte M1;
            public readonly byte M2;
            public bool SingleMaterial =>
                (this.M1 == 0xff);
            public bool MultiMaterial =>
                (this.M1 != 0xff);
            public MaterialTriple(ref VrVoxelVertex v0, ref VrVoxelVertex v1, ref VrVoxelVertex v2)
            {
                this.M0 = v0.Material;
                this.M1 = v1.Material;
                this.M2 = v2.Material;
                if (this.M0 == this.M1)
                {
                    this.M1 = 0xff;
                }
                if (this.M0 == this.M2)
                {
                    this.M2 = 0xff;
                }
                if (this.M1 == this.M2)
                {
                    this.M2 = 0xff;
                }
                if (this.M0 > this.M1)
                {
                    MyUtils.Swap<byte>(ref this.M0, ref this.M1);
                }
                if (this.M1 > this.M2)
                {
                    MyUtils.Swap<byte>(ref this.M1, ref this.M2);
                }
                if (this.M0 > this.M1)
                {
                    MyUtils.Swap<byte>(ref this.M0, ref this.M1);
                }
            }

            public MaterialTriple(byte m0, byte m1, byte m2)
            {
                this.M0 = m0;
                this.M1 = m1;
                this.M2 = m2;
            }

            public static implicit operator int(MyRenderDataBuilder.MaterialTriple triple) => 
                -((triple.M0 | (triple.M1 << 8)) | (triple.M2 << 0x10));

            public static implicit operator MyRenderDataBuilder.MaterialTriple(int packed)
            {
                packed = -packed;
                return new MyRenderDataBuilder.MaterialTriple((byte) (packed & 0xff), (byte) ((packed >> 8) & 0xff), (byte) ((packed >> 0x10) & 0xff));
            }

            public static implicit operator MyVoxelMaterialTriple(MyRenderDataBuilder.MaterialTriple triple) => 
                new MyVoxelMaterialTriple(triple.M0, triple.M1, triple.M2);

            public override string ToString() => 
                (!this.SingleMaterial ? $"M{{{this.M0}, {this.M1}, {this.M2}}}" : $"S{{{this.M0}}}");
        }

        private class Part
        {
            public MyRenderDataBuilder.MaterialTriple Material;
            private readonly Dictionary<ushort, ushort> m_indexMap = new Dictionary<ushort, ushort>();
            public readonly List<MyVertexFormatVoxelSingleData> Vertices = new List<MyVertexFormatVoxelSingleData>();
            public readonly List<VrVoxelTriangle> Triangles = new List<VrVoxelTriangle>();

            public unsafe void AddTriangle(VrVoxelTriangle triangle, VrVoxelVertex* vertices)
            {
                this.RemapVertex(ref triangle.V0, vertices);
                this.RemapVertex(ref triangle.V1, vertices);
                this.RemapVertex(ref triangle.V2, vertices);
                this.Triangles.Add(triangle);
            }

            public void Clear()
            {
                this.m_indexMap.Clear();
                this.Vertices.Clear();
                this.Triangles.Clear();
            }

            private int GetMaterialIndex(byte material) => 
                ((material != this.Material.M0) ? ((material != this.Material.M1) ? ((material != this.Material.M2) ? -1 : 2) : 1) : 0);

            public void Init(MyRenderDataBuilder.MaterialTriple material)
            {
                this.Material = material;
            }

            private unsafe void RemapVertex(ref ushort vertex, VrVoxelVertex* vertices)
            {
                ushort num;
                if (this.m_indexMap.TryGetValue(vertex, out num))
                {
                    vertex = num;
                }
                else
                {
                    int count = this.Vertices.Count;
                    MyVertexFormatVoxelSingleData item = new MyVertexFormatVoxelSingleData {
                        Position = vertices[vertex].Position,
                        Normal = vertices[vertex].Normal,
                        PackedColorShift = vertices[vertex].Color.PackedValue,
                        Material = new Byte4((float) this.Material.M0, (float) this.Material.M1, (float) this.Material.M2, (float) this.GetMaterialIndex(vertices[vertex].Material))
                    };
                    this.Vertices.Add(item);
                    this.m_indexMap[vertex] = (ushort) count;
                    vertex = (ushort) count;
                }
            }
        }
    }
}

