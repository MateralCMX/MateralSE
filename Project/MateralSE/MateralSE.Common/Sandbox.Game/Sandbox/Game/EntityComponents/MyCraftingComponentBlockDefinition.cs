namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_CraftingComponentBlockDefinition), (Type) null)]
    public class MyCraftingComponentBlockDefinition : MyComponentDefinitionBase
    {
        public List<string> AvailableBlueprintClasses = new List<string>();
        public float CraftingSpeedMultiplier = 1f;
        public List<MyDefinitionId> AcceptedOperatingItems = new List<MyDefinitionId>();

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CraftingComponentBlockDefinition definition = builder as MyObjectBuilder_CraftingComponentBlockDefinition;
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
            if ((definition.AcceptedOperatingItems != null) && (definition.AcceptedOperatingItems.Length > 1))
            {
                foreach (SerializableDefinitionId id in definition.AcceptedOperatingItems)
                {
                    this.AcceptedOperatingItems.Add(id);
                }
            }
        }
    }
}

