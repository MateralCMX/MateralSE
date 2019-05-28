namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_CraftingComponentInteractiveDefinition), (Type) null)]
    public class MyCraftingComponentInteractiveDefinition : MyComponentDefinitionBase
    {
        public List<string> AvailableBlueprintClasses = new List<string>();
        public string ActionSound = "";
        public float CraftingSpeedMultiplier = 1f;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CraftingComponentInteractiveDefinition definition = builder as MyObjectBuilder_CraftingComponentInteractiveDefinition;
            this.ActionSound = definition.ActionSound;
            this.CraftingSpeedMultiplier = definition.CraftingSpeedMultiplier;
            if ((definition.AvailableBlueprintClasses != null) && (definition.AvailableBlueprintClasses != string.Empty))
            {
                char[] separator = new char[] { ' ' };
                this.AvailableBlueprintClasses = definition.AvailableBlueprintClasses.Split(separator).ToList<string>();
                if (!MyFakes.ENABLE_DURABILITY_COMPONENT && this.AvailableBlueprintClasses.Contains("ToolsRepair"))
                {
                    this.AvailableBlueprintClasses.Remove("ToolsRepair");
                }
            }
        }
    }
}

