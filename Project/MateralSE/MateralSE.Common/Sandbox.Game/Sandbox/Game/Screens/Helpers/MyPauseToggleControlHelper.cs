namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using System;
    using VRage;

    public class MyPauseToggleControlHelper : MyAbstractControlMenuItem
    {
        public MyPauseToggleControlHelper() : base(MyControlsSpace.PAUSE_GAME, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MySandboxGame.PauseToggle();
        }

        public override string Label =>
            MyTexts.GetString(MyCommonTexts.ControlMenuItemLabel_PauseGame);
    }
}

