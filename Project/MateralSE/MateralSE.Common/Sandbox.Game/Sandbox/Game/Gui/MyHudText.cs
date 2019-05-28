namespace Sandbox.Game.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Utils;
    using VRageMath;

    public class MyHudText
    {
        public static readonly ComparerType Comparer = new ComparerType();
        public string Font;
        public Vector2 Position;
        public VRageMath.Color Color;
        public float Scale;
        public MyGuiDrawAlignEnum Alignement;
        public bool Visible;
        private readonly StringBuilder m_text = new StringBuilder(0x100);

        public void Append(string text)
        {
            this.m_text.Append(text);
        }

        public void Append(StringBuilder sb)
        {
            this.m_text.AppendStringBuilder(sb);
        }

        public void AppendInt32(int number)
        {
            this.m_text.AppendInt32(number);
        }

        public void AppendLine()
        {
            this.m_text.AppendLine();
        }

        public StringBuilder GetStringBuilder() => 
            this.m_text;

        public MyHudText Start(string font, Vector2 position, VRageMath.Color color, float scale, MyGuiDrawAlignEnum alignement)
        {
            this.Font = font;
            this.Position = position;
            this.Color = color;
            this.Scale = scale;
            this.Alignement = alignement;
            this.m_text.Clear();
            return this;
        }

        public class ComparerType : IComparer<MyHudText>
        {
            public int Compare(MyHudText x, MyHudText y) => 
                x.Font.CompareTo(y.Font);
        }
    }
}

