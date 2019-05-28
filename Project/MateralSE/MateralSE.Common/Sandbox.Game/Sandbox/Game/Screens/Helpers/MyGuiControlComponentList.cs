namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiControlComponentList : MyGuiControlBase
    {
        private float m_currentOffsetFromTop;
        private MyGuiBorderThickness m_padding;
        private MyGuiControlLabel m_valuesLabel;

        public MyGuiControlComponentList() : base(nullable, nullable, nullable2, null, null, false, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
            this.m_padding = new MyGuiBorderThickness(0.02f, 0.008f);
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            label1.TextScale = 0.6f;
            this.m_valuesLabel = label1;
            base.Elements.Add(this.m_valuesLabel);
            this.UpdatePositions();
        }

        public void Add(MyDefinitionId id, double val1, double val2, string font)
        {
            ComponentControl control = new ComponentControl(id);
            control.Size = new Vector2(base.Size.X - this.m_padding.HorizontalSum, control.Size.Y);
            this.m_currentOffsetFromTop += control.Size.Y;
            control.Position = ((Vector2) (-0.5f * base.Size)) + new Vector2(this.m_padding.Left, this.m_currentOffsetFromTop);
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            control.ValuesFont = font;
            control.SetValues(val1, val2);
            base.Elements.Add(control);
        }

        public void Clear()
        {
            base.Elements.Clear();
            base.Elements.Add(this.m_valuesLabel);
            this.m_currentOffsetFromTop = this.m_valuesLabel.Size.Y + this.m_padding.Top;
        }

        protected override void OnSizeChanged()
        {
            this.UpdatePositions();
            base.OnSizeChanged();
        }

        private void UpdatePositions()
        {
            this.m_valuesLabel.Position = (base.Size * new Vector2(0.5f, -0.5f)) + this.m_padding.TopRightOffset;
            this.m_currentOffsetFromTop = this.m_valuesLabel.Size.Y + this.m_padding.Top;
            foreach (MyGuiControlBase base2 in base.Elements)
            {
                if (!ReferenceEquals(base2, this.m_valuesLabel))
                {
                    float y = base2.Size.Y;
                    this.m_currentOffsetFromTop += y;
                    base2.Position = ((Vector2) (-0.5f * base.Size)) + new Vector2(this.m_padding.Left, this.m_currentOffsetFromTop);
                    base2.Size = new Vector2(base.Size.X - this.m_padding.HorizontalSum, y);
                }
            }
        }

        public StringBuilder ValuesText
        {
            get => 
                new StringBuilder(this.m_valuesLabel.Text);
            set => 
                (this.m_valuesLabel.Text = value.ToString());
        }

        public ComponentControl this[int i] =>
            ((ComponentControl) base.Elements[i + 1]);

        public int Count =>
            (base.Elements.Count - 1);

        internal class ComponentControl : MyGuiControlBase
        {
            public readonly MyDefinitionId Id;
            private MyGuiControlComponentList.ItemIconControl m_iconControl;
            private MyGuiControlLabel m_nameLabel;
            private MyGuiControlLabel m_valuesLabel;

            internal ComponentControl(MyDefinitionId id) : base(position, new Vector2(0.2f, MyGuiConstants.TEXTURE_GRID_ITEM.SizeGui.Y * 0.75f), colorMask, null, null, false, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
            {
                Vector2? position = null;
                Vector4? colorMask = null;
                MyPhysicalItemDefinition def = (MyPhysicalItemDefinition) MyDefinitionManager.Static.GetDefinition(id);
                MyGuiControlComponentList.ItemIconControl control1 = new MyGuiControlComponentList.ItemIconControl(def);
                control1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_iconControl = control1;
                position = null;
                position = null;
                colorMask = null;
                MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, def.DisplayNameText, colorMask, 0.68f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label1.AutoEllipsis = true;
                this.m_nameLabel = label1;
                position = null;
                position = null;
                colorMask = null;
                this.m_valuesLabel = new MyGuiControlLabel(position, position, new StringBuilder("{0} / {1}").ToString(), colorMask, 0.6f, "White", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                this.SetValues(99.0, 99.0);
                base.Elements.Add(this.m_iconControl);
                base.Elements.Add(this.m_nameLabel);
                base.Elements.Add(this.m_valuesLabel);
                base.MinSize = new Vector2((this.m_iconControl.MinSize.X + this.m_nameLabel.Size.X) + this.m_valuesLabel.Size.X, this.m_iconControl.MinSize.Y);
            }

            protected override void OnSizeChanged()
            {
                this.m_iconControl.Position = base.Size * new Vector2(-0.5f, 0f);
                this.m_nameLabel.Position = this.m_iconControl.Position + new Vector2(this.m_iconControl.Size.X + 0.01f, 0f);
                this.m_valuesLabel.Position = base.Size * new Vector2(0.5f, 0f);
                this.UpdateNameLabelSize();
                base.OnSizeChanged();
            }

            public void SetValues(double val1, double val2)
            {
                object[] args = new object[] { val1.ToString("N", CultureInfo.InvariantCulture), val2.ToString("N", CultureInfo.InvariantCulture) };
                this.m_valuesLabel.UpdateFormatParams(args);
                this.UpdateNameLabelSize();
            }

            private void UpdateNameLabelSize()
            {
                this.m_nameLabel.Size = new Vector2(base.Size.X - (this.m_iconControl.Size.X + this.m_valuesLabel.Size.X), this.m_nameLabel.Size.Y);
            }

            public string ValuesFont
            {
                set => 
                    (this.m_valuesLabel.Font = value);
            }
        }

        private class ItemIconControl : MyGuiControlBase
        {
            private static readonly float SCALE = 0.85f;

            internal ItemIconControl(MyPhysicalItemDefinition def) : base(position, new Vector2?(MyGuiConstants.TEXTURE_GRID_ITEM.SizeGui * SCALE), backgroundColor, null, MyGuiConstants.TEXTURE_RECTANGLE_BUTTON_BORDER, false, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
            {
                Vector2? position = null;
                Vector4? backgroundColor = null;
                base.MinSize = base.MaxSize = base.Size;
                MyGuiBorderThickness thickness = new MyGuiBorderThickness(0.0025f, 0.001f);
                for (int i = 0; i < def.Icons.Length; i++)
                {
                    position = null;
                    backgroundColor = null;
                    base.Elements.Add(new MyGuiControlPanel(position, new Vector2?(base.Size - thickness.SizeChange), backgroundColor, def.Icons[0], null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER));
                }
                if (def.IconSymbol != null)
                {
                    position = null;
                    backgroundColor = null;
                    base.Elements.Add(new MyGuiControlLabel(new Vector2?(((Vector2) (-0.5f * base.Size)) + thickness.TopLeftOffset), position, MyTexts.GetString(def.IconSymbol.Value), backgroundColor, SCALE * 0.75f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP));
                }
            }
        }
    }
}

