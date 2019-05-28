namespace VRage.Render.Scene
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public interface IMyBillboardsHelper
    {
        void AddBillboardOriented(MyStringId material, Vector4 color, Vector3D origin, Vector3 leftVector, Vector3 upVector, float radiusX, float radiusY, MyBillboard.BlendTypeEnum blendType = 0, float softParticleDistanceScale = 1f, int customViewProjection = -1);
        void AddLineBillboard(MyStringId material, Vector4 color, Vector3D origin, Vector3 directionNormalized, float length, float thickness, MyBillboard.BlendTypeEnum blendType = 0, int customViewProjection = -1, float intensity = 1f);
        void AddPointBillboard(MyStringId material, Vector4 color, Vector3D origin, float radius, float angle, MyBillboard.BlendTypeEnum blendType = 0, int customViewProjection = -1, float intensity = 1f);
    }
}

