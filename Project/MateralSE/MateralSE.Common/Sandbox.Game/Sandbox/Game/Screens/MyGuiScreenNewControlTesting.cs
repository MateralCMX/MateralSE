namespace Sandbox.Game.Screens
{
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenNewControlTesting : MyGuiScreenBase
    {
        public MyGuiScreenNewControlTesting() : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.9f, 0.97f), false, null, 0f, 0f)
        {
            MyGuiControlSaveBrowser browser1 = new MyGuiControlSaveBrowser();
            browser1.Size = base.Size.Value - new Vector2(0.1f);
            browser1.Position = (-base.Size.Value / 2f) + new Vector2(0.05f);
            browser1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            browser1.VisibleRowsCount = 20;
            browser1.HeaderVisible = true;
            MyGuiControlSaveBrowser control = browser1;
            this.Controls.Add(control);
        }

        public override string GetFriendlyName() => 
            "TESTING!";
    }
}

