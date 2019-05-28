namespace VRage.Game.ModAPI
{
    using System;
    using VRage.ModAPI;
    using VRageMath;

    public interface IHitInfo
    {
        Vector3D Position { get; }

        IMyEntity HitEntity { get; }

        Vector3 Normal { get; }

        float Fraction { get; }
    }
}

