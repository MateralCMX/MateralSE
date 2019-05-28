namespace VRage.Game.GUI.TextPanel
{
    using System;

    public enum ContentType : byte
    {
        NONE = 0,
        TEXT_AND_IMAGE = 1,
        [Obsolete("Use TEXT_AND_IMAGE instead.")]
        IMAGE = 2,
        SCRIPT = 3
    }
}

