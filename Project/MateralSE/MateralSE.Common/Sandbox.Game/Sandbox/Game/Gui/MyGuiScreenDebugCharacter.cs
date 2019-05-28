namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Definitions.Animation;
    using VRageMath;

    [MyDebugScreen("VRage", "Character")]
    internal class MyGuiScreenDebugCharacter : MyGuiScreenDebugBase
    {
        private MyGuiControlCombobox m_animationComboA;
        private MyGuiControlCombobox m_animationComboB;
        private MyGuiControlSlider m_blendSlider;
        private MyGuiControlCombobox m_animationCombo;
        private MyGuiControlCheckbox m_loopCheckbox;

        public MyGuiScreenDebugCharacter() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override bool CloseScreen()
        {
            if (MySession.Static != null)
            {
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
            }
            return base.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugCharacter";

        private void OnPlayBlendButtonClick(MyGuiControlButton sender)
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            localCharacter.PlayCharacterAnimation(this.m_animationComboA.GetSelectedKey().ToString(), MyBlendOption.Immediate, MyFrameOption.PlayOnce, this.m_blendSlider.Value, 1f, false, null, false);
            localCharacter.PlayCharacterAnimation(this.m_animationComboB.GetSelectedKey().ToString(), MyBlendOption.WaitForPreviousEnd, MyFrameOption.Loop, this.m_blendSlider.Value, 1f, false, null, false);
        }

        private void OnPlayButtonClick(MyGuiControlButton sender)
        {
            if (!MySession.Static.LocalCharacter.UseNewAnimationSystem)
            {
                MySession.Static.LocalCharacter.PlayCharacterAnimation(this.m_animationCombo.GetSelectedValue().ToString(), MyBlendOption.Immediate, this.m_loopCheckbox.IsChecked ? MyFrameOption.Loop : MyFrameOption.PlayOnce, this.m_blendSlider.Value, 1f, false, null, false);
            }
            else
            {
                MySession.Static.LocalCharacter.TriggerCharacterAnimationEvent("play", false);
                MySession.Static.LocalCharacter.TriggerCharacterAnimationEvent(this.m_animationCombo.GetSelectedValue().ToString(), false);
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Render Character", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            if (((MySession.Static == null) || (MySession.Static.ControlledEntity == null)) || !(MySession.Static.ControlledEntity is MyCharacter))
            {
                base.AddLabel("None active character", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            }
            else
            {
                int? nullable3;
                MyCharacter playerCharacter = MySession.Static.LocalCharacter;
                if (constructor)
                {
                    MyAnimationControllerDefinition animControllerDefinition = MyDefinitionManagerBase.Static.GetDefinition<MyAnimationControllerDefinition>("Debug");
                    if (animControllerDefinition == null)
                    {
                        return;
                    }
                    playerCharacter.AnimationController.Clear();
                    playerCharacter.AnimationController.InitFromDefinition(animControllerDefinition, true);
                    if (playerCharacter.AnimationController.ReloadBonesNeeded != null)
                    {
                        playerCharacter.AnimationController.ReloadBonesNeeded();
                    }
                }
                Vector4? color = null;
                base.AddSlider("Max slope", playerCharacter.Definition.MaxSlope, 0f, 89f, (Action<MyGuiControlSlider>) (slider => (playerCharacter.Definition.MaxSlope = slider.Value)), color);
                base.AddLabel(playerCharacter.Model.AssetName, Color.Yellow.ToVector4(), 1.2f, null, "Debug");
                base.AddLabel("Animation A:", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
                color = null;
                captionOffset = null;
                this.m_animationComboA = base.AddCombo(null, color, captionOffset, 10);
                ListReader<MyAnimationDefinition> animationDefinitions = MyDefinitionManager.Static.GetAnimationDefinitions();
                int num = 0;
                foreach (MyAnimationDefinition definition2 in animationDefinitions)
                {
                    num++;
                    nullable3 = null;
                    this.m_animationComboA.AddItem((long) num, new StringBuilder(definition2.Id.SubtypeName), nullable3, null);
                }
                this.m_animationComboA.SelectItemByIndex(0);
                base.AddLabel("Animation B:", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
                color = null;
                captionOffset = null;
                this.m_animationComboB = base.AddCombo(null, color, captionOffset, 10);
                num = 0;
                foreach (MyAnimationDefinition definition3 in animationDefinitions)
                {
                    num++;
                    nullable3 = null;
                    this.m_animationComboB.AddItem((long) num, new StringBuilder(definition3.Id.SubtypeName), nullable3, null);
                }
                this.m_animationComboB.SelectItemByIndex(0);
                color = null;
                this.m_blendSlider = base.AddSlider("Blend time", (float) 0.5f, (float) 0f, (float) 3f, color);
                color = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Play A->B"), new Action<MyGuiControlButton>(this.OnPlayBlendButtonClick), null, color, captionOffset, true, true);
                float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
                singlePtr2[0] += 0.01f;
                color = null;
                captionOffset = null;
                this.m_animationCombo = base.AddCombo(null, color, captionOffset, 10);
                num = 0;
                foreach (MyAnimationDefinition definition4 in animationDefinitions)
                {
                    num++;
                    nullable3 = null;
                    this.m_animationCombo.AddItem((long) num, new StringBuilder(definition4.Id.SubtypeName), nullable3, null);
                }
                this.m_animationCombo.SortItemsByValueText();
                this.m_animationCombo.SelectItemByIndex(0);
                color = null;
                captionOffset = null;
                this.m_loopCheckbox = base.AddCheckBox("Loop", false, (Action<MyGuiControlCheckbox>) null, true, null, color, captionOffset);
                float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
                singlePtr3[0] += 0.02f;
                foreach (string str in playerCharacter.Definition.BoneSets.Keys)
                {
                    color = null;
                    captionOffset = null;
                    MyGuiControlCheckbox checkbox = base.AddCheckBox(str, false, (Action<MyGuiControlCheckbox>) null, true, null, color, captionOffset);
                    checkbox.UserData = str;
                    if (str == "Body")
                    {
                        checkbox.IsChecked = true;
                    }
                }
                color = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Play animation"), new Action<MyGuiControlButton>(this.OnPlayButtonClick), null, color, captionOffset, true, true);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Draw damage and hit hapsules", (Func<bool>) (() => MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE), (Action<bool>) (s => (MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE = s)), true, null, color, captionOffset);
                float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
                singlePtr4[0] += 0.01f;
                color = null;
                this.AddSlider("Gravity mult", MyPerGameSettings.CharacterGravityMultiplier, 0f, 5f, (Action<MyGuiControlSlider>) (slider => (MyPerGameSettings.CharacterGravityMultiplier = slider.Value)), color);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugCharacter.<>c <>9 = new MyGuiScreenDebugCharacter.<>c();
            public static Func<bool> <>9__6_1;
            public static Action<bool> <>9__6_2;
            public static Action<MyGuiControlSlider> <>9__6_3;

            internal bool <RecreateControls>b__6_1() => 
                MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE;

            internal void <RecreateControls>b__6_2(bool s)
            {
                MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE = s;
            }

            internal void <RecreateControls>b__6_3(MyGuiControlSlider slider)
            {
                MyPerGameSettings.CharacterGravityMultiplier = slider.Value;
            }
        }
    }
}

