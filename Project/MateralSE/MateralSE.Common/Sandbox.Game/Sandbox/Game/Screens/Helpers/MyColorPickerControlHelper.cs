namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.Screens;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;

    public class MyColorPickerControlHelper : MyAbstractControlMenuItem
    {
        public MyColorPickerControlHelper() : base(MyControlsSpace.LANDING_GEAR, MySupportKeysEnum.NONE | MySupportKeysEnum.SHIFT)
        {
        }

        public override void Activate()
        {
            MyScreenManager.CloseScreen(typeof(MyGuiScreenControlMenu));
            MyGuiScreenColorPicker screen = new MyGuiScreenColorPicker();
            MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
            MyGuiSandbox.AddScreen(screen);
        }

        public override string Label =>
            MyTexts.GetString(MyCommonTexts.ControlMenuItemLabel_ShowColorPicker);
    }
}

