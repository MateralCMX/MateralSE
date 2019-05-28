namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Utils;
    using VRageMath;

    internal class MyRichLabelText : MyRichLabelPart
    {
        protected StringBuilder m_tmpText;
        private StringBuilder m_text;
        private string m_font;
        private Vector4 m_color;
        private float m_scale;
        private bool m_showTextShadow;

        public MyRichLabelText()
        {
            this.m_tmpText = new StringBuilder();
            this.m_text = new StringBuilder(0x200);
            this.m_font = "Blue";
            this.m_scale = 0f;
            this.m_color = Vector4.Zero;
        }

        public MyRichLabelText(StringBuilder text, string font, float scale, Vector4 color)
        {
            this.m_tmpText = new StringBuilder();
            this.m_text = text;
            this.m_font = font;
            this.m_scale = scale;
            this.m_color = color;
            this.RecalculateSize();
        }

        public void Append(string text)
        {
            this.m_text.Append(text);
            this.RecalculateSize();
        }

        public override void AppendTextTo(StringBuilder builder)
        {
            builder.Append(this.m_text);
        }

        public override bool Draw(Vector2 position, float alphamask, ref int charactersLeft)
        {
            string str = this.m_text.ToString(0, Math.Min(this.m_text.Length, charactersLeft));
            charactersLeft -= this.m_text.Length;
            if (this.ShowTextShadow && !string.IsNullOrWhiteSpace(str))
            {
                Vector2 size = this.Size;
                MyGuiTextShadows.DrawShadow(ref position, ref size, null, alphamask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            }
            Vector4 vector = this.m_color * alphamask;
            this.m_tmpText.Clear();
            this.m_tmpText.Append(str);
            MyGuiManager.DrawString(this.m_font, this.m_tmpText, position, this.m_scale, new VRageMath.Color(vector), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
            return true;
        }

        public override bool HandleInput(Vector2 position) => 
            false;

        public void Init(string text, string font, float scale, Vector4 color)
        {
            this.m_text.Append(text);
            this.m_font = font;
            this.m_scale = scale;
            this.m_color = color;
            this.RecalculateSize();
        }

        private void RecalculateSize()
        {
            this.Size = MyGuiManager.MeasureString(this.m_font, this.m_text, this.m_scale);
        }

        public StringBuilder Text =>
            this.m_text;

        public bool ShowTextShadow
        {
            get => 
                this.m_showTextShadow;
            set => 
                (this.m_showTextShadow = value);
        }

        public float Scale
        {
            get => 
                this.m_scale;
            set
            {
                this.m_scale = value;
                this.RecalculateSize();
            }
        }

        public string Font
        {
            get => 
                this.m_font;
            set
            {
                this.m_font = value;
                this.RecalculateSize();
            }
        }

        public Vector4 Color
        {
            get => 
                this.m_color;
            set => 
                (this.m_color = value);
        }

        public string Tag { get; set; }
    }
}

