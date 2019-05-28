namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRageMath;

    internal class MyTerminalControlColor<TBlock> : MyTerminalValueControl<TBlock, Color>, IMyTerminalControlColor, IMyTerminalControl, IMyTerminalValueControl<Color>, ITerminalProperty, IMyTerminalControlTitleTooltip where TBlock: MyTerminalBlock
    {
        public MyStringId Title;
        public MyStringId Tooltip;
        private MyGuiControlColor m_color;
        private Action<MyGuiControlColor> m_changeColor;

        public MyTerminalControlColor(string id, MyStringId title) : base(id)
        {
            this.Title = title;
            this.Serializer = (stream, value) => stream.Serialize(ref value.PackedValue, 0x20);
        }

        protected override MyGuiControlBase CreateGui()
        {
            this.m_color = new MyGuiControlColor(MyTexts.Get(this.Title).ToString(), 0.95f, Vector2.Zero, Color.White, Color.White, MyCommonTexts.DialogAmount_SetValueCaption, true, "Blue");
            this.m_changeColor = new Action<MyGuiControlColor>(this.OnChangeColor);
            this.m_color.OnChange += this.m_changeColor;
            this.m_color.Size = new Vector2(MyTerminalControl<TBlock>.PREFERRED_CONTROL_WIDTH, this.m_color.Size.Y);
            return new MyGuiControlBlockProperty(string.Empty, string.Empty, this.m_color, MyGuiControlBlockPropertyLayoutEnum.Vertical, true);
        }

        public override Color GetDefaultValue(TBlock block) => 
            new Color(VRageMath.Vector4.One);

        public override Color GetMaximum(TBlock block) => 
            new Color(VRageMath.Vector4.One);

        public override Color GetMinimum(TBlock block) => 
            new Color(VRageMath.Vector4.Zero);

        private void OnChangeColor(MyGuiControlColor obj)
        {
            foreach (TBlock local in base.TargetBlocks)
            {
                this.SetValue(local, obj.GetColor());
            }
        }

        protected override void OnUpdateVisual()
        {
            base.OnUpdateVisual();
            TBlock firstBlock = base.FirstBlock;
            if (firstBlock != null)
            {
                this.m_color.OnChange -= this.m_changeColor;
                this.m_color.SetColor(this.GetValue(firstBlock));
                this.m_color.OnChange += this.m_changeColor;
            }
        }

        public override void SetValue(TBlock block, Color value)
        {
            base.SetValue(block, new Color(VRageMath.Vector4.Clamp(value.ToVector4(), VRageMath.Vector4.Zero, VRageMath.Vector4.One)));
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
            public static readonly MyTerminalControlColor<TBlock>.<>c <>9;
            public static MyTerminalValueControl<TBlock, Color>.SerializerDelegate <>9__4_0;

            static <>c()
            {
                MyTerminalControlColor<TBlock>.<>c.<>9 = new MyTerminalControlColor<TBlock>.<>c();
            }

            internal void <.ctor>b__4_0(BitStream stream, ref Color value)
            {
                stream.Serialize(ref value.PackedValue, 0x20);
            }
        }
    }
}

