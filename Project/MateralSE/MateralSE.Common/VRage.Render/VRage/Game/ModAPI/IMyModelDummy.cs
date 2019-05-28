namespace VRage.Game.ModAPI
{
    using System;
    using VRageMath;

    public interface IMyModelDummy
    {
        string Name { get; }

        VRageMath.Matrix Matrix { get; }
    }
}

