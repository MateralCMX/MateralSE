namespace SpaceEngineers.Game.ModAPI.Ingame
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyGravityGenerator : IMyGravityGeneratorBase, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        [Obsolete("Use FieldSize.X")]
        float FieldWidth { get; }

        [Obsolete("Use FieldSize.Y")]
        float FieldHeight { get; }

        [Obsolete("Use FieldSize.Z")]
        float FieldDepth { get; }

        Vector3 FieldSize { get; set; }
    }
}

