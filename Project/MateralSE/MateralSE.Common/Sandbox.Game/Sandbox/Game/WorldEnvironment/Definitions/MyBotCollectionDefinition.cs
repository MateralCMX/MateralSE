namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_BotCollectionDefinition), (Type) null)]
    public class MyBotCollectionDefinition : MyDefinitionBase
    {
        public MyDiscreteSampler<MyDefinitionId> Bots;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_BotCollectionDefinition definition = builder as MyObjectBuilder_BotCollectionDefinition;
            if (definition != null)
            {
                List<MyDefinitionId> values = new List<MyDefinitionId>();
                List<float> densities = new List<float>();
                for (int i = 0; i < definition.Bots.Length; i++)
                {
                    MyObjectBuilder_BotCollectionDefinition.BotDefEntry entry = definition.Bots[i];
                    values.Add(entry.Id);
                    densities.Add(entry.Probability);
                }
                this.Bots = new MyDiscreteSampler<MyDefinitionId>(values, densities);
            }
        }
    }
}

