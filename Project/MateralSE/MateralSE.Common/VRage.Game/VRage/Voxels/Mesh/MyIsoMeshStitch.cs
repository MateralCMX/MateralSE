namespace VRage.Voxels.Mesh
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Library.Threading;
    using VRage.Voxels;
    using VRageMath;

    public class MyIsoMeshStitch
    {
        public readonly MyIsoMesh Mesh;
        private Dictionary<Vector3I, ushort> m_edgeIndex;
        private SpinLock m_lock;
        private readonly MyStorageData[] m_signField = new MyStorageData[6];
        private byte[] m_signFieldCache;
        private int m_signFieldSize;
        private Vector3I m_forwardLimit;
        private readonly int m_originlaVxCnt;
        private readonly int m_originalTriangleCnt;

        public MyIsoMeshStitch(MyIsoMesh mesh, MyStorageData meshContent)
        {
            this.Mesh = mesh;
            this.SliceSignField(meshContent);
            this.m_originlaVxCnt = mesh.VerticesCount;
            this.m_originalTriangleCnt = mesh.TrianglesCount;
        }

        internal void AddVertexToIndex(ushort vx)
        {
        }

        private void ExtractRange(MyStorageData data, MyStorageData field, Vector3I min, Vector3I max)
        {
            data.Resize((Vector3I) ((max - min) + 1));
            data.CopyRange(field, min, max, Vector3I.Zero, MyStorageDataTypeEnum.Content);
            data.CopyRange(field, min, max, Vector3I.Zero, MyStorageDataTypeEnum.Material);
        }

        public MyStorageData GetBorderSignField(int axis, bool side) => 
            this.m_signField[(axis << 1) + (side ? 1 : 0)];

        public void IndexEdges()
        {
            MyIsoMeshStitch stitch = this;
            lock (stitch)
            {
                if (this.m_edgeIndex == null)
                {
                    this.m_edgeIndex = new Dictionary<Vector3I, ushort>(Vector3I.Comparer);
                    for (int i = 0; i < this.Mesh.Cells.Count; i++)
                    {
                        Vector3I edge = this.Mesh.Cells[i];
                        if (this.IsEdge(ref edge))
                        {
                            this.m_edgeIndex[edge] = (ushort) i;
                        }
                    }
                }
            }
        }

        public bool IsEdge(ref Vector3I edge)
        {
            Vector3I vectori = (this.Mesh.CellEnd - this.Mesh.CellStart) - 1;
            return ((edge.X == 0) || ((edge.X == vectori.X) || ((edge.Y == 0) || ((edge.Y == vectori.Y) || ((edge.Z == 0) || (edge.Z == vectori.Z))))));
        }

        public void Reset()
        {
            for (int i = this.m_originlaVxCnt; i < this.Mesh.VerticesCount; i++)
            {
                this.m_edgeIndex.Remove(this.Mesh.Cells[i]);
            }
            this.Mesh.Resize(this.m_originlaVxCnt, this.m_originalTriangleCnt);
        }

        public unsafe void SampleEdge(Vector3I localPosition, out byte material, out byte content)
        {
            MyStorageData borderSignField;
            if (localPosition.X == 0)
            {
                borderSignField = this.GetBorderSignField(0, false);
            }
            else if (localPosition.X >= this.m_forwardLimit.X)
            {
                borderSignField = this.GetBorderSignField(0, true);
                int* numPtr1 = (int*) ref localPosition.X;
                numPtr1[0] -= this.m_forwardLimit.X;
            }
            else if (localPosition.Y == 0)
            {
                borderSignField = this.GetBorderSignField(1, false);
            }
            else if (localPosition.Y >= this.m_forwardLimit.Y)
            {
                borderSignField = this.GetBorderSignField(1, true);
                int* numPtr2 = (int*) ref localPosition.Y;
                numPtr2[0] -= this.m_forwardLimit.Y;
            }
            else if (localPosition.Z == 0)
            {
                borderSignField = this.GetBorderSignField(2, false);
            }
            else
            {
                if (localPosition.Z < this.m_forwardLimit.Z)
                {
                    throw new InvalidOperationException();
                }
                borderSignField = this.GetBorderSignField(2, true);
                int* numPtr3 = (int*) ref localPosition.Z;
                numPtr3[0] -= this.m_forwardLimit.Z;
            }
            int linearIdx = borderSignField.ComputeLinear(ref localPosition);
            material = borderSignField.Material(linearIdx);
            content = borderSignField.Content(linearIdx);
        }

        private void SliceSignField(MyStorageData field)
        {
            Vector3I vectori = field.Size3D - 1;
            this.m_forwardLimit = vectori - 1;
            this.m_signFieldSize = vectori.X;
            int num = vectori.X - 2;
            int num2 = (((this.m_signFieldSize * this.m_signFieldSize) * 3) + ((this.m_signFieldSize * num) * 3)) + ((num * num) * 3);
            this.m_signFieldCache = new byte[num2];
            for (int i = 0; i < 6; i++)
            {
                this.m_signField[i] = new MyStorageData(MyStorageDataTypeFlags.All);
            }
            this.ExtractRange(this.GetBorderSignField(0, false), field, new Vector3I(0, 0, 0), new Vector3I(0, vectori.Y, vectori.Z));
            this.ExtractRange(this.GetBorderSignField(0, true), field, new Vector3I(vectori.X - 1, 0, 0), new Vector3I(vectori.X, vectori.Y, vectori.Z));
            this.ExtractRange(this.GetBorderSignField(1, false), field, new Vector3I(0, 0, 0), new Vector3I(vectori.X, 0, vectori.Z));
            this.ExtractRange(this.GetBorderSignField(1, true), field, new Vector3I(0, vectori.Y - 1, 0), new Vector3I(vectori.X, vectori.Y, vectori.Z));
            this.ExtractRange(this.GetBorderSignField(2, false), field, new Vector3I(0, 0, 0), new Vector3I(vectori.X, vectori.Y, 0));
            this.ExtractRange(this.GetBorderSignField(2, true), field, new Vector3I(0, 0, vectori.Z - 1), new Vector3I(vectori.X, vectori.Y, vectori.Z));
        }

        public bool TryGetVertex(Vector3I coord, out ushort index)
        {
            if (this.m_edgeIndex != null)
            {
                return this.m_edgeIndex.TryGetValue(coord, out index);
            }
            index = 0;
            return false;
        }

        internal void WriteTriangle(ushort v2, ushort v1, ushort v0)
        {
            this.m_lock.Enter();
            try
            {
                this.Mesh.WriteTriangle(v2, v1, v0);
            }
            finally
            {
                this.m_lock.Exit();
            }
        }

        internal ushort WriteVertex(ref Vector3I cell, ref Vector3 pos, ref Vector3 normal, byte material, uint colorShift)
        {
            ushort num;
            this.m_lock.Enter();
            try
            {
                num = (ushort) this.Mesh.WriteVertex(ref cell, ref pos, ref normal, material, colorShift);
            }
            finally
            {
                this.m_lock.Exit();
            }
            return num;
        }

        public bool IsStitched =>
            (this.m_originlaVxCnt != this.Mesh.VerticesCount);
    }
}

