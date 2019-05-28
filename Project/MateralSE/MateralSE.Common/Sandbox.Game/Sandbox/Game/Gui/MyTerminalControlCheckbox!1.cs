namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Library.Collections;
    using VRage.Utils;

    public class MyTerminalControlCheckbox<TBlock> : MyTerminalValueControl<TBlock, bool>, IMyTerminalControlCheckbox, IMyTerminalControl, IMyTerminalValueControl<bool>, ITerminalProperty, IMyTerminalControlTitleTooltip where TBlock: MyTerminalBlock
    {
        private Action<TBlock> m_action;
        private MyGuiControlCheckbox m_checkbox;
        private Action<MyGuiControlCheckbox> m_checkboxClicked;
        public MyStringId Title;
        public MyStringId OnText;
        public MyStringId OffText;
        public MyStringId Tooltip;

        public MyTerminalControlCheckbox(string id, MyStringId title, MyStringId tooltip, MyStringId? on = new MyStringId?(), MyStringId? off = new MyStringId?()) : base(id)
        {
            this.Title = title;
            MyStringId? nullable = on;
            this.OnText = (nullable != null) ? nullable.GetValueOrDefault() : MySpaceTexts.SwitchText_On;
            nullable = off;
            this.OffText = (nullable != null) ? nullable.GetValueOrDefault() : MySpaceTexts.SwitchText_Off;
            this.Tooltip = tooltip;
            this.Serializer = (stream, value) => stream.Serialize(ref value);
        }

        private void CheckAction(TBlock block)
        {
            this.SetValue(block, true);
        }

        protected override MyGuiControlBase CreateGui()
        {
            Vector2? position = null;
            Vector4? color = null;
            this.m_checkbox = new MyGuiControlCheckbox(position, color, MyTexts.GetString(this.Tooltip), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_checkboxClicked = new Action<MyGuiControlCheckbox>(this.OnCheckboxClicked);
            this.m_checkbox.IsCheckedChanged = this.m_checkboxClicked;
            return new MyGuiControlBlockProperty(MyTexts.GetString(this.Title), MyTexts.GetString(this.Tooltip), this.m_checkbox, MyGuiControlBlockPropertyLayoutEnum.Horizontal, true);
        }

        public MyTerminalAction<TBlock> EnableAction(string icon, StringBuilder name, StringBuilder onText, StringBuilder offText, Func<TBlock, bool> enabled = null, Func<TBlock, bool> callable = null)
        {
            MyTerminalAction<TBlock> action = new MyTerminalAction<TBlock>(base.Id, name, new Action<TBlock>(this.SwitchAction), delegate (TBlock x, StringBuilder r) {
                ((MyTerminalControlCheckbox<TBlock>) this).Writer(x, r, onText, offText);
            }, icon, enabled, callable);
            base.Actions = new MyTerminalAction<TBlock>[] { action };
            return action;
        }

        public override bool GetDefaultValue(TBlock block) => 
            false;

        public override bool GetMaximum(TBlock block) => 
            true;

        public override bool GetMinimum(TBlock block) => 
            false;

        public override bool GetValue(TBlock block) => 
            base.GetValue(block);

        private void OnCheckboxClicked(MyGuiControlCheckbox obj)
        {
            foreach (TBlock local in base.TargetBlocks)
            {
                this.SetValue(local, obj.IsChecked);
            }
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
            TBlock firstBlock = base.FirstBlock;
            if (firstBlock != null)
            {
                this.m_checkbox.IsCheckedChanged = null;
            }
            this.m_checkbox.IsChecked = this.GetValue(firstBlock);
            this.m_checkbox.IsCheckedChanged = this.m_checkboxClicked;
        }

        public override void SetValue(TBlock block, bool value)
        {
            base.SetValue(block, value);
        }

        private void SwitchAction(TBlock block)
        {
            this.SetValue(block, !this.GetValue(block));
        }

        private void UncheckAction(TBlock block)
        {
            this.SetValue(block, false);
        }

        private void Writer(TBlock block, StringBuilder result, StringBuilder onText, StringBuilder offText)
        {
            result.Append(this.GetValue(block) ? onText : offText);
        }

        MyStringId IMyTerminalControlCheckbox.OnText
        {
            get => 
                this.OnText;
            set => 
                (this.OnText = value);
        }

        MyStringId IMyTerminalControlCheckbox.OffText
        {
            get => 
                this.OffText;
            set => 
                (this.OffText = value);
        }

        MyStringId IMyTerminalControlTitleTooltip.Title
        {
            get => 
                this.Title;
            set => 
                (this.Title = value);
        }

        MyStringId IMyTerminalControlTitleTooltip.Tooltip
        {
            get => 
                this.Tooltip;
            set => 
                (this.Tooltip = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalControlCheckbox<TBlock>.<>c <>9;
            public static MyTerminalValueControl<TBlock, bool>.SerializerDelegate <>9__7_0;

            static <>c()
            {
                MyTerminalControlCheckbox<TBlock>.<>c.<>9 = new MyTerminalControlCheckbox<TBlock>.<>c();
            }

            internal void <.ctor>b__7_0(BitStream stream, ref bool value)
            {
                stream.Serialize(ref value);
            }
        }
    }
}

