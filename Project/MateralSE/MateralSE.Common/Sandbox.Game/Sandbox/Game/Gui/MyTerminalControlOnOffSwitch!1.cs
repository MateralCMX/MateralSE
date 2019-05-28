namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRageMath;

    public class MyTerminalControlOnOffSwitch<TBlock> : MyTerminalValueControl<TBlock, bool>, IMyTerminalControlOnOffSwitch, IMyTerminalControl, IMyTerminalValueControl<bool>, ITerminalProperty, IMyTerminalControlTitleTooltip where TBlock: MyTerminalBlock
    {
        private MyGuiControlOnOffSwitch m_onOffSwitch;
        public MyStringId Title;
        public MyStringId OnText;
        public MyStringId OffText;
        public MyStringId Tooltip;
        private Action<MyGuiControlOnOffSwitch> m_valueChanged;

        public MyTerminalControlOnOffSwitch(string id, MyStringId title, MyStringId tooltip = new MyStringId(), MyStringId? on = new MyStringId?(), MyStringId? off = new MyStringId?()) : base(id)
        {
            this.Title = title;
            MyStringId? nullable = on;
            this.OnText = (nullable != null) ? nullable.GetValueOrDefault() : MySpaceTexts.SwitchText_On;
            nullable = off;
            this.OffText = (nullable != null) ? nullable.GetValueOrDefault() : MySpaceTexts.SwitchText_Off;
            this.Tooltip = tooltip;
            this.Serializer = (stream, value) => stream.Serialize(ref value);
        }

        private unsafe void AppendAction(MyTerminalAction<TBlock> action)
        {
            MyTerminalAction<TBlock>[] actions = base.Actions;
            MyTerminalAction<TBlock>[] actionArray = actions ?? new MyTerminalAction<TBlock>[0];
            MyTerminalAction<TBlock>[]* array = ref actionArray;
            Array.Resize<MyTerminalAction<TBlock>>(ref array, actionArray.Length + 1);
            actionArray[actionArray.Length - 1] = action;
            base.Actions = actionArray;
        }

        protected override MyGuiControlBase CreateGui()
        {
            this.m_onOffSwitch = new MyGuiControlOnOffSwitch(false, MyTexts.GetString(this.OnText), MyTexts.GetString(this.OffText));
            this.m_onOffSwitch.Size = new Vector2(MyTerminalControl<TBlock>.PREFERRED_CONTROL_WIDTH, this.m_onOffSwitch.Size.Y);
            this.m_valueChanged = new Action<MyGuiControlOnOffSwitch>(this.OnValueChanged);
            this.m_onOffSwitch.ValueChanged += this.m_valueChanged;
            MyGuiControlBlockProperty property = new MyGuiControlBlockProperty(MyTexts.GetString(this.Title), MyTexts.GetString(this.Tooltip), this.m_onOffSwitch, MyGuiControlBlockPropertyLayoutEnum.Vertical, false);
            property.Size = new Vector2(MyTerminalControl<TBlock>.PREFERRED_CONTROL_WIDTH, property.Size.Y);
            return property;
        }

        public MyTerminalAction<TBlock> EnableOffAction(string icon, StringBuilder name, StringBuilder onText, StringBuilder offText)
        {
            MyTerminalAction<TBlock> action = new MyTerminalAction<TBlock>(base.Id + "_Off", name, new Action<TBlock>(this.OffAction), delegate (TBlock x, StringBuilder r) {
                ((MyTerminalControlOnOffSwitch<TBlock>) this).Writer(x, r, onText, offText);
            }, icon);
            this.AppendAction(action);
            return action;
        }

        public MyTerminalAction<TBlock> EnableOnAction(string icon, StringBuilder name, StringBuilder onText, StringBuilder offText)
        {
            MyTerminalAction<TBlock> action = new MyTerminalAction<TBlock>(base.Id + "_On", name, new Action<TBlock>(this.OnAction), delegate (TBlock x, StringBuilder r) {
                ((MyTerminalControlOnOffSwitch<TBlock>) this).Writer(x, r, onText, offText);
            }, icon);
            this.AppendAction(action);
            return action;
        }

        public MyTerminalAction<TBlock> EnableToggleAction(string icon, StringBuilder name, StringBuilder onText, StringBuilder offText)
        {
            MyTerminalAction<TBlock> action = new MyTerminalAction<TBlock>(base.Id, name, new Action<TBlock>(this.SwitchAction), delegate (TBlock x, StringBuilder r) {
                ((MyTerminalControlOnOffSwitch<TBlock>) this).Writer(x, r, onText, offText);
            }, icon);
            this.AppendAction(action);
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

        private void OffAction(TBlock block)
        {
            this.SetValue(block, false);
        }

        private void OnAction(TBlock block)
        {
            this.SetValue(block, true);
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
            TBlock firstBlock = base.FirstBlock;
            if (firstBlock != null)
            {
                this.m_onOffSwitch.ValueChanged -= this.m_valueChanged;
                this.m_onOffSwitch.Value = this.GetValue(firstBlock);
                this.m_onOffSwitch.ValueChanged += this.m_valueChanged;
            }
        }

        private void OnValueChanged(MyGuiControlOnOffSwitch obj)
        {
            bool flag = obj.Value;
            foreach (TBlock local in base.TargetBlocks)
            {
                if (local.HasLocalPlayerAccess())
                {
                    this.SetValue(local, flag);
                }
            }
        }

        public override void SetValue(TBlock block, bool value)
        {
            base.SetValue(block, value);
        }

        private void SwitchAction(TBlock block)
        {
            this.SetValue(block, !this.GetValue(block));
        }

        private void Writer(TBlock block, StringBuilder result, StringBuilder onText, StringBuilder offText)
        {
            result.AppendStringBuilder(this.GetValue(block) ? onText : offText);
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

        MyStringId IMyTerminalControlOnOffSwitch.OnText
        {
            get => 
                this.OnText;
            set => 
                (this.OnText = value);
        }

        MyStringId IMyTerminalControlOnOffSwitch.OffText
        {
            get => 
                this.OffText;
            set => 
                (this.OffText = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalControlOnOffSwitch<TBlock>.<>c <>9;
            public static MyTerminalValueControl<TBlock, bool>.SerializerDelegate <>9__6_0;

            static <>c()
            {
                MyTerminalControlOnOffSwitch<TBlock>.<>c.<>9 = new MyTerminalControlOnOffSwitch<TBlock>.<>c();
            }

            internal void <.ctor>b__6_0(BitStream stream, ref bool value)
            {
                stream.Serialize(ref value);
            }
        }
    }
}

