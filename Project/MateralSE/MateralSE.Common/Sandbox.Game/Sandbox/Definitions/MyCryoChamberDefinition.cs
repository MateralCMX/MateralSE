namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_CryoChamberDefinition), (Type) null)]
    public class MyCryoChamberDefinition : MyCockpitDefinition
    {
        public string OverlayTexture;
        public string ResourceSinkGroup;
        public float IdlePowerConsumption;
        public MySoundPair OutsideSound;
        public MySoundPair InsideSound;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CryoChamberDefinition definition = builder as MyObjectBuilder_CryoChamberDefinition;
            this.OverlayTexture = definition.OverlayTexture;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.IdlePowerConsumption = definition.IdlePowerConsumption;
            this.OutsideSound = new MySoundPair(definition.OutsideSound, true);
            this.InsideSound = new MySoundPair(definition.InsideSound, true);
        }
    }
}

