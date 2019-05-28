namespace VRage.Serialization
{
    using System;
    using VRage.Library.Collections;

    public interface IDynamicResolver
    {
        void Serialize(BitStream stream, Type baseType, ref Type obj);
    }
}

