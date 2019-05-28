namespace Sandbox.Engine.Voxels
{
    using System;
    using VRageMath;

    public interface IMyWrappedCubemapFace
    {
        void CopyRange(Vector2I start, Vector2I end, IMyWrappedCubemapFace other, Vector2I oStart, Vector2I oEnd);
        void FinishFace(string name);

        int Resolution { get; }

        int ResolutionMinusOne { get; }
    }
}

