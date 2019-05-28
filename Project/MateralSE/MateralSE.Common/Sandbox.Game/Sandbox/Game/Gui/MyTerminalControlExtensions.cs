namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Utils;

    public static class MyTerminalControlExtensions
    {
        private static StringBuilder Combine(MyStringId prefix, MyStringId title)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = MyTexts.Get(prefix);
            if (builder2.Length > 0)
            {
                builder.Append(builder2).Append(" ");
            }
            return builder.Append(MyTexts.GetString(title)).TrimTrailingWhitespace();
        }

        private static StringBuilder CombineOnOff(MyStringId title, MyStringId? on = new MyStringId?(), MyStringId? off = new MyStringId?())
        {
            MyStringId? nullable = on;
            nullable = off;
            return GetTitle(title).Append(" ").Append(MyTexts.GetString((nullable != null) ? nullable.GetValueOrDefault() : MySpaceTexts.SwitchText_On)).Append("/").Append(MyTexts.GetString((nullable != null) ? nullable.GetValueOrDefault() : MySpaceTexts.SwitchText_Off));
        }

        public static MyTerminalAction<TBlock> EnableAction<TBlock>(this MyTerminalControlCheckbox<TBlock> checkbox, Func<TBlock, bool> callable = null) where TBlock: MyTerminalBlock
        {
            MyStringId? on = null;
            on = null;
            StringBuilder name = CombineOnOff(checkbox.Title, on, on);
            MyTerminalAction<TBlock> action = checkbox.EnableAction(MyTerminalActionIcons.TOGGLE, name, MyTexts.Get(checkbox.OnText), MyTexts.Get(checkbox.OffText), null, null);
            action.Enabled = checkbox.Enabled;
            if (callable != null)
            {
                action.Callable = callable;
            }
            return action;
        }

        public static MyTerminalAction<TBlock> EnableAction<TBlock>(this MyTerminalControlButton<TBlock> button, string icon = null, MyStringId? title = new MyStringId?(), MyTerminalControl<TBlock>.WriterDelegate writer = null) where TBlock: MyTerminalBlock
        {
            MyStringId? nullable = title;
            return button.EnableAction(icon ?? MyTerminalActionIcons.TOGGLE, MyTexts.Get((nullable != null) ? nullable.GetValueOrDefault() : button.Title), writer);
        }

        public static void EnableActions<TBlock>(this MyTerminalControlSlider<TBlock> slider, float step = 0.05f, Func<TBlock, bool> enabled = null, Func<TBlock, bool> callable = null) where TBlock: MyTerminalBlock
        {
            slider.EnableActions<TBlock>(MyTerminalActionIcons.INCREASE, MyTerminalActionIcons.DECREASE, step, enabled, callable);
        }

        public static void EnableActions<TBlock>(this MyTerminalControlSlider<TBlock> slider, string increaseIcon, string decreaseIcon, float step = 0.05f, Func<TBlock, bool> enabled = null, Func<TBlock, bool> callable = null) where TBlock: MyTerminalBlock
        {
            StringBuilder increaseName = Combine(MySpaceTexts.ToolbarAction_Increase, slider.Title);
            slider.EnableActions(increaseIcon, decreaseIcon, increaseName, Combine(MySpaceTexts.ToolbarAction_Decrease, slider.Title), step, null, null, enabled, callable);
        }

        public static void EnableActionsWithReset<TBlock>(this MyTerminalControlSlider<TBlock> slider, float step = 0.05f) where TBlock: MyTerminalBlock
        {
            slider.EnableActionsWithReset<TBlock>(MyTerminalActionIcons.INCREASE, MyTerminalActionIcons.DECREASE, MyTerminalActionIcons.RESET, step);
        }

        public static void EnableActionsWithReset<TBlock>(this MyTerminalControlSlider<TBlock> slider, string increaseIcon, string decreaseIcon, string resetIcon, float step = 0.05f) where TBlock: MyTerminalBlock
        {
            StringBuilder increaseName = Combine(MySpaceTexts.ToolbarAction_Increase, slider.Title);
            StringBuilder resetName = Combine(MySpaceTexts.ToolbarAction_Reset, slider.Title);
            slider.EnableActions(increaseIcon, decreaseIcon, increaseName, Combine(MySpaceTexts.ToolbarAction_Decrease, slider.Title), step, resetIcon, resetName, null, null);
        }

        public static void EnableOnOffActions<TBlock>(this MyTerminalControlOnOffSwitch<TBlock> onOff) where TBlock: MyTerminalBlock
        {
            onOff.EnableOnOffActions<TBlock>(MyTerminalActionIcons.ON, MyTerminalActionIcons.OFF);
        }

        public static void EnableOnOffActions<TBlock>(this MyTerminalControlOnOffSwitch<TBlock> onOff, string onIcon, string OffIcon) where TBlock: MyTerminalBlock
        {
            StringBuilder onText = MyTexts.Get(onOff.OnText);
            StringBuilder offText = MyTexts.Get(onOff.OffText);
            onOff.EnableOnAction(onIcon, GetTitle(onOff.Title).Append(" ").Append(onText), onText, offText);
            onOff.EnableOffAction(OffIcon, GetTitle(onOff.Title).Append(" ").Append(offText), onText, offText);
        }

        public static MyTerminalAction<TBlock> EnableToggleAction<TBlock>(this MyTerminalControlOnOffSwitch<TBlock> onOff) where TBlock: MyTerminalBlock => 
            onOff.EnableToggleAction<TBlock>(MyTerminalActionIcons.TOGGLE);

        public static MyTerminalAction<TBlock> EnableToggleAction<TBlock>(this MyTerminalControlOnOffSwitch<TBlock> onOff, string iconPath) where TBlock: MyTerminalBlock
        {
            StringBuilder name = CombineOnOff(onOff.Title, new MyStringId?(onOff.OnText), new MyStringId?(onOff.OffText));
            return onOff.EnableToggleAction(iconPath, name, MyTexts.Get(onOff.OnText), MyTexts.Get(onOff.OffText));
        }

        private static StringBuilder GetTitle(MyStringId title)
        {
            StringBuilder builder = new StringBuilder();
            string str = MyTexts.GetString(title);
            if (str.Length > 0)
            {
                builder.Append(str);
            }
            return builder;
        }
    }
}

