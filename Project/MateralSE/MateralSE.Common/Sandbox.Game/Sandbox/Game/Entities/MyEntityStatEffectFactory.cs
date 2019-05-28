namespace Sandbox.Game.Entities
{
    using System;
    using System.Reflection;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;

    internal static class MyEntityStatEffectFactory
    {
        private static MyObjectFactory<MyEntityStatEffectTypeAttribute, MyEntityStatRegenEffect> m_objectFactory = new MyObjectFactory<MyEntityStatEffectTypeAttribute, MyEntityStatRegenEffect>();

        static MyEntityStatEffectFactory()
        {
            m_objectFactory.RegisterFromAssembly(Assembly.GetAssembly(typeof(MyEntityStatRegenEffect)));
        }

        public static MyEntityStatRegenEffect CreateInstance(MyObjectBuilder_EntityStatRegenEffect builder) => 
            m_objectFactory.CreateInstance(builder.TypeId);

        public static MyObjectBuilder_EntityStatRegenEffect CreateObjectBuilder(MyEntityStatRegenEffect effect) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_EntityStatRegenEffect>(effect);

        public static Type GetProducedType(MyObjectBuilderType objectBuilderType) => 
            m_objectFactory.GetProducedType(objectBuilderType);
    }
}

