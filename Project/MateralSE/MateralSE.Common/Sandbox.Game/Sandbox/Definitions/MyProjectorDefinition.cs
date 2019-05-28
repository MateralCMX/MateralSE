namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ProjectorDefinition), (Type) null)]
    public class MyProjectorDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public MySoundPair IdleSound;
        public bool AllowScaling;
        public bool AllowWelding;
        public bool IgnoreSize;
        public List<ScreenArea> ScreenAreas;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ProjectorDefinition definition = builder as MyObjectBuilder_ProjectorDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.IdleSound = new MySoundPair(definition.IdleSound, true);
            this.AllowScaling = definition.AllowScaling;
            this.AllowWelding = definition.AllowWelding;
            this.IgnoreSize = definition.IgnoreSize;
            this.ScreenAreas = (definition.ScreenAreas != null) ? definition.ScreenAreas.ToList<ScreenArea>() : null;
        }
    }
}

