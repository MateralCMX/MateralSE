namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGuiPaddedTexture
    {
        public string Texture;
        private Vector2 m_sizePx;
        private Vector2 m_paddingSizePx;
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
        public Vector2 PaddingSizePx
        {
            get => 
                this.m_paddingSizePx;
            set
            {
                this.m_paddingSizePx = value;
                this.PaddingSizeGui = this.m_paddingSizePx / MyGuiConstants.GUI_OPTIMAL_SIZE;
            }
        }
        public Vector2 SizeGui { get; private set; }
        public Vector2 PaddingSizeGui { get; private set; }
    }
}

