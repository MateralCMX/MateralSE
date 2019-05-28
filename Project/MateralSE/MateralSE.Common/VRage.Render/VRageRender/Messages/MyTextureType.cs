namespace VRageRender.Messages
{
    using System;

    [Flags]
    public enum MyTextureType
    {
        Unspecified = 0,
        ColorMetal = 1,
        NormalGloss = 2,
        Extensions = 4,
        Alphamask = 8
    }
}

