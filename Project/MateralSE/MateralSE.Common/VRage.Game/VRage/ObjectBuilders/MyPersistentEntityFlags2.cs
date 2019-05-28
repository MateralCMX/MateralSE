namespace VRage.ObjectBuilders
{
    using System;

    [Flags]
    public enum MyPersistentEntityFlags2
    {
        None = 0,
        Enabled = 2,
        CastShadows = 4,
        InScene = 0x10
    }
}

