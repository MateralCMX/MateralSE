namespace Sandbox.Game.AI.BehaviorTree
{
    using System;
    using System.Reflection;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

    internal static class MyBehaviorTreeNodeFactory
    {
        private static MyObjectFactory<MyBehaviorTreeNodeTypeAttribute, MyBehaviorTreeNode> m_objectFactory = new MyObjectFactory<MyBehaviorTreeNodeTypeAttribute, MyBehaviorTreeNode>();

        static MyBehaviorTreeNodeFactory()
        {
            m_objectFactory.RegisterFromAssembly(Assembly.GetAssembly(typeof(MyBehaviorTreeNode)));
            m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.SandboxAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.UserAssemblies);
        }

        public static MyBehaviorTreeNode CreateBTNode(MyObjectBuilder_BehaviorTreeNode builder) => 
            m_objectFactory.CreateInstance(builder.TypeId);

        public static Type GetProducedType(MyObjectBuilderType objectBuilderType) => 
            m_objectFactory.GetProducedType(objectBuilderType);
    }
}

