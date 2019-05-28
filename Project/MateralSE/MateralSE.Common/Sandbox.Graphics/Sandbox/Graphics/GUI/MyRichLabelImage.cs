namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using VRage.Utils;
    using VRageMath;

    internal class MyRichLabelImage : MyRichLabelPart
    {
        private string m_texture;
        private Vector4 m_color;
        private Vector2 m_size;

        public MyRichLabelImage(string texture, Vector2 size, Vector4 color)
        {
            this.m_texture = texture;
            this.m_size = size;
            this.m_color = color;
        }

        public override bool Draw(Vector2 position, float alphamask, ref int charactersLeft)
        {
            Vector4 vector = this.m_color * alphamask;
            MyGuiManager.DrawSpriteBatch(this.m_texture, position, this.m_size, new VRageMath.Color(vector), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, true);
            charactersLeft--;
            return true;
        }

        public override bool HandleInput(Vector2 position) => 
            false;

        public string Texture
        {
            get => 
                this.m_texture;
            set => 
                (this.m_texture = value);
        }

        public Vector4 Color
        {
            get => 
                this.m_color;
            set => 
                (this.m_color = value);
        }

        public Vector2 Size
        {
            get => 
                base.Size;
            set => 
                (base.Size = value);
        }
    }
}

