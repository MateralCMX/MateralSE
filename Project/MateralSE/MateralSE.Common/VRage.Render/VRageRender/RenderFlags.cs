namespace VRageRender
{
    using System;

    [Flags]
    public enum RenderFlags
    {
        SkipIfTooSmall = 1,
        NeedsResolveCastShadow = 2,
        FastCastShadowResolve = 4,
        CastShadows = 8,
        Visible = 0x10,
        DrawOutsideViewDistance = 0x20,
        Near = 0x40,
        UseCustomDrawMatrix = 0x80,
        ShadowLodBox = 0x100,
        NoBackFaceCulling = 0x200,
        SkipInMainView = 0x400,
        ForceOldPipeline = 0x800,
        CastShadowsOnLow = 0x1000,
        DrawInAllCascades = 0x2000,
        DistanceFade = 0x4000,
        SkipInDepth = 0x8000,
        MetalnessColorable = 0x10000
    }
}

