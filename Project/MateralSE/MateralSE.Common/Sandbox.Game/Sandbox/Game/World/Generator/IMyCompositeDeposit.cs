namespace Sandbox.Game.World.Generator
{
    using System;
    using VRage.Game;
    using VRageMath;

    public interface IMyCompositeDeposit : IMyCompositeShape
    {
        MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 localPos, float lodVoxelSize);
    }
}

