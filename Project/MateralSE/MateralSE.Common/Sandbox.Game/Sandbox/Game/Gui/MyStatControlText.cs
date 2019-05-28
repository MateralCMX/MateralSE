namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyStatControlText : MyStatControlBase
    {
        private readonly StringBuilder m_text;
        private readonly StringBuilder m_textTmp;
        private string m_precisionFormat;
        private int m_precision;
        private MyFont m_font;
        private MyStringHash m_fontHash;
        private readonly bool m_hasStat;
        private const string STAT_TAG = "{STAT}";

        public MyStatControlText(MyStatControls parent, string text) : base(parent)
        {
            this.m_textTmp = new StringBuilder(0x80);
            this.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.Font = "Blue";
            this.Scale = 1f;
            this.TextColorMask = Vector4.One;
            this.m_text = new StringBuilder(MyTexts.SubstituteTexts(text, null));
            this.m_hasStat = this.m_text.ToString().Contains("{STAT}");
        }

        public override void Draw(float transitionAlpha)
        {
            StringBuilder text;
            Vector4 textColorMask = this.TextColorMask;
            if (base.BlinkBehavior.Blink && (base.BlinkBehavior.ColorMask != null))
            {
                textColorMask = base.BlinkBehavior.ColorMask.Value;
            }
            textColorMask = (Vector4) MyGuiControlBase.ApplyColorMaskModifiers(textColorMask, true, transitionAlpha);
            if (!this.m_hasStat)
            {
                text = this.m_text;
            }
            else
            {
                this.m_textTmp.Clear();
                this.m_textTmp.Append(this.m_text);
                this.m_textTmp.Replace("{STAT}", base.StatString);
                text = this.m_textTmp;
            }
            Vector2 size = this.m_font.MeasureString(text, this.Scale);
            Vector2 screenCoord = MyUtils.GetCoordTopLeftFromAligned(base.Position, size, this.TextAlign) + (base.Size / 2f);
            MyRenderProxy.DrawString((int) this.m_fontHash, screenCoord, textColorMask, text.ToString(), this.Scale, size.X + 100f, null);
        }

        public static string SubstituteTexts(string text, string context = null) => 
            MyTexts.SubstituteTexts(text, context);

        public float Scale { get; set; }

        public Vector4 TextColorMask { get; set; }

        public MyGuiDrawAlignEnum TextAlign { get; set; }

        public string Font
        {
            get => 
                this.m_fontHash.String;
            set
            {
                this.m_fontHash = MyStringHash.GetOrCompute(value);
                this.m_font = MyGuiManager.GetFont(this.m_fontHash);
            }
        }
    }
}

