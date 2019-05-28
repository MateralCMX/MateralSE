namespace VRage.Game.Components
{
    using System;
    using System.Reflection;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

    [PreloadRequired]
    public static class MyComponentFactory
    {
        private static MyObjectFactory<MyComponentBuilderAttribute, MyComponentBase> m_objectFactory = new MyObjectFactory<MyComponentBuilderAttribute, MyComponentBase>();

        static MyComponentFactory()
        {
            m_objectFactory.RegisterFromAssembly(Assembly.GetExecutingAssembly());
            m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.SandboxGameAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.UserAssemblies);
        }

        public static MyComponentBase CreateInstanceByType(Type type) => 
            (!type.IsAssignableFrom(typeof(MyComponentBase)) ? null : (Activator.CreateInstance(type) as MyComponentBase));

        public static MyComponentBase CreateInstanceByTypeId(MyObjectBuilderType type) => 
            m_objectFactory.CreateInstance(type);

        public static MyObjectBuilder_ComponentBase CreateObjectBuilder(MyComponentBase instance) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_ComponentBase>(instance);

        public static Type GetCreatedInstanceType(MyObjectBuilderType type) => 
            m_objectFactory.GetProducedType(type);

        public static Type TryGetCreatedInstanceType(MyObjectBuilderType type) => 
            m_objectFactory.TryGetProducedType(type);
    }
}

