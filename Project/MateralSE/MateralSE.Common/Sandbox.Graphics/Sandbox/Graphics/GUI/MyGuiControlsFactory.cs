namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Reflection;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public static class MyGuiControlsFactory
    {
        private static MyObjectFactory<MyGuiControlTypeAttribute, MyGuiControlBase> m_objectFactory = new MyObjectFactory<MyGuiControlTypeAttribute, MyGuiControlBase>();

        public static MyGuiControlBase CreateGuiControl(MyObjectBuilder_Base builder) => 
            m_objectFactory.CreateInstance(builder.TypeId);

        public static MyObjectBuilder_GuiControlBase CreateObjectBuilder(MyGuiControlBase control) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_GuiControlBase>(control);

        public static void RegisterDescriptorsFromAssembly(Assembly assembly)
        {
            m_objectFactory.RegisterFromAssembly(assembly);
        }
    }
}

