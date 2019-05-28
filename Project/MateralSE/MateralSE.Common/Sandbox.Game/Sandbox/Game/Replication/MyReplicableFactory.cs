namespace Sandbox.Game.Replication
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Collections;
    using VRage.Plugins;

    public class MyReplicableFactory
    {
        private readonly MyConcurrentDictionary<Type, Type> m_objTypeToExternalReplicableType = new MyConcurrentDictionary<Type, Type>(0x20, null);

        public MyReplicableFactory()
        {
            Assembly[] first = new Assembly[] { typeof(MySandboxGame).Assembly, MyPlugins.GameAssembly, MyPlugins.SandboxAssembly, MyPlugins.SandboxGameAssembly };
            if (MyPlugins.UserAssemblies != null)
            {
                first = first.Union<Assembly>(MyPlugins.UserAssemblies).ToArray<Assembly>();
            }
            this.RegisterFromAssemblies(first);
        }

        public Type FindTypeFor(object obj)
        {
            Type key = obj.GetType();
            if (key.IsValueType)
            {
                throw new InvalidOperationException("obj cannot be value type");
            }
            Type type2 = null;
            Type baseType = key;
            while ((baseType != typeof(object)) && !this.m_objTypeToExternalReplicableType.TryGetValue(baseType, out type2))
            {
                baseType = baseType.BaseType;
            }
            if (key != baseType)
            {
                this.m_objTypeToExternalReplicableType.TryAdd(key, type2);
            }
            return type2;
        }

        private void RegisterFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in from x in assemblies
                where x != null
                select x)
            {
                this.RegisterFromAssembly(assembly);
            }
        }

        private void RegisterFromAssembly(Assembly assembly)
        {
            foreach (Type type in from t in assembly.GetTypes()
                where typeof(MyExternalReplicable).IsAssignableFrom(t) && !t.IsAbstract
                select t)
            {
                Type key = type.FindGenericBaseTypeArgument(typeof(MyExternalReplicable<>));
                if ((key != null) && !this.m_objTypeToExternalReplicableType.ContainsKey(key))
                {
                    this.m_objTypeToExternalReplicableType.TryAdd(key, type);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyReplicableFactory.<>c <>9 = new MyReplicableFactory.<>c();
            public static Func<Assembly, bool> <>9__2_0;
            public static Func<Type, bool> <>9__3_0;

            internal bool <RegisterFromAssemblies>b__2_0(Assembly x) => 
                (x != null);

            internal bool <RegisterFromAssembly>b__3_0(Type t) => 
                (typeof(MyExternalReplicable).IsAssignableFrom(t) && !t.IsAbstract);
        }
    }
}

