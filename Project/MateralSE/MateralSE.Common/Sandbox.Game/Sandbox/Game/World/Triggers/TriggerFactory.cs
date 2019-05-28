namespace Sandbox.Game.World.Triggers
{
    using System;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public static class TriggerFactory
    {
        private static MyObjectFactory<TriggerTypeAttribute, MyTrigger> m_objectFactory = new MyObjectFactory<TriggerTypeAttribute, MyTrigger>();

        static TriggerFactory()
        {
            m_objectFactory.RegisterFromCreatedObjectAssembly();
        }

        public static MyTrigger CreateInstance(MyObjectBuilder_Trigger builder)
        {
            MyTrigger local1 = m_objectFactory.CreateInstance(builder.TypeId);
            local1.Init(builder);
            return local1;
        }

        public static MyObjectBuilder_Trigger CreateObjectBuilder(MyTrigger instance) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_Trigger>(instance);
    }
}

