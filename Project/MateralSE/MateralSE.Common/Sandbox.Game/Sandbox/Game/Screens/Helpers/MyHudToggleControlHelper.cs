namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Gui;
    using System;
    using VRage;

    public class MyHudToggleControlHelper : MyAbstractControlMenuItem
    {
        private string m_value;

        public MyHudToggleControlHelper() : base(MyControlsSpace.TOGGLE_HUD, MySupportKeysEnum.NONE)
        {
        }

        public override void Activate()
        {
            MyHud.MinimalHud = !MyHud.MinimalHud;
        }

        public override void Next()
        {
            this.Activate();
        }

        public override void Previous()
        {
            this.Activate();
        }

        public override void UpdateValue()
        {
            if (!MyHud.MinimalHud)
            {
                this.m_value = MyTexts.GetString(MyCommonTexts.ControlMenuItemValue_On);
            }
            else
            {
                this.m_value = MyTexts.GetString(MyCommonTexts.ControlMenuItemValue_Off);
            }
        }

        public override string CurrentValue =>
            this.m_value;

        public override string Label =>
            MyTexts.GetString(MyCommonTexts.ControlMenuItemLabel_ToggleHud);
    }
}

