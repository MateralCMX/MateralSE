namespace Sandbox.Gui.RichTextLabel
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyRichLabelLink : MyRichLabelText
    {
        private Action<string> m_onClick;
        private bool m_highlight;
        private int m_lastTimeClicked;
        private const string m_linkImgTex = @"Textures\GUI\link.dds";
        private MyRichLabelImage m_linkImg;
        private const float m_linkImgSpace = 0.008f;

        public MyRichLabelLink(string url, string text, float scale, Action<string> onClick)
        {
            base.Init(text, "Blue", scale, Vector4.Zero);
            this.Url = url;
            this.m_onClick = onClick;
            Vector2 normalizedSizeFromScreenSize = MyGuiManager.GetNormalizedSizeFromScreenSize(new Vector2(MyGuiManager.GetScreenSizeFromNormalizedSize(new Vector2(0.015f * scale), false).X));
            this.m_linkImg = new MyRichLabelImage(@"Textures\GUI\link.dds", normalizedSizeFromScreenSize, Vector4.One);
        }

        public override bool Draw(Vector2 position, float alphamask, ref int charactersLeft)
        {
            MyFontEnum enum2;
            Color powderBlue;
            if (this.m_highlight)
            {
                enum2 = "White";
                powderBlue = MyGuiConstants.LABEL_TEXT_COLOR;
            }
            else
            {
                enum2 = "Blue";
                powderBlue = Color.PowderBlue;
            }
            powderBlue *= alphamask;
            string str = base.Text.ToString(0, Math.Min(charactersLeft, base.Text.Length));
            charactersLeft -= base.Text.Length;
            base.m_tmpText.Clear();
            base.m_tmpText.Append(str);
            MyGuiManager.DrawString((string) enum2, base.m_tmpText, position, base.Scale, new Color?(powderBlue), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
            this.m_linkImg.Draw(position + new Vector2(base.Size.X + 0.008f, 0f), alphamask, ref charactersLeft);
            this.m_highlight = false;
            return true;
        }

        public override bool HandleInput(Vector2 position)
        {
            Vector2 mouseCursorPosition = MyGuiManager.MouseCursorPosition;
            if (((mouseCursorPosition.X <= (position.X + 0.001f)) || ((mouseCursorPosition.Y <= position.Y) || (mouseCursorPosition.X >= (position.X + this.Size.X)))) || (mouseCursorPosition.Y >= (position.Y + this.Size.Y)))
            {
                this.m_highlight = false;
            }
            else
            {
                this.m_highlight = true;
                if (MyInput.Static.IsLeftMousePressed() && ((MyGuiManager.TotalTimeInMilliseconds - this.m_lastTimeClicked) > MyGuiConstants.REPEAT_PRESS_DELAY))
                {
                    this.m_onClick(this.Url);
                    this.m_lastTimeClicked = MyGuiManager.TotalTimeInMilliseconds;
                    return true;
                }
            }
            return false;
        }

        public string Url { get; set; }

        public override Vector2 Size
        {
            get
            {
                Vector2 size = base.Size;
                Vector2 vector2 = this.m_linkImg.Size;
                return new Vector2((size.X + 0.008f) + vector2.X, Math.Max(size.Y, vector2.Y));
            }
        }
    }
}

