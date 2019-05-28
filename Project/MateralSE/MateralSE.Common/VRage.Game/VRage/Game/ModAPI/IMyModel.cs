namespace VRage.Game.ModAPI
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public interface IMyModel
    {
        int GetDummies(IDictionary<string, IMyModelDummy> dummies);
        int GetTrianglesCount();
        int GetVerticesCount();

        int UniqueId { get; }

        int DataVersion { get; }

        VRageMath.BoundingSphere BoundingSphere { get; }

        VRageMath.BoundingBox BoundingBox { get; }

        Vector3 BoundingBoxSize { get; }

        Vector3 BoundingBoxSizeHalf { get; }

        Vector3I[] BoneMapping { get; }

        float PatternScale { get; }

        float ScaleFactor { get; }

        string AssetName { get; }
    }
}

