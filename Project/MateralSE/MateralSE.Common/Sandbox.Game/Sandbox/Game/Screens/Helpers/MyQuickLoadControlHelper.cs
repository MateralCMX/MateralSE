namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyQuickLoadControlHelper : MyAbstractControlMenuItem
    {
        public MyQuickLoadControlHelper() : base("F5", MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            if (!Sync.IsServer)
            {
                MyGuiScreenGamePlay.Static.ShowReconnectMessageBox();
            }
            else if (!MyAsyncSaving.InProgress)
            {
                MyGuiScreenGamePlay.Static.ShowLoadMessageBox(MySession.Static.CurrentPath);
            }
            else
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextSavingInProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
                screen.SkipTransition = true;
                screen.InstantClose = false;
                MyGuiSandbox.AddScreen(screen);
            }
        }

        public override string Label =>
            MyTexts.GetString(MyCommonTexts.ControlMenuItemLabel_QuickLoad);
    }
}

