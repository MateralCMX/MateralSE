namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    public static class MySerializerNetObject
    {
        private static INetObjectResolver m_netObjectResolver;

        public static ResolverToken Using(INetObjectResolver netObjectResolver) => 
            new ResolverToken(netObjectResolver);

        public static INetObjectResolver NetObjectResolver =>
            m_netObjectResolver;

        [StructLayout(LayoutKind.Sequential)]
        public struct ResolverToken : IDisposable
        {
            private INetObjectResolver m_previousResolver;
            public ResolverToken(INetObjectResolver newResolver)
            {
                this.m_previousResolver = MySerializerNetObject.m_netObjectResolver;
                MySerializerNetObject.m_netObjectResolver = newResolver;
            }

            public void Dispose()
            {
                MySerializerNetObject.m_netObjectResolver = this.m_previousResolver;
                this.m_previousResolver = null;
            }
        }
    }
}

