namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public interface IMyTerrainHeightProvider
    {
        float GetReferenceTerrainHeight();
        bool GetTerrainHeight(Vector3 bonePosition, Vector3 boneRigPosition, out float terrainHeight, out Vector3 terrainNormal);
    }
}

