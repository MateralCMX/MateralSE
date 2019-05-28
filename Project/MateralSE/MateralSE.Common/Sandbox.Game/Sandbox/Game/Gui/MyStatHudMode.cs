namespace Sandbox.Game.GUI
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Input;
    using VRage.Utils;

    public class MyStatHudMode : MyStatBase
    {
        public MyStatHudMode()
        {
            base.Id = MyStringHash.GetOrCompute("hud_mode");
            base.CurrentValue = MySandboxGame.Config.HudState;
        }

        public override void Update()
        {
            base.CurrentValue = MyHud.HudState;
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TOGGLE_HUD) && (MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay))
            {
                base.CurrentValue++;
                if (base.CurrentValue > 2f)
                {
                    base.CurrentValue = 0f;
                }
                MyHud.HudState = (int) base.CurrentValue;
                MyHud.MinimalHud = MyHud.IsHudMinimal;
            }
        }
    }
}

