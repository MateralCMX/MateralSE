namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyBlueprintMenuControlHelper : MyAbstractControlMenuItem
    {
        public MyBlueprintMenuControlHelper() : base("F10", MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            if (MyFakes.I_AM_READY_FOR_NEW_BLUEPRINT_SCREEN)
            {
                MyGuiSandbox.AddScreen(MyGuiBlueprintScreen_Reworked.CreateBlueprintScreen(MyClipboardComponent.Static.Clipboard, MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(Sync.MyId), MyBlueprintAccessType.NORMAL));
            }
            else
            {
                MyGuiSandbox.AddScreen(new MyGuiBlueprintScreen(MyClipboardComponent.Static.Clipboard, MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(Sync.MyId), MyBlueprintAccessType.NORMAL));
            }
        }

        public override string Label =>
            MyTexts.GetString(MySpaceTexts.ControlMenuItemLabel_ShowBlueprints);
    }
}

