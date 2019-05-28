namespace Sandbox.Game.AI
{
    using Sandbox.Common.ObjectBuilders;
    using System;
    using VRage.ObjectBuilders;

    internal static class MyAutopilotFactory
    {
        private static MyObjectFactory<MyAutopilotTypeAttribute, MyAutopilotBase> m_objectFactory = new MyObjectFactory<MyAutopilotTypeAttribute, MyAutopilotBase>();

        static MyAutopilotFactory()
        {
            m_objectFactory.RegisterFromCreatedObjectAssembly();
        }

        public static MyAutopilotBase CreateAutopilot(MyObjectBuilder_AutopilotBase builder) => 
            m_objectFactory.CreateInstance(builder.TypeId);

        public static MyObjectBuilder_AutopilotBase CreateObjectBuilder(MyAutopilotBase autopilot) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_AutopilotBase>(autopilot);
    }
}

