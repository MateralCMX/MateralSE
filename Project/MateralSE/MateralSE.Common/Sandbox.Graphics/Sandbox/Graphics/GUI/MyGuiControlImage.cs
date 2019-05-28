namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.Utils;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlImage))]
    public class MyGuiControlImage : MyGuiControlBase
    {
        private MyGuiBorderThickness m_padding;
        private StyleDefinition m_styleDefinition;

        public MyGuiControlImage() : this(nullable, nullable, nullable2, null, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlImage(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), string backgroundTexture = null, string[] textures = null, string toolTip = null, MyGuiDrawAlignEnum originAlign = 4) : base(position, size, backgroundColor, toolTip, texture1, false, false, false, MyGuiControlHighlightType.NEVER, originAlign)
        {
            MyGuiSizedTexture texture = new MyGuiSizedTexture {
                Texture = backgroundTexture
            };
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.Center = texture;
            base.Visible = true;
            this.SetTextures(textures);
        }

        public void ApplyStyle(StyleDefinition style)
        {
            this.m_styleDefinition = style;
            this.RefreshInternals();
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.DrawBackground(backgroundTransitionAlpha);
            if (this.Textures != null)
            {
                for (int i = 0; i < this.Textures.Length; i++)
                {
                    MyGuiManager.DrawSpriteBatch(this.Textures[i], base.GetPositionAbsoluteTopLeft() + (this.m_padding.TopLeftOffset / MyGuiConstants.GUI_OPTIMAL_SIZE), base.Size - (this.m_padding.SizeChange / MyGuiConstants.GUI_OPTIMAL_SIZE), ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                }
            }
            this.DrawElements(transitionAlpha, backgroundTransitionAlpha);
            base.DrawBorder(transitionAlpha);
        }

        public bool IsAnyTextureValid()
        {
            for (int i = 0; i < this.Textures.Length; i++)
            {
                if (!string.IsNullOrEmpty(this.Textures[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private void RefreshInternals()
        {
            if (this.m_styleDefinition != null)
            {
                base.BackgroundTexture = this.m_styleDefinition.BackgroundTexture;
                this.m_padding = this.m_styleDefinition.Padding;
            }
        }

        public void SetPadding(MyGuiBorderThickness padding)
        {
            this.m_padding = padding;
        }

        public void SetTexture(string texture = null)
        {
            string[] textArray3;
            if (texture == null)
            {
                textArray3 = new string[] { "" };
            }
            else
            {
                textArray3 = new string[] { texture };
            }
            this.Textures = textArray3;
        }

        public void SetTextures(string[] textures = null)
        {
            string[] textArray2 = textures;
            if (textures == null)
            {
                string[] local1 = textures;
                textArray2 = new string[] { "" };
            }
            this.Textures = textArray2;
        }

        public string[] Textures { get; private set; }

        public MyGuiBorderThickness Padding
        {
            get => 
                this.m_padding;
            set => 
                (this.m_padding = value);
        }

        public class StyleDefinition
        {
            public MyGuiCompositeTexture BackgroundTexture;
            public MyGuiBorderThickness Padding;
        }
    }
}

