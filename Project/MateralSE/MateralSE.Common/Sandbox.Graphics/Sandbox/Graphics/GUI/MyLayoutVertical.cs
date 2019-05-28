namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyLayoutVertical
    {
        private IMyGuiControlsParent m_parent;
        private Vector2 m_parentSize;
        private float m_currentPosY;
        private float m_horizontalPadding;
        public float CurrentY =>
            this.m_currentPosY;
        public float HorrizontalPadding =>
            this.m_horizontalPadding;
        public MyLayoutVertical(IMyGuiControlsParent parent, float horizontalPaddingPx)
        {
            this.m_parent = parent;
            Vector2? size = parent.GetSize();
            this.m_parentSize = (size != null) ? size.GetValueOrDefault() : Vector2.One;
            this.m_currentPosY = this.m_parentSize.Y * -0.5f;
            this.m_horizontalPadding = horizontalPaddingPx / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
        }

        public void Add(MyGuiControlBase control, MyAlignH align, bool advance = true)
        {
            this.AddInternal(control, align, MyAlignV.Top, advance, control.Size.Y);
        }

        public void Add(MyGuiControlBase control, float preferredWidthPx, float preferredHeightPx, MyAlignH align)
        {
            control.Size = new Vector2(preferredWidthPx, preferredHeightPx) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            this.Add(control, align, true);
        }

        public void Add(MyGuiControlBase leftControl, MyGuiControlBase rightControl)
        {
            float verticalSize = Math.Max(leftControl.Size.Y, rightControl.Size.Y);
            this.AddInternal(leftControl, MyAlignH.Left, MyAlignV.Center, false, verticalSize);
            this.AddInternal(rightControl, MyAlignH.Right, MyAlignV.Center, false, verticalSize);
            this.m_currentPosY += verticalSize;
        }

        public void Add(MyGuiControlBase leftControl, MyGuiControlBase centerControl, MyGuiControlBase rightControl)
        {
            float verticalSize = MathHelper.Max(leftControl.Size.Y, centerControl.Size.Y, rightControl.Size.Y);
            this.AddInternal(leftControl, MyAlignH.Left, MyAlignV.Center, false, verticalSize);
            this.AddInternal(centerControl, MyAlignH.Center, MyAlignV.Center, false, verticalSize);
            this.AddInternal(rightControl, MyAlignH.Right, MyAlignV.Center, false, verticalSize);
            this.m_currentPosY += verticalSize;
        }

        public void Advance(float advanceAmountPx)
        {
            this.m_currentPosY += advanceAmountPx / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
        }

        private void AddInternal(MyGuiControlBase control, MyAlignH alignH, MyAlignV alignV, bool advance, float verticalSize)
        {
            control.OriginAlign = (MyGuiDrawAlignEnum) (((MyAlignH.Right | MyAlignH.Center) * alignH) + ((MyAlignH) ((int) alignV)));
            int num = -1 + alignH;
            float num2 = (verticalSize * 0.5f) * ((float) alignV);
            control.Position = new Vector2(num * ((0.5f * this.m_parentSize.X) - this.m_horizontalPadding), this.m_currentPosY + num2);
            this.m_currentPosY += advance ? (verticalSize - num2) : 0f;
            this.m_parent.Controls.Add(control);
        }
    }
}

