namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiControlRating : MyGuiControlBase
    {
        private Vector2 m_textureSize;
        private float m_space;
        private Vector2 m_position;
        private int m_value;
        private int m_maxValue;
        public string EmptyTexture;
        public string FilledTexture;
        public string HalfFilledTexture;

        public MyGuiControlRating(int value = 0, int maxValue = 10) : base(nullable, nullable, nullable2, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_textureSize = new Vector2(32f);
            this.m_space = 8f;
            this.EmptyTexture = @"Textures\GUI\Icons\Rating\NoStar.png";
            this.FilledTexture = @"Textures\GUI\Icons\Rating\FullStar.png";
            this.HalfFilledTexture = @"Textures\GUI\Icons\Rating\HalfStar.png";
            Vector2? nullable = null;
            nullable = null;
            this.m_value = value;
            this.m_maxValue = maxValue;
            base.BackgroundTexture = null;
            base.ColorMask = Vector4.One;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            if (this.MaxValue > 0)
            {
                Vector2 normalizedSize = MyGuiManager.GetHudNormalizedSizeFromPixelSize(this.m_textureSize) * new Vector2(0.75f, 1f);
                Vector2 hudNormalizedSizeFromPixelSize = MyGuiManager.GetHudNormalizedSizeFromPixelSize(new Vector2(this.m_space * 0.75f, 0f));
                Vector2 vector3 = base.GetPositionAbsoluteTopLeft() + new Vector2(0f, (base.Size.Y - normalizedSize.Y) / 2f);
                Vector2 vector4 = new Vector2((normalizedSize.X + hudNormalizedSizeFromPixelSize.X) * 0.5f, normalizedSize.Y);
                for (int i = 0; i < this.MaxValue; i += 2)
                {
                    Vector2 normalizedCoord = vector3 + new Vector2(vector4.X * i, 0f);
                    if (i == (this.Value - 1))
                    {
                        MyGuiManager.DrawSpriteBatch(this.HalfFilledTexture, normalizedCoord, normalizedSize, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                    }
                    else if (i < this.Value)
                    {
                        MyGuiManager.DrawSpriteBatch(this.FilledTexture, normalizedCoord, normalizedSize, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                    }
                    else
                    {
                        MyGuiManager.DrawSpriteBatch(this.EmptyTexture, normalizedCoord, normalizedSize, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
                    }
                }
            }
        }

        private void RecalculateSize()
        {
            Vector2 vector = MyGuiManager.GetHudNormalizedSizeFromPixelSize(this.m_textureSize) * new Vector2(0.75f, 1f);
            Vector2 hudNormalizedSizeFromPixelSize = MyGuiManager.GetHudNormalizedSizeFromPixelSize(new Vector2(this.m_space * 0.75f, 0f));
            base.Size = new Vector2((vector.X + hudNormalizedSizeFromPixelSize.X) * this.m_maxValue, vector.Y);
        }

        public int MaxValue
        {
            get => 
                this.m_maxValue;
            set
            {
                this.m_maxValue = value;
                this.RecalculateSize();
            }
        }

        public int Value
        {
            get => 
                this.m_value;
            set => 
                (this.m_value = value);
        }
    }
}

