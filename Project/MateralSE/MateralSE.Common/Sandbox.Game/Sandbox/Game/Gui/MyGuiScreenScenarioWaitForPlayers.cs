namespace Sandbox.Game.GUI
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenScenarioWaitForPlayers : MyGuiScreenBase
    {
        private MyGuiControlLabel m_timeOutLabel;
        private MyGuiControlButton m_leaveButton;
        private StringBuilder m_tmpStringBuilder;

        public MyGuiScreenScenarioWaitForPlayers() : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
        {
            this.m_tmpStringBuilder = new StringBuilder();
            base.Size = new Vector2?(new Vector2(800f, 330f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            base.CloseButtonEnabled = false;
            base.m_closeOnEsc = false;
            this.RecreateControls(true);
            base.CanHideOthers = false;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenBattleWaitingConnectedPlayers";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.MainMenu, Array.Empty<object>()));
            }
        }

        private void OnLeaveClicked(MyGuiControlButton sender)
        {
            this.CloseScreen();
            MySessionLoader.UnloadAndExitToMenu();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MyStringId.GetOrCompute("Waiting for other players"), captionTextColor, captionOffset, 0.8f);
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            captionOffset = null;
            captionOffset = null;
            captionTextColor = null;
            this.m_timeOutLabel = new MyGuiControlLabel(captionOffset, captionOffset, null, captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            captionOffset = null;
            StringBuilder text = new StringBuilder("Leave");
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_leaveButton = new MyGuiControlButton(captionOffset, MyGuiControlButtonStyleEnum.Rectangular, new Vector2?(new Vector2(190f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnLeaveClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            MyLayoutTable table = new MyLayoutTable(this);
            table.SetColumnWidths(new float[] { 60f, 680f, 60f });
            table.SetRowHeights(new float[] { 110f, 65f, 65f, 65f, 65f, 65f });
            table.Add(new MyGuiControlLabel(captionOffset, captionOffset, "Game will start when all players join the world", captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER), MyAlignH.Center, MyAlignV.Center, 1, 1, 1, 1);
            table.Add(this.m_timeOutLabel, MyAlignH.Center, MyAlignV.Center, 2, 1, 1, 1);
            table.Add(this.m_leaveButton, MyAlignH.Center, MyAlignV.Center, 3, 1, 1, 1);
        }

        public override bool Update(bool hasFocus)
        {
            TimeSpan span = TimeSpan.FromSeconds(0.0);
            DateTime serverPreparationStartTime = MyScenarioSystem.Static.ServerPreparationStartTime;
            span = (TimeSpan) (DateTime.UtcNow - MyScenarioSystem.Static.ServerPreparationStartTime);
            span = TimeSpan.FromSeconds((double) MyScenarioSystem.LoadTimeout) - span;
            if (span.TotalMilliseconds < 0.0)
            {
                span = TimeSpan.FromSeconds(0.0);
            }
            string str = span.ToString(@"mm\:ss");
            this.m_tmpStringBuilder.Clear().Append("Timeout: ").Append(str);
            this.m_timeOutLabel.Text = this.m_tmpStringBuilder.ToString();
            return base.Update(hasFocus);
        }
    }
}

