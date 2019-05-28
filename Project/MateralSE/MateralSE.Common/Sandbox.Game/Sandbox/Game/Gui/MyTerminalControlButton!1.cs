namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;

    public class MyTerminalControlButton<TBlock> : MyTerminalControl<TBlock>, IMyTerminalControlButton, IMyTerminalControl, IMyTerminalControlTitleTooltip where TBlock: MyTerminalBlock
    {
        private Action<TBlock> m_action;
        private Action<MyGuiControlButton> m_buttonClicked;
        public MyStringId Title;
        public MyStringId Tooltip;

        public MyTerminalControlButton(string id, MyStringId title, MyStringId tooltip, Action<TBlock> action) : base(id)
        {
            this.Title = title;
            this.Tooltip = tooltip;
            this.m_action = action;
        }

        protected override MyGuiControlBase CreateGui()
        {
            Vector2? position = null;
            position = null;
            Vector4? colorMask = null;
            StringBuilder text = MyTexts.Get(this.Title);
            int? buttonIndex = null;
            this.m_buttonClicked = new Action<MyGuiControlButton>(this.OnButtonClicked);
            MyGuiControlButton button1 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(this.Tooltip), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.ButtonClicked += this.m_buttonClicked;
            return button1;
        }

        public MyTerminalAction<TBlock> EnableAction(string icon, StringBuilder name, MyTerminalControl<TBlock>.WriterDelegate writer = null)
        {
            MyTerminalAction<TBlock> action = new MyTerminalAction<TBlock>(base.Id, name, this.m_action, writer, icon);
            base.Actions = new MyTerminalAction<TBlock>[] { action };
            return action;
        }

        private void OnButtonClicked(MyGuiControlButton obj)
        {
            foreach (TBlock local in base.TargetBlocks)
            {
                if (this.m_action != null)
                {
                    this.m_action(local);
                }
            }
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
        }

        public Action<TBlock> Action
        {
            get => 
                this.m_action;
            set => 
                (this.m_action = value);
        }

        Action<IMyTerminalBlock> IMyTerminalControlButton.Action
        {
            get
            {
                Action<TBlock> oldAction = this.Action;
                return delegate (IMyTerminalBlock x) {
                    oldAction((TBlock) x);
                };
            }
            set => 
                (this.Action = (Action<TBlock>) value);
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
    }
}

