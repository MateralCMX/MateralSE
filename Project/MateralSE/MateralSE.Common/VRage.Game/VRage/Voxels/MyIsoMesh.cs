namespace VRage.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;
    using VRageMath;
    using VRageMath.PackedVector;

    public sealed class MyIsoMesh
    {
        public readonly List<Vector3> Positions = new List<Vector3>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public readonly List<byte> Materials = new List<byte>();
        public readonly List<sbyte> Densities = new List<sbyte>();
        public readonly List<Vector3I> Cells = new List<Vector3I>();
        public readonly List<uint> ColorShiftHSV = new List<uint>();
        public readonly List<MyVoxelTriangle> Triangles = new List<MyVoxelTriangle>();
        public int Lod;
        public Vector3 PositionScale;
        public Vector3D PositionOffset;
        public Vector3I CellStart;
        public Vector3I CellEnd;

        public void Clear()
        {
            this.Cells.Clear();
            this.Positions.Clear();
            this.Normals.Clear();
            this.Materials.Clear();
            this.Densities.Clear();
            this.ColorShiftHSV.Clear();
            this.Triangles.Clear();
        }

        public unsafe void CopytFromNative(VrVoxelMesh vrMesh)
        {
            Vector3* vectorPtr;
            Vector3[] pinned vectorArray;
            Vector3* vectorPtr2;
            Vector3[] pinned vectorArray2;
            byte* numPtr;
            byte[] pinned buffer;
            Vector3I* vectoriPtr;
            Vector3I[] pinned vectoriArray;
            MyVoxelTriangle* trianglePtr;
            MyVoxelTriangle[] pinned triangleArray;
            uint* numPtr2;
            uint[] pinned numArray;
            this.Lod = vrMesh.Lod;
            this.CellStart = vrMesh.Start;
            this.CellEnd = vrMesh.End;
            this.PositionScale = new Vector3((float) (1 << (this.Lod & 0x1f)));
            this.PositionOffset = (Vector3D) (this.CellStart * this.PositionScale);
            int vertexCount = vrMesh.VertexCount;
            int triangleCount = vrMesh.TriangleCount;
            this.Reserve(vertexCount, triangleCount);
            if (((vectorArray = this.Positions.GetInternalArray<Vector3>()) == null) || (vectorArray.Length == 0))
            {
                vectorPtr = null;
            }
            else
            {
                vectorPtr = vectorArray;
            }
            if (((vectorArray2 = this.Normals.GetInternalArray<Vector3>()) == null) || (vectorArray2.Length == 0))
            {
                vectorPtr2 = null;
            }
            else
            {
                vectorPtr2 = vectorArray2;
            }
            if (((buffer = this.Materials.GetInternalArray<byte>()) == null) || (buffer.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer;
            }
            if (((vectoriArray = this.Cells.GetInternalArray<Vector3I>()) == null) || (vectoriArray.Length == 0))
            {
                vectoriPtr = null;
            }
            else
            {
                vectoriPtr = vectoriArray;
            }
            if (((triangleArray = this.Triangles.GetInternalArray<MyVoxelTriangle>()) == null) || (triangleArray.Length == 0))
            {
                trianglePtr = null;
            }
            else
            {
                trianglePtr = triangleArray;
            }
            if (((numArray = this.ColorShiftHSV.GetInternalArray<uint>()) == null) || (numArray.Length == 0))
            {
                numPtr2 = null;
            }
            else
            {
                numPtr2 = numArray;
            }
            vrMesh.GetMeshData(vectorPtr, vectorPtr2, numPtr, vectoriPtr, (Byte4*) numPtr2, (VrVoxelTriangle*) trianglePtr);
            numArray = null;
            triangleArray = null;
            vectoriArray = null;
            buffer = null;
            vectorArray2 = null;
            vectorArray = null;
            this.Positions.SetSize<Vector3>(vertexCount);
            this.Normals.SetSize<Vector3>(vertexCount);
            this.Materials.SetSize<byte>(vertexCount);
            this.Cells.SetSize<Vector3I>(vertexCount);
            this.ColorShiftHSV.SetSize<uint>(vertexCount);
            this.Triangles.SetSize<MyVoxelTriangle>(triangleCount);
        }

        public static MyIsoMesh FromNative(VrVoxelMesh nativeMesh)
        {
            if (nativeMesh == null)
            {
                return null;
            }
            MyIsoMesh mesh1 = new MyIsoMesh();
            mesh1.CopytFromNative(nativeMesh);
            return mesh1;
        }

        public void GetUnpackedPosition(int idx, out Vector3 position)
        {
            position = (this.Positions[idx] * this.PositionScale) + this.PositionOffset;
        }

        public void GetUnpackedVertex(int idx, out MyVoxelVertex vertex)
        {
            vertex.Position = (this.Positions[idx] * this.PositionScale) + this.PositionOffset;
            vertex.Normal = this.Normals[idx];
            vertex.Material = this.Materials[idx];
            vertex.ColorShiftHSV = this.ColorShiftHSV[idx];
            vertex.Cell = this.Cells[idx];
        }

        public bool IsEdge(ref Vector3I cell)
        {
            Vector3I vectori = (this.CellEnd - this.CellStart) - 1;
            return ((cell.X == 0) || ((cell.X == vectori.X) || ((cell.Y == 0) || ((cell.Y == vectori.Y) || ((cell.Z == 0) || (cell.Z == vectori.Z))))));
        }

        public static bool IsEmpty(MyIsoMesh self) => 
            ((self == null) || (self.Triangles.Count == 0));

        public void Reserve(int vertexCount, int triangleCount)
        {
            if (this.Positions.Capacity < vertexCount)
            {
                this.Positions.Capacity = vertexCount;
                this.Normals.Capacity = vertexCount;
                this.Materials.Capacity = vertexCount;
                this.ColorShiftHSV.Capacity = vertexCount;
                this.Cells.Capacity = vertexCount;
                this.Densities.Capacity = vertexCount;
            }
            if (this.Triangles.Capacity < triangleCount)
            {
                this.Triangles.Capacity = triangleCount;
            }
        }

        public void Resize(int vertexCount, int triangleCount)
        {
            if (this.Positions.Capacity >= vertexCount)
            {
                this.Positions.SetSize<Vector3>(vertexCount);
                this.Normals.SetSize<Vector3>(vertexCount);
                this.Materials.SetSize<byte>(vertexCount);
                this.ColorShiftHSV.SetSize<uint>(vertexCount);
                this.Cells.SetSize<Vector3I>(vertexCount);
                this.Densities.SetSize<sbyte>(vertexCount);
            }
            if (this.Triangles.Capacity >= triangleCount)
            {
                this.Triangles.SetSize<MyVoxelTriangle>(triangleCount);
            }
        }

        public void WriteTriangle(int v0, int v1, int v2)
        {
            MyVoxelTriangle item = new MyVoxelTriangle {
                V0 = (ushort) v0,
                V1 = (ushort) v1,
                V2 = (ushort) v2
            };
            this.Triangles.Add(item);
        }

        public int WriteVertex(ref Vector3I cell, ref Vector3 position, ref Vector3 normal, byte material, uint colorShift)
        {
            this.Positions.Add(position);
            this.Normals.Add(normal);
            this.Materials.Add(material);
            this.Cells.Add(cell);
            this.ColorShiftHSV.Add(colorShift);
            return this.Positions.Count;
        }

        public int VerticesCount =>
            this.Positions.Count;

        public int TrianglesCount =>
            this.Triangles.Count;

        public Vector3I Size =>
            ((Vector3I) ((this.CellEnd - this.CellStart) + 1));
    }
}

