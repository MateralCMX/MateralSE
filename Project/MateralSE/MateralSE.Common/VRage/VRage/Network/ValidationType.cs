namespace VRage.Network
{
    using System;

    [Flags]
    public enum ValidationType
    {
        None = 0,
        Access = 1,
        Controlled = 2,
        Ownership = 4,
        BigOwner = 8,
        BigOwnerSpaceMaster = 0x10,
        IgnoreDLC = 0x20
    }
}

