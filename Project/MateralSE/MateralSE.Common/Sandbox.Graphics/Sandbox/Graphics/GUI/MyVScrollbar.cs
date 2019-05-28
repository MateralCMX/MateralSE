namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyVScrollbar : MyScrollbar
    {
        private Vector2 m_dragClick;

        public MyVScrollbar(MyGuiControlBase control) : base(control, MyGuiConstants.TEXTURE_SCROLLBAR_V_THUMB, MyGuiConstants.TEXTURE_SCROLLBAR_V_THUMB_HIGHLIGHT, MyGuiConstants.TEXTURE_SCROLLBAR_V_BACKGROUND)
        {
        }

        public override void Draw(Color colorMask)
        {
            if (base.Visible)
            {
                Vector2 positionLeftTop = base.OwnerControl.GetPositionAbsoluteCenter() + base.Position;
                base.m_backgroundTexture.Draw(positionLeftTop, base.Size, colorMask, 1f);
                if (base.CanScroll())
                {
                    base.Texture.Draw(positionLeftTop + this.GetCarretPosition(), base.CaretSize, colorMask, 1f);
                }
            }
        }

        private Vector2 GetCarretPosition() => 
            new Vector2(0f, (base.Value * (base.Size.Y - base.CaretSize.Y)) / (base.Max - base.Page));

        public override bool HandleInput()
        {
            bool flag = false;
            if (base.Visible && base.CanScroll())
            {
                Vector2 position = base.OwnerControl.GetPositionAbsoluteCenter() + base.Position;
                bool flag2 = MyGuiControlBase.CheckMouseOver(base.Size, position, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                bool isMouseOver = base.OwnerControl.IsMouseOver;
                base.IsOverCaret = MyGuiControlBase.CheckMouseOver(base.CaretSize, position + this.GetCarretPosition(), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                base.IsInDomainCaret = MyGuiControlBase.CheckMouseOver(base.Size, position, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                base.HasHighlight = base.IsOverCaret;
                switch (base.State)
                {
                    case MyScrollbar.StateEnum.Ready:
                        if (MyInput.Static.IsNewPrimaryButtonPressed() && base.IsOverCaret)
                        {
                            flag = true;
                            base.State = MyScrollbar.StateEnum.Drag;
                            this.m_dragClick = MyGuiManager.MouseCursorPosition;
                        }
                        else if (MyInput.Static.IsNewPrimaryButtonPressed() && base.IsInDomainCaret)
                        {
                            flag = true;
                            this.m_dragClick = MyGuiManager.MouseCursorPosition;
                            base.State = MyScrollbar.StateEnum.Click;
                        }
                        break;

                    case MyScrollbar.StateEnum.Drag:
                        if (!MyInput.Static.IsPrimaryButtonPressed())
                        {
                            base.State = MyScrollbar.StateEnum.Ready;
                        }
                        else
                        {
                            base.ChangeValue(((MyGuiManager.MouseCursorPosition.Y - this.m_dragClick.Y) * (base.Max - base.Page)) / (base.Size.Y - base.CaretSize.Y));
                            this.m_dragClick = MyGuiManager.MouseCursorPosition;
                        }
                        flag = true;
                        break;

                    case MyScrollbar.StateEnum.Click:
                    {
                        this.m_dragClick = MyGuiManager.MouseCursorPosition;
                        Vector2 vector2 = (this.GetCarretPosition() + position) + (base.CaretSize / 2f);
                        float amount = ((this.m_dragClick.Y - vector2.Y) * (base.Max - base.Page)) / (base.Size.Y - base.CaretSize.Y);
                        base.ChangeValue(amount);
                        base.State = MyScrollbar.StateEnum.Ready;
                        break;
                    }
                    default:
                        break;
                }
                if (flag2 | isMouseOver)
                {
                    int num2 = MyInput.Static.DeltaMouseScrollWheelValue();
                    if ((num2 != 0) && (num2 != -MyInput.Static.PreviousMouseScrollWheelValue()))
                    {
                        flag = true;
                        base.ChangeValue(((((float) num2) / -120f) * base.Page) * 0.25f);
                    }
                }
            }
            return flag;
        }

        public override void Layout(Vector2 positionTopLeft, float length)
        {
            base.Position = positionTopLeft;
            base.Size = new Vector2(base.Texture.MinSizeGui.X, length);
            if (base.CanScroll())
            {
                base.CaretSize = new Vector2(base.Texture.MinSizeGui.X, MathHelper.Clamp((base.Page / base.Max) * length, base.Texture.MinSizeGui.Y, base.Texture.MaxSizeGui.Y));
                base.CaretPageSize = new Vector2(base.Texture.MinSizeGui.X, MathHelper.Clamp(base.Page, base.Texture.MinSizeGui.Y, base.Texture.MaxSizeGui.Y));
                if (base.Value > (base.Max - base.Page))
                {
                    base.Value = base.Max - base.Page;
                }
            }
        }

        protected override void RefreshInternals()
        {
            base.RefreshInternals();
            base.Size = new Vector2(base.Texture.MinSizeGui.X, base.Size.Y);
        }
    }
}

