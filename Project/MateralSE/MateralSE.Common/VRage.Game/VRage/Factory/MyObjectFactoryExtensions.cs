namespace VRage.Factory
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.ObjectBuilders;

    public static class MyObjectFactoryExtensions
    {
        public static TCreated CreateAndDeserialize<TAttribute, TCreated>(this VRage.Factory.MyObjectFactory<TAttribute, TCreated> self, MyObjectBuilder_Base builder) where TAttribute: MyFactoryTagAttribute where TCreated: class, IMyObject
        {
            TCreated local1 = self.CreateInstance(builder.TypeId);
            local1.Deserialize(builder);
            return local1;
        }
    }
}

