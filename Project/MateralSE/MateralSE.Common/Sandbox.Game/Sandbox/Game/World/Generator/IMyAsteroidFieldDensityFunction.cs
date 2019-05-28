namespace Sandbox.Game.World.Generator
{
    using System;
    using VRage.Noise;
    using VRageMath;

    public interface IMyAsteroidFieldDensityFunction : IMyModule
    {
        bool ExistsInCell(ref BoundingBoxD bbox);
    }
}

