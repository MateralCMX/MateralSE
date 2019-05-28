namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyTextSurfaceScript : IDisposable
    {
        void Run();

        IMyTextSurface Surface { get; }

        IMyCubeBlock Block { get; }

        ScriptUpdate NeedsUpdate { get; }

        Color ForegroundColor { get; }

        Color BackgroundColor { get; }
    }
}

