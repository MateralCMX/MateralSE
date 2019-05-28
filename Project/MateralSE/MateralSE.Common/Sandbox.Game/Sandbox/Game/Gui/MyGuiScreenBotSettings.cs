namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenBotSettings : MyGuiScreenBase
    {
        public MyGuiScreenBotSettings() : base(nullable, nullable2, nullable, false, null, 0f, 0f)
        {
            Vector2? nullable = null;
            nullable = null;
            base.Size = new Vector2?(new Vector2(650f, 350f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            base.BackgroundColor = new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR);
            this.RecreateControls(true);
            base.CanHideOthers = false;
        }

        private void enableDebuggingCheckBox_IsCheckedChanged(MyGuiControlCheckbox checkBox)
        {
            MyDebugDrawSettings.DEBUG_DRAW_BOTS = checkBox.IsChecked;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenBotSettings";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
        }

        private void nextButton_OnButtonClick(MyGuiControlButton button)
        {
            MyAIComponent.Static.DebugSelectNextBot();
        }

        private void OnCloseClicked(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        private void previousButton_OnButtonClick(MyGuiControlButton button)
        {
            MyAIComponent.Static.DebugSelectPreviousBot();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_position = new Vector2(MyGuiManager.GetMaxMouseCoord().X - 0.25f, 0.5f);
            MyLayoutVertical vertical = new MyLayoutVertical(this, 35f);
            vertical.Advance(20f);
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            vertical.Add(new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.BotSettingsScreen_Title), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER), MyAlignH.Center, true);
            vertical.Advance(30f);
            position = null;
            colorMask = null;
            MyGuiControlCheckbox control = new MyGuiControlCheckbox(position, colorMask, null, MyDebugDrawSettings.DEBUG_DRAW_BOTS, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.enableDebuggingCheckBox_IsCheckedChanged));
            position = null;
            position = null;
            colorMask = null;
            vertical.Add(new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.BotSettingsScreen_EnableBotsDebugging), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER), MyAlignH.Left, false);
            vertical.Add(control, MyAlignH.Right, true);
            vertical.Advance(15f);
            position = null;
            position = null;
            colorMask = null;
            int? buttonIndex = null;
            position = null;
            position = null;
            colorMask = null;
            buttonIndex = null;
            vertical.Add(new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.BotSettingsScreen_NextBot), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.nextButton_OnButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false), new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.BotSettingsScreen_PreviousBot), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.previousButton_OnButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false));
            vertical.Advance(30f);
            position = null;
            position = null;
            colorMask = null;
            buttonIndex = null;
            vertical.Add(new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Close), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCloseClicked), GuiSounds.MouseClick, 1f, buttonIndex, false), MyAlignH.Center, true);
        }
    }
}

