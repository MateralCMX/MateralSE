namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlScreenSwitchPanel : MyGuiControlParent
    {
        public unsafe MyGuiControlScreenSwitchPanel(MyGuiScreenBase owner, StringBuilder ownerDescription)
        {
            Vector2 vector = new Vector2(0.002f, 0.05f);
            Vector2 vector2 = new Vector2(50f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            Vector2 vector3 = new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText();
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            text1.Position = new Vector2(0.002f, 0.13f);
            Vector2? size = owner.Size;
            text1.Size = new Vector2(size.Value.X - 0.1f, 0.05f);
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            text1.Text = ownerDescription;
            text1.Font = "Blue";
            MyGuiControlMultilineText control = text1;
            size = null;
            VRageMath.Vector4? colorMask = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.ScreenCaptionNewGame);
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(new Vector2?(vector), MyGuiControlButtonStyleEnum.ToolbarButton, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_Campaign), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCampaignButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            float* singlePtr1 = (float*) ref vector.X;
            singlePtr1[0] += button.Size.X + MyGuiConstants.GENERIC_BUTTON_SPACING.X;
            size = null;
            colorMask = null;
            text = MyTexts.Get(MyCommonTexts.ScreenCaptionWorkshop);
            buttonIndex = null;
            MyGuiControlButton button2 = new MyGuiControlButton(new Vector2?(vector), MyGuiControlButtonStyleEnum.ToolbarButton, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_WorkshopContent), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnWorkshopButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            float* singlePtr2 = (float*) ref vector.X;
            singlePtr2[0] += button2.Size.X + MyGuiConstants.GENERIC_BUTTON_SPACING.X;
            size = null;
            colorMask = null;
            text = MyTexts.Get(MyCommonTexts.ScreenCaptionCustomWorld);
            buttonIndex = null;
            MyGuiControlButton button3 = new MyGuiControlButton(new Vector2?(vector), MyGuiControlButtonStyleEnum.ToolbarButton, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_CustomGame), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCustomWorldButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            colorMask = null;
            list.AddHorizontal(new Vector2(0f, 0.0305f), owner.Size.Value.X - (2f * vector3.X), 0f, colorMask);
            colorMask = null;
            list.AddHorizontal(new Vector2(0f, 0.098f), owner.Size.Value.X - (2f * vector3.X), 0f, colorMask);
            colorMask = null;
            list.AddHorizontal(new Vector2(0f, 0.166f), owner.Size.Value.X - (2f * vector3.X), 0f, colorMask);
            if (owner is MyGuiScreenNewGame)
            {
                owner.FocusedControl = button;
                button.HighlightType = MyGuiControlHighlightType.FORCED;
                button.HasHighlight = true;
                button.Selected = true;
                button.Name = "CampaignButton";
            }
            else if (owner is MyGuiScreenWorldSettings)
            {
                owner.FocusedControl = button3;
                button3.HighlightType = MyGuiControlHighlightType.FORCED;
                button3.HasHighlight = true;
                button3.Selected = true;
            }
            else if (((owner is MyGuiScreenLoadSubscribedWorld) || (owner is MyGuiScreenNewWorkshopGame)) && (button2 != null))
            {
                owner.FocusedControl = button2;
                button2.HighlightType = MyGuiControlHighlightType.FORCED;
                button2.HasHighlight = true;
                button2.Selected = true;
            }
            base.Controls.Add(control);
            base.Controls.Add(list);
            base.Controls.Add(button);
            base.Controls.Add(button3);
            if (button2 != null)
            {
                base.Controls.Add(button2);
            }
            base.Position = (-owner.Size.Value / 2f) + new Vector2(vector3.X, vector2.Y);
            owner.Controls.Add(this);
        }

        private void OnCampaignButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenNewGame))
            {
                SeamlesslyChangeScreen(screenWithFocus, new MyGuiScreenNewGame());
            }
        }

        private void OnCustomWorldButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenWorldSettings))
            {
                SeamlesslyChangeScreen(screenWithFocus, new MyGuiScreenWorldSettings());
            }
        }

        private void OnWorkshopButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenNewWorkshopGame))
            {
                SeamlesslyChangeScreen(screenWithFocus, new MyGuiScreenNewWorkshopGame());
            }
        }

        private static void SeamlesslyChangeScreen(MyGuiScreenBase focusedScreen, MyGuiScreenBase exchangedFor)
        {
            focusedScreen.SkipTransition = true;
            focusedScreen.CloseScreen();
            exchangedFor.SkipTransition = true;
            MyScreenManager.AddScreenNow(exchangedFor);
        }
    }
}

