namespace VRage.ObjectBuilders
{
    using System;
    using VRage.Library.Collections;
    using VRage.Serialization;

    public class MyObjectBuilderDynamicSerializer : IDynamicResolver
    {
        void IDynamicResolver.Serialize(BitStream stream, Type baseType, ref Type obj)
        {
            MyObjectBuilderSerializer.SerializeDynamic(stream, baseType, ref obj);
        }
    }
}

