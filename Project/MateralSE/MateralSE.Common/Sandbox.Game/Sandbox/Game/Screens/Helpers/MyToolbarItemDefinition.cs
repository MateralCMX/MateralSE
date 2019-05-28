namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using System;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public abstract class MyToolbarItemDefinition : MyToolbarItem
    {
        public MyDefinitionBase Definition;

        public MyToolbarItemDefinition()
        {
            base.SetEnabled(true);
            base.WantsToBeActivated = true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            MyToolbarItemDefinition definition = obj as MyToolbarItemDefinition;
            return ((definition != null) && ((this.Definition != null) && this.Definition.Id.Equals(definition.Definition.Id)));
        }

        public sealed override int GetHashCode() => 
            this.Definition.Id.GetHashCode();

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder()
        {
            if (this.Definition == null)
            {
                return null;
            }
            MyObjectBuilder_ToolbarItemDefinition definition1 = (MyObjectBuilder_ToolbarItemDefinition) MyToolbarItemFactory.CreateObjectBuilder(this);
            definition1.DefinitionId = (SerializableDefinitionId) this.Definition.Id;
            return definition1;
        }

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            if (!MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(((MyObjectBuilder_ToolbarItemDefinition) data).DefinitionId, out this.Definition))
            {
                return false;
            }
            if (!this.Definition.Public && !MyFakes.ENABLE_NON_PUBLIC_BLOCKS)
            {
                return false;
            }
            base.SetDisplayName(this.Definition.DisplayNameText);
            base.SetIcons(this.Definition.Icons);
            return true;
        }
    }
}

