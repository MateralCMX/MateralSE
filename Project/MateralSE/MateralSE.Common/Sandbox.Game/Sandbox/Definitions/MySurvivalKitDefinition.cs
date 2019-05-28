namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_SurvivalKitDefinition), (Type) null)]
    public class MySurvivalKitDefinition : MyAssemblerDefinition
    {
        public string ProgressSound = "BlockMedicalProgress";
        public List<ScreenArea> ScreenAreas;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SurvivalKitDefinition definition = (MyObjectBuilder_SurvivalKitDefinition) builder;
            this.ProgressSound = definition.ProgressSound;
            this.ScreenAreas = (definition.ScreenAreas != null) ? definition.ScreenAreas.ToList<ScreenArea>() : null;
        }
    }
}

