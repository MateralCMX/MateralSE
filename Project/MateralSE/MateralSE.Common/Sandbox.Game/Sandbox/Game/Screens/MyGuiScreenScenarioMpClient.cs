namespace Sandbox.Game.Screens
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;

    internal class MyGuiScreenScenarioMpClient : MyGuiScreenScenarioMpBase
    {
        public MyGuiScreenScenarioMpClient()
        {
            base.m_startButton.Enabled = false;
            MySyncScenario.InfoAnswer += new Action<bool, bool>(this.MySyncScenario_InfoAnswer);
            MySyncScenario.AskInfo();
        }

        public void MySyncScenario_InfoAnswer(bool gameAlreadyRunning, bool canJoinGame)
        {
            if (canJoinGame)
            {
                base.m_startButton.Enabled = gameAlreadyRunning;
            }
            else
            {
                StringBuilder messageCaption = MyTexts.Get(MySpaceTexts.GuiScenarioCannotJoinCaption);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyScreenManager.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.GuiScenarioCannotJoin), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, v => this.Canceling(), 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
            }
        }

        protected override void OnClosed()
        {
            MySyncScenario.InfoAnswer -= new Action<bool, bool>(this.MySyncScenario_InfoAnswer);
            base.OnClosed();
        }

        protected override void OnStartClicked(MyGuiControlButton sender)
        {
            MySyncScenario.OnPrepareScenarioFromLobby(-1L);
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_canJoinRunning.Enabled = false;
        }
    }
}

