namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRage.Network;
    using VRage.Plugins;

    public static class MyReplicationLayerExtensions
    {
        public static void RegisterFromGameAssemblies(this MyReplicationLayerBase layer)
        {
            Assembly[] assemblyArray = new Assembly[] { typeof(MySandboxGame).Assembly, typeof(MyRenderProfiler).Assembly, MyPlugins.GameAssembly, MyPlugins.SandboxAssembly, MyPlugins.SandboxGameAssembly };
            layer.RegisterFromAssembly((from s in assemblyArray
                where s != null
                select s).Distinct<Assembly>());
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyReplicationLayerExtensions.<>c <>9 = new MyReplicationLayerExtensions.<>c();
            public static Func<Assembly, bool> <>9__0_0;

            internal bool <RegisterFromGameAssemblies>b__0_0(Assembly s) => 
                (s != null);
        }
    }
}

