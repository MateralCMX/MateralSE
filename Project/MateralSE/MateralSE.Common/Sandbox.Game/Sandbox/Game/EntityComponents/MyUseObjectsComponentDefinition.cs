namespace Sandbox.Game.EntityComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyDefinitionType(typeof(MyObjectBuilder_UseObjectsComponentDefinition), (Type) null)]
    public class MyUseObjectsComponentDefinition : MyComponentDefinitionBase
    {
        public bool LoadFromModel;
        public string UseObjectFromModelBBox;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_UseObjectsComponentDefinition definition = builder as MyObjectBuilder_UseObjectsComponentDefinition;
            this.LoadFromModel = definition.LoadFromModel;
            this.UseObjectFromModelBBox = definition.UseObjectFromModelBBox;
        }
    }
}

