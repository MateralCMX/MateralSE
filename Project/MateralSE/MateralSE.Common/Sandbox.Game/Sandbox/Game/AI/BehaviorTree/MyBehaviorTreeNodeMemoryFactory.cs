namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Reflection;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

    internal static class MyBehaviorTreeNodeMemoryFactory
    {
        private static MyObjectFactory<MyBehaviorTreeNodeMemoryTypeAttribute, MyBehaviorTreeNodeMemory> m_objectFactory = new MyObjectFactory<MyBehaviorTreeNodeMemoryTypeAttribute, MyBehaviorTreeNodeMemory>();

        static MyBehaviorTreeNodeMemoryFactory()
        {
            m_objectFactory.RegisterFromAssembly(Assembly.GetAssembly(typeof(MyBehaviorTreeNodeMemory)));
            m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.SandboxAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.UserAssemblies);
        }

        public static MyBehaviorTreeNodeMemory CreateNodeMemory(MyObjectBuilder_BehaviorTreeNodeMemory builder) => 
            m_objectFactory.CreateInstance(builder.TypeId);

        public static MyObjectBuilder_BehaviorTreeNodeMemory CreateObjectBuilder(MyBehaviorTreeNodeMemory cubeBlock) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_BehaviorTreeNodeMemory>(cubeBlock);

        public static Type GetProducedType(MyObjectBuilderType objectBuilderType) => 
            m_objectFactory.GetProducedType(objectBuilderType);
    }
}

