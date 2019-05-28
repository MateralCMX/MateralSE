namespace SpaceEngineers.Game.GUI
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenOptionsSpace : MyGuiScreenBase
    {
        private MyGuiControlElementGroup m_elementGroup;

        public MyGuiScreenOptionsSpace() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.5200382f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenOptions";

        private void m_elementGroup_HighlightChanged(MyGuiControlElementGroup obj)
        {
            foreach (MyGuiControlBase base2 in this.m_elementGroup)
            {
                if (base2.HasFocus && !ReferenceEquals(obj.SelectedElement, base2))
                {
                    base.FocusedControl = obj.SelectedElement;
                    break;
                }
            }
        }

        public void OnBackClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void OnClickCredits(MyGuiControlButton sender)
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenGameCredits());
        }

        protected override void OnShow()
        {
            base.OnShow();
            base.m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
            base.m_guiTransition = MySandboxGame.Config.UIOpacity;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.m_elementGroup = new MyGuiControlElementGroup();
            this.m_elementGroup.HighlightChanged += new Action<MyGuiControlElementGroup>(this.m_elementGroup_HighlightChanged);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionOptions, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            base.m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
            base.m_guiTransition = MySandboxGame.Config.UIOpacity;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.83f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(-new Vector2((base.m_size.Value.X * 0.83f) / 2f, (-base.m_size.Value.Y / 2f) + 0.05f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(list2);
            int num = 0;
            num++;
            Vector2? size = null;
            captionTextColor = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.ScreenOptionsButtonGame);
            Vector2 vector1 = new Vector2(0.001f, (-base.m_size.Value.Y / 2f) + 0.126f);
            Vector2 vector2 = new Vector2(0.001f, (-base.m_size.Value.Y / 2f) + 0.126f);
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2?(new Vector2(0.001f, (-base.m_size.Value.Y / 2f) + 0.126f) + (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Game), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGame()), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            num++;
            size = null;
            captionTextColor = null;
            text = MyTexts.Get(MyCommonTexts.ScreenOptionsButtonDisplay);
            Vector2 local12 = vector2;
            Vector2 local13 = vector2;
            buttonIndex = null;
            button = new MyGuiControlButton(new Vector2?(vector2 + (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Display), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenOptionsDisplay()), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            num++;
            size = null;
            captionTextColor = null;
            text = MyTexts.Get(MyCommonTexts.ScreenOptionsButtonGraphics);
            Vector2 local10 = local13;
            Vector2 local11 = local13;
            buttonIndex = null;
            button = new MyGuiControlButton(new Vector2?(local13 + (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Graphics), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGraphics()), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            num++;
            size = null;
            captionTextColor = null;
            text = MyTexts.Get(MyCommonTexts.ScreenOptionsButtonAudio);
            Vector2 local8 = local11;
            Vector2 local9 = local11;
            buttonIndex = null;
            button = new MyGuiControlButton(new Vector2?(local11 + (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Audio), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenOptionsAudio()), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            num++;
            size = null;
            captionTextColor = null;
            text = MyTexts.Get(MyCommonTexts.ScreenOptionsButtonControls);
            Vector2 local6 = local9;
            Vector2 local7 = local9;
            buttonIndex = null;
            button = new MyGuiControlButton(new Vector2?(local9 + (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Controls), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenOptionsControls()), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            num++;
            size = null;
            captionTextColor = null;
            text = MyTexts.Get(MyCommonTexts.ScreenMenuButtonCredits);
            buttonIndex = null;
            button = new MyGuiControlButton(new Vector2?(local7 + (num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Credits), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => this.OnClickCredits(sender), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            base.CloseButtonEnabled = true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenOptionsSpace.<>c <>9 = new MyGuiScreenOptionsSpace.<>c();
            public static Action<MyGuiControlButton> <>9__2_0;
            public static Action<MyGuiControlButton> <>9__2_1;
            public static Action<MyGuiControlButton> <>9__2_2;
            public static Action<MyGuiControlButton> <>9__2_3;
            public static Action<MyGuiControlButton> <>9__2_4;

            internal void <RecreateControls>b__2_0(MyGuiControlButton sender)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGame());
            }

            internal void <RecreateControls>b__2_1(MyGuiControlButton sender)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenOptionsDisplay());
            }

            internal void <RecreateControls>b__2_2(MyGuiControlButton sender)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGraphics());
            }

            internal void <RecreateControls>b__2_3(MyGuiControlButton sender)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenOptionsAudio());
            }

            internal void <RecreateControls>b__2_4(MyGuiControlButton sender)
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenOptionsControls());
            }
        }
    }
}

