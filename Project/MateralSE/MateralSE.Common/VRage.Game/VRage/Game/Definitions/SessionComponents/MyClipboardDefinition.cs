namespace VRage.Game.Definitions.SessionComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;

    [MyDefinitionType(typeof(MyObjectBuilder_ClipboardDefinition), (Type) null)]
    public class MyClipboardDefinition : MySessionComponentDefinition
    {
        public MyPlacementSettings PastingSettings;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ClipboardDefinition definition = (MyObjectBuilder_ClipboardDefinition) builder;
            this.PastingSettings = definition.PastingSettings;
        }
    }
}

