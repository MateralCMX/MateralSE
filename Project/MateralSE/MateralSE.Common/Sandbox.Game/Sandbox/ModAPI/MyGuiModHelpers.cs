namespace Sandbox.ModAPI
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;

    public class MyGuiModHelpers : IMyGui
    {
        public event Action<object> GuiControlCreated;

        public event Action<object> GuiControlRemoved;

        private Action<object> GetDelegate(Action<object> value) => 
            ((Action<object>) Delegate.CreateDelegate(typeof(Action<object>), value.Target, value.Method));

        public string ActiveGamePlayScreen
        {
            get
            {
                MyGuiScreenBase activeGameplayScreen = MyGuiScreenGamePlay.ActiveGameplayScreen;
                return ((activeGameplayScreen != null) ? activeGameplayScreen.Name : ((string) ((MyGuiScreenTerminal.GetCurrentScreen() == MyTerminalPageEnum.None) ? null : "MyGuiScreenTerminal")));
            }
        }

        public IMyEntity InteractedEntity =>
            MyGuiScreenTerminal.InteractedEntity;

        public MyTerminalPageEnum GetCurrentScreen =>
            MyGuiScreenTerminal.GetCurrentScreen();

        public bool ChatEntryVisible =>
            ((MyGuiScreenChat.Static != null) && ((MyGuiScreenChat.Static.ChatTextbox != null) && MyGuiScreenChat.Static.ChatTextbox.Visible));

        public bool IsCursorVisible =>
            MySandboxGame.Static.IsCursorVisible;
    }
}

