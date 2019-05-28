namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlBlockProperty : MyGuiControlBase
    {
        private MyGuiControlBlockPropertyLayoutEnum m_layout;
        private MyGuiControlLabel m_title;
        private MyGuiControlLabel m_extraInfo;
        private MyGuiControlBase m_propertyControl;
        private float m_titleHeight;

        public MyGuiControlBlockProperty(string title, string tooltip, MyGuiControlBase propertyControl, MyGuiControlBlockPropertyLayoutEnum layout = 1, bool showExtraInfo = true) : base(position, position, colorMask, tooltip, null, false, true, true, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? position = null;
            position = null;
            Vector4? colorMask = null;
            position = null;
            position = null;
            colorMask = null;
            this.m_title = new MyGuiControlLabel(position, position, title, colorMask, 0.76f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            if (title.Length > 0)
            {
                base.Elements.Add(this.m_title);
            }
            position = null;
            position = null;
            colorMask = null;
            this.m_extraInfo = new MyGuiControlLabel(position, position, null, colorMask, 0.76f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            if (showExtraInfo)
            {
                base.Elements.Add(this.m_extraInfo);
            }
            this.m_propertyControl = propertyControl;
            base.Elements.Add(this.m_propertyControl);
            this.m_titleHeight = ((title.Length > 0) | showExtraInfo) ? this.m_title.Size.Y : 0f;
            this.m_layout = layout;
            if (layout == MyGuiControlBlockPropertyLayoutEnum.Horizontal)
            {
                base.MinSize = new Vector2(this.m_propertyControl.Size.X + (this.m_title.Size.X * 1.1f), Math.Max(this.m_propertyControl.Size.Y, 2.1f * this.m_titleHeight));
                this.m_title.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_propertyControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
                this.m_extraInfo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            }
            else if (layout == MyGuiControlBlockPropertyLayoutEnum.Vertical)
            {
                base.MinSize = new Vector2(Math.Max(this.m_propertyControl.Size.X, this.m_title.Size.X), this.m_propertyControl.Size.Y + (this.m_titleHeight * 1.8f));
                this.m_title.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_propertyControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_extraInfo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            }
            base.Size = base.MinSize;
            this.m_extraInfo.Text = "";
            this.m_extraInfo.Visible = false;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 == null)
            {
                base2 = base.HandleInputElements();
            }
            if ((base2 == null) && base.HasFocus)
            {
                base2 = this.m_propertyControl.HandleInput();
            }
            return base2;
        }

        public override void OnRemoving()
        {
            this.ClearEvents();
        }

        protected override void OnSizeChanged()
        {
            this.RefreshPositionsAndSizes();
            base.OnSizeChanged();
        }

        private void RefreshPositionsAndSizes()
        {
            MyGuiControlBlockPropertyLayoutEnum layout = this.m_layout;
            if (layout == MyGuiControlBlockPropertyLayoutEnum.Horizontal)
            {
                this.m_title.Position = new Vector2(base.Size.X * -0.5f, base.Size.Y * -0.25f);
                this.m_extraInfo.Position = this.m_title.Position + new Vector2(0f, this.m_titleHeight * 1.05f);
                this.m_propertyControl.Position = new Vector2(base.Size.X * 0.505f, base.Size.Y * -0.5f);
            }
            else if (layout == MyGuiControlBlockPropertyLayoutEnum.Vertical)
            {
                this.m_title.Position = base.Size * -0.5f;
                this.m_extraInfo.Position = base.Size * new Vector2(0.5f, -0.5f);
                this.m_propertyControl.Position = this.m_title.Position + new Vector2(0f, this.m_titleHeight * 1.05f);
            }
        }

        public MyGuiControlLabel TitleLabel =>
            this.m_title;

        public MyGuiControlBase PropertyControl =>
            this.m_propertyControl;

        public MyGuiControlLabel ExtraInfoLabel =>
            this.m_extraInfo;
    }
}

