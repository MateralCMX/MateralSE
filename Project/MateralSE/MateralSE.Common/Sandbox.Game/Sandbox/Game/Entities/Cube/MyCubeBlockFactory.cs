namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using System.Reflection;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

    internal static class MyCubeBlockFactory
    {
        private static MyObjectFactory<MyCubeBlockTypeAttribute, object> m_objectFactory = new MyObjectFactory<MyCubeBlockTypeAttribute, object>();

        static MyCubeBlockFactory()
        {
            m_objectFactory.RegisterFromAssembly(Assembly.GetAssembly(typeof(MyCubeBlock)));
            m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.SandboxAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.UserAssemblies);
        }

        public static object CreateCubeBlock(MyObjectBuilder_CubeBlock builder)
        {
            object local1 = m_objectFactory.CreateInstance(builder.TypeId);
            VRage.Game.Entity.MyEntity entity = local1 as VRage.Game.Entity.MyEntity;
            if (entity != null)
            {
                MyEntityFactory.AddScriptGameLogic(entity, builder.TypeId, builder.SubtypeName);
            }
            return local1;
        }

        public static MyObjectBuilder_CubeBlock CreateObjectBuilder(MyCubeBlock cubeBlock) => 
            ((MyObjectBuilder_CubeBlock) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) cubeBlock.BlockDefinition.Id));

        public static Type GetProducedType(MyObjectBuilderType objectBuilderType) => 
            m_objectFactory.GetProducedType(objectBuilderType);
    }
}

