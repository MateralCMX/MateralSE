namespace VRage.Network
{
    using System;
    using VRageMath;

    public interface IMyEntityReplicable
    {
        MatrixD WorldMatrix { get; }

        long EntityId { get; }
    }
}

