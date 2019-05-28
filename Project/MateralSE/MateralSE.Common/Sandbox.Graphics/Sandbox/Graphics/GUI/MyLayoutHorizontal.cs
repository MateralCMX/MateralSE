namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyLayoutHorizontal
    {
        private IMyGuiControlsParent m_parent;
        private Vector2 m_parentSize;
        private float m_currentPosX;
        private float m_verticalPadding;
        public float CurrentX =>
            this.m_currentPosX;
        public float VerticalPadding =>
            this.m_verticalPadding;
        public MyLayoutHorizontal(IMyGuiControlsParent parent, float verticalPaddingPx)
        {
            this.m_parent = parent;
            Vector2? size = parent.GetSize();
            this.m_parentSize = (size != null) ? size.GetValueOrDefault() : Vector2.One;
            this.m_currentPosX = this.m_parentSize.X * -0.5f;
            this.m_verticalPadding = verticalPaddingPx / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
        }

        public void Add(MyGuiControlBase control, MyAlignV align, bool advance = true)
        {
            this.AddInternal(control, MyAlignH.Left, align, advance, control.Size.X);
        }

        public void Add(MyGuiControlBase control, float preferredHeightPx, float preferredWidthPx, MyAlignV align)
        {
            control.Size = new Vector2(preferredWidthPx, preferredHeightPx) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            this.Add(control, align, true);
        }

        public void Advance(float advanceAmountPx)
        {
            this.m_currentPosX += advanceAmountPx / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
        }

        private void AddInternal(MyGuiControlBase control, MyAlignH alignH, MyAlignV alignV, bool advance, float horizontalSize)
        {
            control.OriginAlign = (MyGuiDrawAlignEnum) (((MyAlignH.Right | MyAlignH.Center) * alignH) + ((MyAlignH) ((int) alignV)));
            int num = -1 + alignV;
            float num2 = (horizontalSize * 0.5f) * ((float) alignH);
            control.Position = new Vector2(this.m_currentPosX + num2, num * ((0.5f * this.m_parentSize.Y) - this.m_verticalPadding));
            this.m_currentPosX += advance ? (horizontalSize - num2) : 0f;
            this.m_parent.Controls.Add(control);
        }
    }
}

