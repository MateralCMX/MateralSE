namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyProjector : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity, IMyTextSurfaceProvider
    {
        void UpdateOffsetAndRotation();

        [Obsolete("Use ProjectionOffset vector instead.")]
        int ProjectionOffsetX { get; }

        [Obsolete("Use ProjectionOffset vector instead.")]
        int ProjectionOffsetY { get; }

        [Obsolete("Use ProjectionOffset vector instead.")]
        int ProjectionOffsetZ { get; }

        [Obsolete("Use ProjectionRotation vector instead.")]
        int ProjectionRotX { get; }

        [Obsolete("Use ProjectionRotation vector instead.")]
        int ProjectionRotY { get; }

        [Obsolete("Use ProjectionRotation vector instead.")]
        int ProjectionRotZ { get; }

        bool IsProjecting { get; }

        int TotalBlocks { get; }

        int RemainingBlocks { get; }

        Dictionary<MyDefinitionBase, int> RemainingBlocksPerType { get; }

        int RemainingArmorBlocks { get; }

        int BuildableBlocksCount { get; }

        Vector3I ProjectionOffset { get; set; }

        Vector3I ProjectionRotation { get; set; }

        bool ShowOnlyBuildable { get; set; }
    }
}

