namespace VRageRender.Messages
{
    using System;

    [Flags]
    public enum TextureType
    {
        GUI = 1,
        Particles = 2,
        ColorMetal = 4,
        NormalGloss = 8,
        AlphaMask = 0x10,
        Extensions = 0x20,
        GUIWithoutPremultiplyAlpha = 0x40,
        Temporary = 0x100,
        Prioritized = 0x80
    }
}

