namespace Sandbox.Game.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;
    using VRageRender;

    public class MyGuiProgressSimpleTexture
    {
        private bool m_dirty;
        private MyObjectBuilder_GuiTexture m_backgroundTexture;
        private MyObjectBuilder_GuiTexture m_progressBarTexture;
        private Vector2I m_size;
        private Vector2I m_barSize;
        private Vector2I m_progressBarTextureOffset;
        private Vector2 m_zeroOrigin = Vector2.Zero;

        public void Draw(float progression, Color backgroundColorMask, Color progressColorMask)
        {
            Rectangle? sourceRectangle = null;
            RectangleF destination = new RectangleF();
            float single1 = MyMath.Clamp(progression, 0f, 1f);
            progression = single1;
            if (this.m_dirty)
            {
                this.RecalculateInternals();
            }
            destination.X = this.Position.X;
            destination.Y = this.Position.Y;
            destination.Width = this.m_size.X;
            destination.Height = this.m_size.Y;
            MyRenderProxy.DrawSprite(this.m_backgroundTexture.Path, ref destination, false, ref sourceRectangle, backgroundColorMask, 0f, Vector2.UnitX, ref this.m_zeroOrigin, SpriteEffects.None, 0f, true, null);
            Vector2I vectori = this.Position + this.m_progressBarTextureOffset;
            if (this.Inverted)
            {
                destination.X = vectori.X + (this.m_barSize.X * (1f - progression));
                destination.Y = vectori.Y;
                destination.Width = (int) (this.m_barSize.X * progression);
                destination.Height = this.m_barSize.Y;
                sourceRectangle = new Rectangle((int) (this.m_progressBarTexture.SizePx.X * (1f - progression)), 0, (int) (this.m_progressBarTexture.SizePx.X * progression), this.m_progressBarTexture.SizePx.Y);
            }
            else
            {
                destination.X = vectori.X;
                destination.Y = vectori.Y;
                destination.Width = (int) (this.m_barSize.X * progression);
                destination.Height = this.m_barSize.Y;
                sourceRectangle = new Rectangle(0, 0, (int) (this.m_progressBarTexture.SizePx.X * progression), this.m_progressBarTexture.SizePx.Y);
            }
            MyRenderProxy.DrawSprite(this.m_progressBarTexture.Path, ref destination, false, ref sourceRectangle, progressColorMask, 0f, Vector2.UnitX, ref this.m_zeroOrigin, SpriteEffects.None, 0f, true, null);
        }

        private void RecalculateInternals()
        {
            Vector2 vector = new Vector2(((float) this.m_size.X) / ((float) this.m_backgroundTexture.SizePx.X), ((float) this.m_size.Y) / ((float) this.m_backgroundTexture.SizePx.Y));
            this.m_barSize = new Vector2I((Vector2) (this.m_progressBarTexture.SizePx * vector));
            this.m_progressBarTextureOffset = new Vector2I((Vector2) (this.m_progressBarTextureOffset * vector));
            this.m_dirty = false;
        }

        public MyObjectBuilder_GuiTexture BackgroundTexture
        {
            get => 
                this.m_backgroundTexture;
            set
            {
                this.m_backgroundTexture = value;
                this.m_dirty = true;
            }
        }

        public MyObjectBuilder_GuiTexture ProgressBarTexture
        {
            get => 
                this.m_progressBarTexture;
            set
            {
                this.m_progressBarTexture = value;
                this.m_dirty = true;
            }
        }

        public Vector2I Size
        {
            get => 
                this.m_size;
            set
            {
                this.m_size = value;
                this.m_dirty = true;
            }
        }

        public Vector2I ProgressBarTextureOffset
        {
            get => 
                this.m_progressBarTextureOffset;
            set
            {
                this.m_progressBarTextureOffset = value;
                this.m_dirty = true;
            }
        }

        public Vector4 BackgroundColorMask { get; set; }

        public Vector4 ProgressBarColorMask { get; set; }

        public Vector2I Position { get; set; }

        public bool Inverted { get; set; }
    }
}

