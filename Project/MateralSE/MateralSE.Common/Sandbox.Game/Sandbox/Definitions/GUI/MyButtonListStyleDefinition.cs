namespace Sandbox.Definitions.GUI
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions.GUI;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ButtonListStyleDefinition), (Type) null)]
    public class MyButtonListStyleDefinition : MyDefinitionBase
    {
        public Vector2 ButtonSize;
        public Vector2 ButtonMargin;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
        }
    }
}

