namespace Sandbox.Game.GameSystems.Chat
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.ModAPI;
    using VRage.Utils;

    public static class AnimationCommands
    {
        private static MyAnimationDefinition m_waveAnimation;
        private static MyAnimationDefinition m_facepalmAnimation;
        private static MyAnimationDefinition m_victoryAnimation;
        private static MyAnimationDefinition m_thumbsupAnimation;

        private static unsafe void Activate(MyAnimationDefinition animationDefinition)
        {
            if (animationDefinition != null)
            {
                MyCharacter character = (MySession.Static.ControlledEntity is MyCockpit) ? ((MyCockpit) MySession.Static.ControlledEntity).Pilot : MySession.Static.LocalCharacter;
                if ((character != null) && !character.IsOnLadder)
                {
                    if (character.UseNewAnimationSystem)
                    {
                        character.TriggerCharacterAnimationEvent(animationDefinition.Id.SubtypeName.ToLower(), true);
                    }
                    else
                    {
                        MyAnimationCommand* commandPtr1;
                        MyAnimationCommand command = new MyAnimationCommand {
                            AnimationSubtypeName = animationDefinition.Id.SubtypeName,
                            BlendTime = 0.2f,
                            PlaybackCommand = MyPlaybackCommand.Play
                        };
                        commandPtr1->FrameOption = animationDefinition.Loop ? MyFrameOption.Loop : MyFrameOption.PlayOnce;
                        commandPtr1 = (MyAnimationCommand*) ref command;
                        command.TimeScale = 1f;
                        character.AddCommand(command, true);
                    }
                }
            }
        }

        [ChatCommand("/facepalm", "ChatCommand_Help_Facepalm", "ChatCommand_HelpSimple_Facepalm", MyPromoteLevel.None)]
        private static void CommandFacepalm(string[] args)
        {
            if (m_facepalmAnimation == null)
            {
                LoadAnimations();
            }
            Activate(m_facepalmAnimation);
        }

        [ChatCommand("/thumb", "ChatCommand_Help_Thumb", "ChatCommand_HelpSimple_Thumb", MyPromoteLevel.None)]
        private static void CommandThumb(string[] args)
        {
            if (m_thumbsupAnimation == null)
            {
                LoadAnimations();
            }
            Activate(m_thumbsupAnimation);
        }

        [ChatCommand("/victory", "ChatCommand_Help_Victory", "ChatCommand_HelpSimple_Victory", MyPromoteLevel.None)]
        private static void CommandVictory(string[] args)
        {
            if (m_victoryAnimation == null)
            {
                LoadAnimations();
            }
            Activate(m_victoryAnimation);
        }

        [ChatCommand("/wave", "ChatCommand_Help_Wave", "ChatCommand_HelpSimple_Wave", MyPromoteLevel.None)]
        private static void CommandWave(string[] args)
        {
            if (m_waveAnimation == null)
            {
                LoadAnimations();
            }
            Activate(m_waveAnimation);
        }

        private static void LoadAnimations()
        {
            foreach (MyAnimationDefinition definition in MyDefinitionManager.Static.GetAnimationDefinitions())
            {
                MyStringId? displayNameEnum = definition.DisplayNameEnum;
                MyStringId orCompute = MyStringId.GetOrCompute("DisplayName_Animation_Wave");
                if ((displayNameEnum != null) ? ((displayNameEnum != null) ? (displayNameEnum.GetValueOrDefault() == orCompute) : true) : false)
                {
                    m_waveAnimation = definition;
                    continue;
                }
                displayNameEnum = definition.DisplayNameEnum;
                orCompute = MyStringId.GetOrCompute("DisplayName_Animation_Facepalm");
                if ((displayNameEnum != null) ? ((displayNameEnum != null) ? (displayNameEnum.GetValueOrDefault() == orCompute) : true) : false)
                {
                    m_facepalmAnimation = definition;
                    continue;
                }
                displayNameEnum = definition.DisplayNameEnum;
                orCompute = MyStringId.GetOrCompute("DisplayName_Animation_ThumbUp");
                if ((displayNameEnum != null) ? ((displayNameEnum != null) ? (displayNameEnum.GetValueOrDefault() == orCompute) : true) : false)
                {
                    m_thumbsupAnimation = definition;
                    continue;
                }
                displayNameEnum = definition.DisplayNameEnum;
                orCompute = MyStringId.GetOrCompute("DisplayName_Animation_Victory");
                if ((displayNameEnum != null) ? ((displayNameEnum != null) ? (displayNameEnum.GetValueOrDefault() == orCompute) : true) : false)
                {
                    m_victoryAnimation = definition;
                }
            }
        }
    }
}

