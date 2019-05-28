namespace VRage.Library.Collections
{
    using System;
    using System.Runtime.InteropServices;

    public interface IBitSerializable
    {
        bool Serialize(BitStream stream, bool validate, bool acceptAndSetValue = true);
    }
}

