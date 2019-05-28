namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemAiCommand))]
    public class MyToolbarItemAiCommand : MyToolbarItemDefinition
    {
        public override bool Activate()
        {
            if (base.Definition == null)
            {
                return false;
            }
            MyAIComponent.Static.CommandDefinition = base.Definition as MyAiCommandDefinition;
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                controlledEntity.SwitchToWeapon((MyToolbarItemWeapon) null);
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type == MyToolbarType.Character) || (type == MyToolbarType.Spectator));

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            base.ActivateOnClick = false;
            return base.Init(data);
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            MyAiCommandDefinition commandDefinition = MyAIComponent.Static.CommandDefinition;
            this.WantsToBeSelected = (commandDefinition != null) && (commandDefinition.Id.SubtypeId == (base.Definition as MyAiCommandDefinition).Id.SubtypeId);
            return MyToolbarItem.ChangeInfo.None;
        }
    }
}

