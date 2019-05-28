namespace Sandbox.ModAPI.Ingame
{
    using System;

    public interface IMyTextSurfaceProvider
    {
        IMyTextSurface GetSurface(int index);

        int SurfaceCount { get; }
    }
}

