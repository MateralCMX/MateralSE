namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyHScrollbar : MyScrollbar
    {
        private Vector2 m_dragClick;
        public bool EnableWheelScroll;

        public MyHScrollbar(MyGuiControlBase control) : base(control, MyGuiConstants.TEXTURE_SCROLLBAR_H_THUMB, MyGuiConstants.TEXTURE_SCROLLBAR_H_THUMB_HIGHLIGHT, MyGuiConstants.TEXTURE_SCROLLBAR_V_BACKGROUND)
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
                    base.Texture.Draw(positionLeftTop + this.GetCarretPosition(), base.CaretSize, colorMask, base.ScrollBarScale);
                }
            }
        }

        private Vector2 GetCarretPosition() => 
            new Vector2((base.Value * (base.Size.X - base.CaretSize.X)) / (base.Max - base.Page), 0f);

        public override bool HandleInput()
        {
            bool flag = false;
            if (base.Visible && base.CanScroll())
            {
                Vector2 position = base.OwnerControl.GetPositionAbsoluteCenter() + base.Position;
                base.IsOverCaret = MyGuiControlBase.CheckMouseOver(base.CaretSize, position + this.GetCarretPosition(), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                MyScrollbar.StateEnum state = base.State;
                if (state == MyScrollbar.StateEnum.Ready)
                {
                    if (MyInput.Static.IsNewLeftMousePressed() && base.IsOverCaret)
                    {
                        flag = true;
                        base.State = MyScrollbar.StateEnum.Drag;
                        this.m_dragClick = MyGuiManager.MouseCursorPosition;
                    }
                }
                else if (state == MyScrollbar.StateEnum.Drag)
                {
                    if (!MyInput.Static.IsLeftMousePressed())
                    {
                        base.State = MyScrollbar.StateEnum.Ready;
                    }
                    else
                    {
                        base.ChangeValue(((MyGuiManager.MouseCursorPosition.X - this.m_dragClick.X) * (base.Max - base.Page)) / (base.Size.X - base.CaretSize.X));
                        this.m_dragClick = MyGuiManager.MouseCursorPosition;
                    }
                    flag = true;
                }
                if (this.EnableWheelScroll && (MyGuiControlBase.CheckMouseOver(base.Size, position, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) | base.OwnerControl.IsMouseOver))
                {
                    base.Value += ((((float) MyInput.Static.DeltaMouseScrollWheelValue()) / -120f) * base.Page) * 0.25f;
                }
            }
            return flag;
        }

        public override void Layout(Vector2 positionTopLeft, float length)
        {
            base.Position = positionTopLeft;
            base.Size = new Vector2(length, base.Texture.MinSizeGui.Y);
            if (base.CanScroll())
            {
                base.CaretSize = new Vector2(MathHelper.Clamp((base.Page / base.Max) * length, base.Texture.MinSizeGui.X, base.Texture.MaxSizeGui.X), base.Texture.MinSizeGui.Y);
                if (base.Value > (base.Max - base.Page))
                {
                    base.Value = base.Max - base.Page;
                }
            }
        }

        protected override void RefreshInternals()
        {
            base.RefreshInternals();
            base.Size = new Vector2(base.Size.X, base.Texture.MinSizeGui.Y);
        }
    }
}

