namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemBot))]
    public class MyToolbarItemBot : MyToolbarItemDefinition
    {
        public override bool Activate()
        {
            if (!MyFakes.ENABLE_BARBARIANS || !MyPerGameSettings.EnableAi)
            {
                return false;
            }
            if (base.Definition == null)
            {
                return false;
            }
            MyAIComponent.Static.BotToSpawn = base.Definition as MyAgentDefinition;
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
            base.Init(data);
            base.ActivateOnClick = false;
            return true;
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            MyAgentDefinition botToSpawn = MyAIComponent.Static.BotToSpawn;
            this.WantsToBeSelected = (botToSpawn != null) && (botToSpawn.Id.SubtypeId == (base.Definition as MyAgentDefinition).Id.SubtypeId);
            return MyToolbarItem.ChangeInfo.None;
        }
    }
}

