namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ProgrammableBlockDefinition), (Type) null)]
    public class MyProgrammableBlockDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public List<ScreenArea> ScreenAreas;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ProgrammableBlockDefinition definition = (MyObjectBuilder_ProgrammableBlockDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.ScreenAreas = (definition.ScreenAreas != null) ? definition.ScreenAreas.ToList<ScreenArea>() : null;
        }
    }
}

