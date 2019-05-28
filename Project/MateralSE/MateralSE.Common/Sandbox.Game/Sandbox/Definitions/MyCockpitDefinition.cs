namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_CockpitDefinition), (Type) null)]
    public class MyCockpitDefinition : MyShipControllerDefinition
    {
        public float OxygenCapacity;
        public bool IsPressurized;
        public string HUD;
        public bool HasInventory;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CockpitDefinition definition = builder as MyObjectBuilder_CockpitDefinition;
            base.GlassModel = definition.GlassModel;
            base.InteriorModel = definition.InteriorModel;
            string characterAnimation = definition.CharacterAnimation;
            this.CharacterAnimation = characterAnimation ?? definition.CharacterAnimationFile;
            if (!string.IsNullOrEmpty(definition.CharacterAnimationFile))
            {
                MyDefinitionErrors.Add(base.Context, "<CharacterAnimation> tag must contain animation name (defined in Animations.sbc) not the file: " + definition.CharacterAnimationFile, TErrorSeverity.Error, true);
            }
            this.OxygenCapacity = definition.OxygenCapacity;
            this.IsPressurized = definition.IsPressurized;
            this.HasInventory = definition.HasInventory;
            this.HUD = definition.HUD;
            this.ScreenAreas = (definition.ScreenAreas != null) ? definition.ScreenAreas.ToList<ScreenArea>() : null;
        }
    }
}

