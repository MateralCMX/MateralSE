namespace VRage.Utils
{
    using System;

    [Flags]
    public enum LoggingOptions
    {
        NONE = 1,
        ENUM_CHECKING = 2,
        LOADING_MODELS = 4,
        LOADING_TEXTURES = 8,
        LOADING_CUSTOM_ASSETS = 0x10,
        LOADING_SPRITE_VIDEO = 0x20,
        VALIDATING_CUE_PARAMS = 0x40,
        CONFIG_ACCESS = 0x80,
        SIMPLE_NETWORKING = 0x100,
        VOXEL_MAPS = 0x200,
        MISC_RENDER_ASSETS = 0x400,
        AUDIO = 0x800,
        TRAILERS = 0x1000,
        SESSION_SETTINGS = 0x2000,
        ALL = 0x3fff
    }
}

