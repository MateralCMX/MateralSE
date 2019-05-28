namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Utils;
    using VRageMath;

    public class MyColoredText
    {
        private float m_scale;

        public MyColoredText(string text, Color? normalColor = new Color?(), Color? highlightColor = new Color?(), string font = "White", float textScale = 0.75f, Vector2? offset = new Vector2?())
        {
            this.Text = new StringBuilder(text.Length).Append(text);
            Color? nullable = normalColor;
            this.NormalColor = (nullable != null) ? nullable.GetValueOrDefault() : MyGuiConstants.COLORED_TEXT_DEFAULT_COLOR;
            nullable = highlightColor;
            this.HighlightColor = (nullable != null) ? nullable.GetValueOrDefault() : MyGuiConstants.COLORED_TEXT_DEFAULT_HIGHLIGHT_COLOR;
            this.Font = font;
            this.Scale = textScale;
            Vector2? nullable2 = offset;
            this.Offset = (nullable2 != null) ? nullable2.GetValueOrDefault() : Vector2.Zero;
        }

        public void Draw(Vector2 normalizedPosition, MyGuiDrawAlignEnum drawAlign, float backgroundAlphaFade, float colorMultiplicator = 1f)
        {
            this.Draw(normalizedPosition, drawAlign, backgroundAlphaFade, false, colorMultiplicator);
        }

        public unsafe void Draw(Vector2 normalizedPosition, MyGuiDrawAlignEnum drawAlign, float backgroundAlphaFade, bool isHighlight, float colorMultiplicator = 1f)
        {
            Vector4 vector = (isHighlight ? this.HighlightColor : this.NormalColor).ToVector4();
            float* singlePtr1 = (float*) ref vector.W;
            singlePtr1[0] *= backgroundAlphaFade;
            vector *= colorMultiplicator;
            MyGuiManager.DrawString(this.Font, this.Text, normalizedPosition + this.Offset, this.ScaleWithLanguage, new Color(vector), drawAlign, false, float.PositiveInfinity);
        }

        public StringBuilder Text { get; private set; }

        public Color NormalColor { get; private set; }

        public Color HighlightColor { get; private set; }

        public string Font { get; private set; }

        public Vector2 Offset { get; private set; }

        public float ScaleWithLanguage { get; private set; }

        public Vector2 Size { get; private set; }

        public float Scale
        {
            get => 
                this.m_scale;
            private set
            {
                this.m_scale = value;
                this.ScaleWithLanguage = value * MyGuiManager.LanguageTextScale;
                this.Size = MyGuiManager.MeasureString(this.Font, this.Text, this.ScaleWithLanguage);
            }
        }
    }
}

