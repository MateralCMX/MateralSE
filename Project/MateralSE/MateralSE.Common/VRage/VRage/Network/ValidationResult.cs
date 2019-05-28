namespace VRage.Network
{
    using System;

    [Flags]
    public enum ValidationResult
    {
        Passed = 0,
        Kick = 1,
        Access = 2,
        Controlled = 4,
        Ownership = 8,
        BigOwner = 0x10,
        BigOwnerSpaceMaster = 0x20
    }
}

