namespace Sandbox.Game.Screens.Terminal.Controls
{
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using VRage;
    using VRage.Utils;

    public class MyTerminalControlLabel<TBlock> : MyTerminalControl<TBlock>, IMyTerminalControlLabel, IMyTerminalControl where TBlock: MyTerminalBlock
    {
        public MyStringId Label;
        private MyGuiControlLabel m_label;

        public MyTerminalControlLabel(MyStringId label) : base("Label")
        {
            this.Label = label;
        }

        protected override MyGuiControlBase CreateGui()
        {
            this.m_label = new MyGuiControlLabel();
            return new MyGuiControlBlockProperty(MyTexts.GetString(this.Label), null, this.m_label, MyGuiControlBlockPropertyLayoutEnum.Horizontal, true);
        }

        MyStringId IMyTerminalControlLabel.Label
        {
            get => 
                this.Label;
            set => 
                (this.Label = value);
        }
    }
}

