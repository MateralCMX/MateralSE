namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ButtonPanelDefinition), (Type) null)]
    public class MyButtonPanelDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public int ButtonCount;
        public string[] ButtonSymbols;
        public Vector4[] ButtonColors;
        public Vector4 UnassignedButtonColor;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ButtonPanelDefinition definition = builder as MyObjectBuilder_ButtonPanelDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.ButtonCount = definition.ButtonCount;
            this.ButtonSymbols = definition.ButtonSymbols;
            this.ButtonColors = definition.ButtonColors;
            this.UnassignedButtonColor = definition.UnassignedButtonColor;
        }
    }
}

