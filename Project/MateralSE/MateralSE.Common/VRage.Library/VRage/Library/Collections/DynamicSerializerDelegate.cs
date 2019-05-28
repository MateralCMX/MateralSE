namespace VRage.Library.Collections
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void DynamicSerializerDelegate(BitStream stream, Type baseType, ref Type obj);
}

