namespace Havok
{
    using System;
    using VRageMath;

    public interface IPhysicsMesh
    {
        void AddIndex(int index);
        void AddSectionData(int indexStart, int triCount, string materialName);
        void AddVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord);
        int GetIndex(int idx);
        int GetIndicesCount();
        bool GetSectionData(int idx, ref int indexStart, ref int triCount, ref string matIdx);
        int GetSectionsCount();
        bool GetVertex(int vertexId, ref Vector3 position, ref Vector3 normal, ref Vector3 tangent, ref Vector2 texCoord);
        int GetVerticesCount();
        void SetAABB(Vector3 min, Vector3 max);
        void Transform(Matrix m);
    }
}

