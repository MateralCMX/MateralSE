namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;

    public class MyStatControlProgressBar : MyStatControlBase
    {
        private readonly MyGuiProgressCompositeTexture m_progressionCompositeTexture;
        private readonly MyGuiProgressSimpleTexture m_progressionSimpleTexture;

        public MyStatControlProgressBar(MyStatControls parent, MyObjectBuilder_CompositeTexture texture) : base(parent)
        {
            if (texture.IsValid())
            {
                this.m_progressionCompositeTexture = new MyGuiProgressCompositeTexture();
                MyObjectBuilder_GuiTexture texture2 = MyGuiTextures.Static.GetTexture(texture.LeftTop);
                MyGuiSizedTexture texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.LeftTop = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.LeftCenter);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.LeftCenter = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.LeftBottom);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.LeftBottom = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.CenterTop);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.CenterTop = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.Center);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.Center = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.CenterBottom);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.CenterBottom = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.RightTop);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.RightTop = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.RightCenter);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.RightCenter = texture3;
                texture2 = MyGuiTextures.Static.GetTexture(texture.RightBottom);
                texture3 = new MyGuiSizedTexture {
                    Texture = texture2.Path,
                    SizePx = (Vector2) texture2.SizePx
                };
                this.m_progressionCompositeTexture.RightBottom = texture3;
            }
        }

        public MyStatControlProgressBar(MyStatControls parent, MyObjectBuilder_GuiTexture background, MyObjectBuilder_GuiTexture progressBar, Vector2I progressBarOffset, Vector4? backgroundColorMask = new Vector4?(), Vector4? progressColorMask = new Vector4?()) : base(parent)
        {
            MyGuiProgressSimpleTexture texture1 = new MyGuiProgressSimpleTexture();
            texture1.BackgroundTexture = background;
            texture1.ProgressBarTexture = progressBar;
            texture1.ProgressBarTextureOffset = progressBarOffset;
            this.m_progressionSimpleTexture = texture1;
            Vector4? nullable = backgroundColorMask;
            this.m_progressionSimpleTexture.BackgroundColorMask = (nullable != null) ? nullable.GetValueOrDefault() : Vector4.One;
            nullable = progressColorMask;
            this.m_progressionSimpleTexture.ProgressBarColorMask = (nullable != null) ? nullable.GetValueOrDefault() : Vector4.One;
        }

        public override void Draw(float transitionAlpha)
        {
            float progression = 0f;
            if (base.StatMaxValue != 0f)
            {
                progression = base.StatCurrent / base.StatMaxValue;
            }
            Vector4 sourceColorMask = (this.m_progressionSimpleTexture != null) ? this.m_progressionSimpleTexture.ProgressBarColorMask : base.ColorMask;
            base.BlinkBehavior.UpdateBlink();
            if (base.BlinkBehavior.Blink)
            {
                transitionAlpha = base.BlinkBehavior.CurrentBlinkAlpha;
                if (base.BlinkBehavior.ColorMask != null)
                {
                    sourceColorMask = base.BlinkBehavior.ColorMask.Value;
                }
            }
            if (this.m_progressionCompositeTexture != null)
            {
                this.m_progressionCompositeTexture.Draw(progression, MyGuiControlBase.ApplyColorMaskModifiers(sourceColorMask, true, transitionAlpha));
            }
            if (this.m_progressionSimpleTexture != null)
            {
                this.m_progressionSimpleTexture.Draw(progression, this.m_progressionSimpleTexture.BackgroundColorMask, MyGuiControlBase.ApplyColorMaskModifiers(sourceColorMask, true, transitionAlpha));
            }
        }

        protected override void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            if (this.m_progressionCompositeTexture != null)
            {
                this.m_progressionCompositeTexture.Position = new Vector2I(newPosition);
            }
            if (this.m_progressionSimpleTexture != null)
            {
                this.m_progressionSimpleTexture.Position = new Vector2I(newPosition);
            }
        }

        protected override void OnSizeChanged(Vector2 oldSize, Vector2 newSize)
        {
            if (this.m_progressionCompositeTexture != null)
            {
                this.m_progressionCompositeTexture.Size = new Vector2I(newSize);
            }
            if (this.m_progressionSimpleTexture != null)
            {
                this.m_progressionSimpleTexture.Size = new Vector2I(newSize);
            }
        }

        public bool Inverted
        {
            get => 
                ((this.m_progressionSimpleTexture == null) ? ((this.m_progressionCompositeTexture != null) && this.m_progressionCompositeTexture.IsInverted) : this.m_progressionSimpleTexture.Inverted);
            set
            {
                if (this.m_progressionSimpleTexture != null)
                {
                    this.m_progressionSimpleTexture.Inverted = value;
                }
                if (this.m_progressionCompositeTexture != null)
                {
                    this.m_progressionCompositeTexture.IsInverted = value;
                }
            }
        }
    }
}

