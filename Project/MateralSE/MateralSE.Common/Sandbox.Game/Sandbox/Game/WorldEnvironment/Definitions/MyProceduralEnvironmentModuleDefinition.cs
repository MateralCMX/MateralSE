namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilder;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ProceduralEnvironmentModuleDefinition), (Type) null)]
    public class MyProceduralEnvironmentModuleDefinition : MyDefinitionBase
    {
        public Type ModuleType;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ProceduralEnvironmentModuleDefinition definition = (MyObjectBuilder_ProceduralEnvironmentModuleDefinition) builder;
            this.ModuleType = MyGlobalTypeMetadata.Static.GetType(definition.QualifiedTypeName, false);
            if (this.ModuleType == null)
            {
                object[] args = new object[] { definition.QualifiedTypeName };
                MyLog.Default.Error("Could not find module type {0}!", args);
                throw new ArgumentException("Could not find module type;");
            }
        }
    }
}

