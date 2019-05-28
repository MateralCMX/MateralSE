namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGuiSizedTexture
    {
        private Vector2 m_sizePx;
        private Vector2 m_sizeGui;
        public string Texture;
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
        public Vector2 SizeGui
        {
            get => 
                this.m_sizeGui;
            private set => 
                (this.m_sizeGui = value);
        }
        public MyGuiSizedTexture(MyGuiPaddedTexture original)
        {
            this.Texture = original.Texture;
            this.m_sizePx = original.SizePx;
            this.m_sizeGui = original.SizeGui;
        }
    }
}

