namespace VRage.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public static class MyVoxelCoordSystems
    {
        public static Vector3I FindBestOctreeSize(float radius)
        {
            int x = 0x20;
            while (x < radius)
            {
                x *= 2;
            }
            return new Vector3I(x, x, x);
        }

        public static void GeometryCellCenterCoordToWorldPos(Vector3D referenceVoxelMapPosition, ref Vector3I geometryCellCoord, out Vector3D worldPos)
        {
            Vector3 vector;
            GeometryCellCoordToLocalPosition(ref geometryCellCoord, out vector);
            Vector3D worldPosition = vector;
            LocalPositionToWorldPosition(referenceVoxelMapPosition, ref vector, out worldPosition);
            worldPos = worldPosition + 4.0;
        }

        public static void GeometryCellCoordToLocalAABB(ref Vector3I geometryCellCoord, out BoundingBox localAABB)
        {
            Vector3 vector;
            GeometryCellCoordToLocalPosition(ref geometryCellCoord, out vector);
            localAABB = new BoundingBox(vector, vector + 8f);
        }

        public static void GeometryCellCoordToLocalPosition(ref MyCellCoord geometryCellCoord, out Vector3 localPosition)
        {
            localPosition = (Vector3) ((geometryCellCoord.CoordInLod * 8f) * (1 << (geometryCellCoord.Lod & 0x1f)));
        }

        public static void GeometryCellCoordToLocalPosition(ref Vector3I geometryCellCoord, out Vector3 localPosition)
        {
            localPosition = (Vector3) (geometryCellCoord * 8f);
        }

        public static void GeometryCellCoordToWorldAABB(Vector3D referenceVoxelMapPosition, ref MyCellCoord geometryCellCoord, out BoundingBoxD worldAABB)
        {
            Vector3 vector;
            GeometryCellCoordToLocalPosition(ref geometryCellCoord, out vector);
            Vector3D worldPosition = vector;
            LocalPositionToWorldPosition(referenceVoxelMapPosition, ref vector, out worldPosition);
            worldAABB = new BoundingBoxD(worldPosition, worldPosition + (8f * (1 << (geometryCellCoord.Lod & 0x1f))));
        }

        public static void GeometryCellCoordToWorldAABB(Vector3D referenceVoxelMapPosition, ref Vector3I geometryCellCoord, out BoundingBoxD worldAABB)
        {
            Vector3 vector;
            GeometryCellCoordToLocalPosition(ref geometryCellCoord, out vector);
            Vector3D worldPosition = vector;
            LocalPositionToWorldPosition(referenceVoxelMapPosition, ref vector, out worldPosition);
            worldAABB = new BoundingBoxD(worldPosition, worldPosition + 8f);
        }

        public static void LocalPositionToGeometryCellCoord(ref Vector3 localPosition, out Vector3I geometryCellCoord)
        {
            Vector3I.Floor(ref (ref Vector3D) ref (localPosition / 8f), out geometryCellCoord);
        }

        public static void LocalPositionToVertexCell(int lod, ref Vector3 localPosition, out Vector3I vertexCell)
        {
            float num = 1f * (1 << (lod & 0x1f));
            vertexCell = Vector3I.Floor(localPosition / num);
        }

        public static void LocalPositionToVoxelCoord(ref Vector3 localPosition, out Vector3D voxelCoord)
        {
            voxelCoord = localPosition / 1f;
        }

        public static void LocalPositionToVoxelCoord(ref Vector3 localPosition, out Vector3I voxelCoord)
        {
            Vector3I.Floor(ref localPosition / 1f, out voxelCoord);
        }

        public static void LocalPositionToWorldPosition(Vector3D referenceVoxelMapPosition, ref Vector3 localPosition, out Vector3D worldPosition)
        {
            worldPosition = localPosition + referenceVoxelMapPosition;
        }

        public static void VertexCellToLocalAABB(int lod, ref Vector3I vertexCell, out BoundingBoxD localAABB)
        {
            float num = 1f * (1 << (lod & 0x1f));
            Vector3 min = (Vector3) (vertexCell * num);
            localAABB = new BoundingBoxD(min, min + num);
        }

        public static void VertexCellToLocalPosition(int lod, ref Vector3I vertexCell, out Vector3 localPosition)
        {
            float num = 1f * (1 << (lod & 0x1f));
            localPosition = (Vector3) (vertexCell * num);
        }

        public static void VoxelCoordToGeometryCellCoord(ref Vector3I voxelCoord, out Vector3I geometryCellCoord)
        {
            geometryCellCoord = voxelCoord >> 3;
        }

        public static void VoxelCoordToLocalPosition(ref Vector3I voxelCoord, out Vector3 localPosition)
        {
            localPosition = (Vector3) (voxelCoord * 1f);
        }

        public static void VoxelCoordToWorldAABB(Vector3D referenceVoxelMapPosition, ref Vector3I voxelCoord, out BoundingBoxD worldAABB)
        {
            Vector3D vectord;
            VoxelCoordToWorldPosition(referenceVoxelMapPosition, ref voxelCoord, out vectord);
            worldAABB = new BoundingBoxD(vectord, vectord + 1f);
        }

        public static void VoxelCoordToWorldPosition(Vector3D referenceVoxelMapPosition, ref Vector3I voxelCoord, out Vector3D worldPosition)
        {
            Vector3 vector;
            VoxelCoordToLocalPosition(ref voxelCoord, out vector);
            LocalPositionToWorldPosition(referenceVoxelMapPosition, ref vector, out worldPosition);
        }

        public static void WorldPositionToGeometryCellCoord(Vector3D referenceVoxelMapPosition, ref Vector3D worldPosition, out Vector3I geometryCellCoord)
        {
            Vector3 vector;
            WorldPositionToLocalPosition(referenceVoxelMapPosition, ref worldPosition, out vector);
            LocalPositionToGeometryCellCoord(ref vector, out geometryCellCoord);
        }

        public static void WorldPositionToLocalPosition(Vector3D referenceVoxelMapPosition, ref Vector3D worldPosition, out Vector3 localPosition)
        {
            localPosition = (Vector3) (worldPosition - referenceVoxelMapPosition);
        }

        public static void WorldPositionToLocalPosition(Vector3D worldPosition, MatrixD worldMatrix, MatrixD worldMatrixInv, Vector3 halfSize, out Vector3 localPosition)
        {
            localPosition = (Vector3) Vector3D.Transform(worldPosition + Vector3D.TransformNormal(halfSize, worldMatrix), worldMatrixInv);
        }

        public static void WorldPositionToVoxelCoord(Vector3D referenceVoxelMapPosition, ref Vector3D worldPosition, out Vector3I voxelCoord)
        {
            Vector3 vector;
            WorldPositionToLocalPosition(referenceVoxelMapPosition, ref worldPosition, out vector);
            LocalPositionToVoxelCoord(ref vector, out voxelCoord);
        }

        public static void WorldPositionToVoxelCoord(ref Vector3D worldPosition, MatrixD worldMatrix, MatrixD worldMatrixInv, Vector3 halfSize, out Vector3I voxelCoord)
        {
            Vector3 vector;
            WorldPositionToLocalPosition(worldPosition, worldMatrix, worldMatrixInv, halfSize, out vector);
            LocalPositionToVoxelCoord(ref vector, out voxelCoord);
        }
    }
}

