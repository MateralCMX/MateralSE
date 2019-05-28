namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRageMath;

    public class MyTerminalControlTextbox<TBlock> : MyTerminalValueControl<TBlock, StringBuilder>, ITerminalControlSync, IMyTerminalControlTextbox, IMyTerminalControl, IMyTerminalValueControl<StringBuilder>, ITerminalProperty, IMyTerminalControlTitleTooltip where TBlock: MyTerminalBlock
    {
        private char[] m_tmpArray;
        private MyGuiControlTextbox m_textbox;
        public SerializerDelegate<TBlock> Serializer;
        public MyStringId Title;
        public MyStringId Tooltip;
        private StringBuilder m_tmpText;
        private Action<MyGuiControlTextbox> m_textChanged;

        public MyTerminalControlTextbox(string id, MyStringId title, MyStringId tooltip) : base(id)
        {
            this.m_tmpArray = new char[0x40];
            this.m_tmpText = new StringBuilder(15);
            this.Title = title;
            this.Tooltip = tooltip;
            this.Serializer = (s, sb) => s.Serialize(sb, ref base.m_tmpArray, Encoding.UTF8);
        }

        protected override MyGuiControlBase CreateGui()
        {
            this.m_textbox = new MyGuiControlTextbox();
            this.m_textbox.Size = new Vector2(MyTerminalControl<TBlock>.PREFERRED_CONTROL_WIDTH, this.m_textbox.Size.Y);
            this.m_textChanged = new Action<MyGuiControlTextbox>(this.OnTextChanged);
            this.m_textbox.TextChanged += this.m_textChanged;
            MyGuiControlBlockProperty property = new MyGuiControlBlockProperty(MyTexts.GetString(this.Title), MyTexts.GetString(this.Tooltip), this.m_textbox, MyGuiControlBlockPropertyLayoutEnum.Vertical, true);
            property.Size = new Vector2(MyTerminalControl<TBlock>.PREFERRED_CONTROL_WIDTH, property.Size.Y);
            return property;
        }

        public override StringBuilder GetDefaultValue(TBlock block) => 
            new StringBuilder();

        public override StringBuilder GetMaximum(TBlock block) => 
            new StringBuilder();

        public override StringBuilder GetMinimum(TBlock block) => 
            new StringBuilder();

        public override StringBuilder GetValue(TBlock block) => 
            this.Getter(block);

        private void OnTextChanged(MyGuiControlTextbox obj)
        {
            this.m_tmpText.Clear();
            obj.GetText(this.m_tmpText);
            foreach (TBlock local in base.TargetBlocks)
            {
                this.SetValue(local, this.m_tmpText);
            }
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
            if (!this.m_textbox.IsImeActive)
            {
                TBlock firstBlock = base.FirstBlock;
                if (firstBlock != null)
                {
                    StringBuilder text = this.GetValue(firstBlock);
                    if (!this.m_textbox.TextEquals(text))
                    {
                        this.m_textbox.TextChanged -= this.m_textChanged;
                        this.m_textbox.SetText(text);
                        this.m_textbox.TextChanged += this.m_textChanged;
                    }
                }
            }
        }

        public void Serialize(BitStream stream, MyTerminalBlock block)
        {
        }

        public override void SetValue(TBlock block, StringBuilder value)
        {
            this.Setter(block, new StringBuilder(value.ToString()));
            block.NotifyTerminalValueChanged(this);
        }

        public GetterDelegate<TBlock> Getter { private get; set; }

        public SetterDelegate<TBlock> Setter { private get; set; }

        public Expression<Func<TBlock, StringBuilder>> MemberExpression
        {
            set
            {
                this.Getter = new GetterDelegate<TBlock>(value.CreateGetter<TBlock, StringBuilder>().Invoke);
                this.Setter = new SetterDelegate<TBlock>(value.CreateSetter<TBlock, StringBuilder>().Invoke);
            }
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

        Func<IMyTerminalBlock, StringBuilder> IMyTerminalValueControl<StringBuilder>.Getter
        {
            get
            {
                GetterDelegate<TBlock> oldGetter = this.Getter;
                return new Func<IMyTerminalBlock, StringBuilder>(class_1.<Sandbox.ModAPI.Interfaces.Terminal.IMyTerminalValueControl<System.Text.StringBuilder>.get_Getter>b__0);
            }
            set => 
                (this.Getter = new GetterDelegate<TBlock>(value.Invoke));
        }

        Action<IMyTerminalBlock, StringBuilder> IMyTerminalValueControl<StringBuilder>.Setter
        {
            get
            {
                SetterDelegate<TBlock> oldSetter = this.Setter;
                return new Action<IMyTerminalBlock, StringBuilder>(class_1.<Sandbox.ModAPI.Interfaces.Terminal.IMyTerminalValueControl<System.Text.StringBuilder>.get_Setter>b__0);
            }
            set => 
                (this.Setter = new SetterDelegate<TBlock>(value.Invoke));
        }

        public delegate StringBuilder GetterDelegate(TBlock block);

        public delegate void SerializerDelegate(BitStream stream, StringBuilder value);

        public delegate void SetterDelegate(TBlock block, StringBuilder value);
    }
}

