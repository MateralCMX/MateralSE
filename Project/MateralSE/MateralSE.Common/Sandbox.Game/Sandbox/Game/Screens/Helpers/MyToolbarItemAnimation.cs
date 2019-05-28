namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemAnimation))]
    internal class MyToolbarItemAnimation : MyToolbarItemDefinition
    {
        public override unsafe bool Activate()
        {
            if (base.Definition == null)
            {
                return false;
            }
            MyAnimationDefinition definition = (MyAnimationDefinition) base.Definition;
            MyCharacter character = (MySession.Static.ControlledEntity is MyCockpit) ? ((MyCockpit) MySession.Static.ControlledEntity).Pilot : MySession.Static.LocalCharacter;
            if (character != null)
            {
                if (character.UseNewAnimationSystem)
                {
                    if (!character.IsOnLadder)
                    {
                        character.TriggerCharacterAnimationEvent(definition.Id.SubtypeName.ToLower(), true);
                    }
                }
                else
                {
                    MyAnimationCommand* commandPtr1;
                    MyAnimationCommand command = new MyAnimationCommand {
                        AnimationSubtypeName = definition.Id.SubtypeName,
                        BlendTime = 0.2f,
                        PlaybackCommand = MyPlaybackCommand.Play
                    };
                    commandPtr1->FrameOption = definition.Loop ? MyFrameOption.Loop : MyFrameOption.PlayOnce;
                    commandPtr1 = (MyAnimationCommand*) ref command;
                    command.TimeScale = 1f;
                    character.AddCommand(command, true);
                }
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type == MyToolbarType.Character) || ((type == MyToolbarType.Ship) || (type == MyToolbarType.Seat)));

        public override bool Init(MyObjectBuilder_ToolbarItem objBuilder)
        {
            base.Init(objBuilder);
            base.ActivateOnClick = true;
            base.WantsToBeActivated = true;
            return true;
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L) => 
            MyToolbarItem.ChangeInfo.None;
    }
}

