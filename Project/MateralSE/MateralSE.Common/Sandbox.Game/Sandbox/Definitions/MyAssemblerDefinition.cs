namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AssemblerDefinition), (Type) null)]
    public class MyAssemblerDefinition : MyProductionBlockDefinition
    {
        private float m_assemblySpeed;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AssemblerDefinition definition = builder as MyObjectBuilder_AssemblerDefinition;
            this.m_assemblySpeed = definition.AssemblySpeed;
        }

        protected override void InitializeLegacyBlueprintClasses(MyObjectBuilder_ProductionBlockDefinition ob)
        {
            ob.BlueprintClasses = new string[] { "LargeBlocks", "SmallBlocks", "Components", "Tools" };
        }

        public float AssemblySpeed =>
            this.m_assemblySpeed;
    }
}

