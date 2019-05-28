namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ResearchBlockDefinition), (Type) null)]
    public class MyResearchBlockDefinition : MyDefinitionBase
    {
        public string[] UnlockedByGroups;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ResearchBlockDefinition definition = builder as MyObjectBuilder_ResearchBlockDefinition;
            if (definition.UnlockedByGroups != null)
            {
                this.UnlockedByGroups = new string[definition.UnlockedByGroups.Length];
                for (int i = 0; i < definition.UnlockedByGroups.Length; i++)
                {
                    this.UnlockedByGroups[i] = definition.UnlockedByGroups[i];
                }
            }
        }
    }
}

