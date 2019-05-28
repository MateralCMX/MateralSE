namespace VRage.ModAPI
{
    using System;

    [Flags]
    public enum EntityFlags
    {
        None = 1,
        Visible = 2,
        Save = 8,
        Near = 0x10,
        NeedsUpdate = 0x20,
        NeedsResolveCastShadow = 0x40,
        FastCastShadowResolve = 0x80,
        SkipIfTooSmall = 0x100,
        NeedsUpdate10 = 0x200,
        NeedsUpdate100 = 0x400,
        NeedsDraw = 0x800,
        InvalidateOnMove = 0x1000,
        Sync = 0x2000,
        NeedsDrawFromParent = 0x4000,
        ShadowBoxLod = 0x8000,
        Transparent = 0x10000,
        NeedsUpdateBeforeNextFrame = 0x20000,
        DrawOutsideViewDistance = 0x40000,
        IsGamePrunningStructureObject = 0x80000,
        NeedsWorldMatrix = 0x100000,
        IsNotGamePrunningStructureObject = 0x200000,
        NeedsSimulate = 0x400000,
        UpdateRender = 0x800000,
        Default = 0x90114a
    }
}

