namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGuiHighlightTexture
    {
        public string Normal;
        public string Highlight;
        private Vector2 m_sizePx;
        public Vector2 SizePx
        {
            get => 
                this.m_sizePx;
            set
            {
                this.m_sizePx = value;
                this.SizeGui = this.m_sizePx / MyGuiConstants.GUI_OPTIMAL_SIZE;
            }
        }
        public Vector2 SizeGui { get; private set; }
    }
}

