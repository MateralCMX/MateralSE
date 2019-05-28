namespace VRageRender.Utils
{
    using Havok;
    using System;
    using VRageMath;
    using VRageRender.Messages;

    public class MyPhysicsMesh : IPhysicsMesh
    {
        private MyModelData m_data = new MyModelData();

        public BoundingBox GetAABB() => 
            this.m_data.AABB;

        void IPhysicsMesh.AddIndex(int index)
        {
            this.m_data.Indices.Add(index);
        }

        void IPhysicsMesh.AddSectionData(int indexStart, int triCount, string matName)
        {
            if (matName.Contains("/"))
            {
                matName = matName.Substring(matName.IndexOf('/') + 1);
            }
            MyRuntimeSectionInfo item = new MyRuntimeSectionInfo {
                IndexStart = indexStart,
                TriCount = triCount,
                MaterialName = matName
            };
            this.m_data.Sections.Add(item);
        }

        void IPhysicsMesh.AddVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord)
        {
            this.m_data.Positions.Add(position);
            this.m_data.Normals.Add(normal);
            this.m_data.Tangents.Add(tangent);
            this.m_data.TexCoords.Add(texCoord);
        }

        int IPhysicsMesh.GetIndex(int idx) => 
            this.m_data.Indices[idx];

        int IPhysicsMesh.GetIndicesCount() => 
            this.m_data.Indices.Count;

        bool IPhysicsMesh.GetSectionData(int idx, ref int indexStart, ref int triCount, ref string matName)
        {
            if (idx >= this.m_data.Sections.Count)
            {
                return false;
            }
            indexStart = this.m_data.Sections[idx].IndexStart;
            triCount = this.m_data.Sections[idx].TriCount;
            matName = this.m_data.Sections[idx].MaterialName;
            return true;
        }

        int IPhysicsMesh.GetSectionsCount() => 
            this.m_data.Sections.Count;

        bool IPhysicsMesh.GetVertex(int vertexId, ref Vector3 position, ref Vector3 normal, ref Vector3 tangent, ref Vector2 texCoord)
        {
            if (vertexId >= this.m_data.Positions.Count)
            {
                return false;
            }
            position = this.m_data.Positions[vertexId];
            normal = this.m_data.Normals[vertexId];
            tangent = this.m_data.Tangents[vertexId];
            texCoord = this.m_data.TexCoords[vertexId];
            return true;
        }

        int IPhysicsMesh.GetVerticesCount() => 
            this.m_data.Positions.Count;

        void IPhysicsMesh.SetAABB(Vector3 min, Vector3 max)
        {
            this.m_data.AABB = new BoundingBox(min, max);
        }

        void IPhysicsMesh.Transform(Matrix m)
        {
            this.TransformInternal(ref m);
        }

        public void Transform(Matrix m)
        {
            this.TransformInternal(ref m);
        }

        private void TransformInternal(ref Matrix m)
        {
            for (int i = 0; i < this.m_data.Positions.Count; i++)
            {
                this.m_data.Positions[i] = Vector3.Transform(this.m_data.Positions[i], (Matrix) m);
            }
        }

        public MyModelData Data
        {
            set => 
                (this.m_data = value);
        }
    }
}

