namespace VRageRender
{
    using System;

    [Flags]
    public enum MyInstanceFlagsEnum : byte
    {
        CastShadows = 1,
        ShowLod1 = 2,
        EnableColorMask = 4
    }
}

