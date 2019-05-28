namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using VRageMath;
    using VRageRender;

    public class MyGuiProgressCompositeTextureAdvanced : MyGuiProgressCompositeTexture
    {
        private float[] phasesThresholds;

        public MyGuiProgressCompositeTextureAdvanced(MyGuiCompositeTexture texture)
        {
            base.LeftBottom = texture.LeftBottom;
            base.LeftCenter = texture.LeftCenter;
            base.LeftTop = texture.LeftTop;
            base.CenterBottom = texture.CenterBottom;
            base.Center = texture.Center;
            base.CenterTop = texture.CenterTop;
            base.RightBottom = texture.RightBottom;
            base.RightCenter = texture.RightCenter;
            base.RightTop = texture.RightTop;
        }

        public override void Draw(float progression, Color colorMask)
        {
            if (base.m_positionsAndSizesDirty)
            {
                this.RefreshPositionsAndSizes();
            }
            float single1 = MyMath.Clamp(progression, 0f, 1f);
            progression = single1;
            int index = 0;
            if (progression <= this.phasesThresholds[0])
            {
                index = 1;
            }
            if (progression <= this.phasesThresholds[1])
            {
                index = 2;
            }
            progression = (progression - this.phasesThresholds[index]) / (((index == 0) ? 1f : this.phasesThresholds[index - 1]) - this.phasesThresholds[index]);
            Vector2 one = Vector2.One;
            bool flag = false;
            MyGuiProgressCompositeTexture.BarOrientation orientation = base.Orientation;
            if (orientation == MyGuiProgressCompositeTexture.BarOrientation.HORIZONTAL)
            {
                one = new Vector2(progression, 1f);
                flag = false;
            }
            else if (orientation == MyGuiProgressCompositeTexture.BarOrientation.VERTICAL)
            {
                one = new Vector2(1f, progression);
                flag = true;
            }
            RectangleF dest = new RectangleF();
            Rectangle rectangle = new Rectangle();
            Rectangle? source = new Rectangle?(rectangle);
            int num2 = 0;
            while (num2 < 3)
            {
                int num3 = 0;
                while (true)
                {
                    if (num3 >= 3)
                    {
                        num2++;
                        break;
                    }
                    int num4 = flag ? num2 : num3;
                    if (base.IsInverted)
                    {
                        if (((index < 1) || (num4 != 0)) && ((index < 2) || (num4 != 1)))
                        {
                            MyGuiProgressCompositeTexture.TextureData texData = base.m_textures[num2, num3];
                            if (!string.IsNullOrEmpty(texData.Texture.Texture))
                            {
                                if ((index == 0) && (num4 == 0))
                                {
                                    this.SetTarget(ref dest, ref source, texData, one);
                                    MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                                }
                                else if ((index == 1) && (num4 == 1))
                                {
                                    this.SetTarget(ref dest, ref source, texData, one);
                                    MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                                }
                                else if ((index != 2) || (num4 != 2))
                                {
                                    this.SetTarget(ref dest, ref source, texData, Vector2.One);
                                    MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                                }
                                else
                                {
                                    this.SetTarget(ref dest, ref source, texData, one);
                                    MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                                }
                            }
                        }
                    }
                    else if (((index < 1) || (num4 != 2)) && ((index < 2) || (num4 != 1)))
                    {
                        MyGuiProgressCompositeTexture.TextureData texData = base.m_textures[num2, num3];
                        if (!string.IsNullOrEmpty(texData.Texture.Texture))
                        {
                            if ((index == 0) && (num4 == 2))
                            {
                                this.SetTarget(ref dest, ref source, texData, one);
                                MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                            }
                            else if ((index == 1) && (num4 == 1))
                            {
                                this.SetTarget(ref dest, ref source, texData, one);
                                MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                            }
                            else if ((index != 2) || (num4 != 0))
                            {
                                this.SetTarget(ref dest, ref source, texData, Vector2.One);
                                MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                            }
                            else
                            {
                                this.SetTarget(ref dest, ref source, texData, one);
                                MyRenderProxy.DrawSprite(texData.Texture.Texture, ref dest, false, ref source, colorMask, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
                            }
                        }
                    }
                    num3++;
                }
            }
        }

        protected override void RefreshPositionsAndSizes()
        {
            MyGuiProgressCompositeTexture.BarOrientation orientation;
            base.m_textures[0, 0].Position = base.m_position;
            int num = 0;
            while (num < 3)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= 3)
                    {
                        num++;
                        break;
                    }
                    base.m_textures[num, num2].Size = (Vector2I) MyGuiManager.GetScreenSizeFromNormalizedSize(base.m_textures[num, num2].Texture.SizeGui, false);
                    num2++;
                }
            }
            Vector2I vectori = (base.m_size - base.m_textures[0, 0].Size) - base.m_textures[2, 2].Size;
            base.m_textures[1, 0].Size.Y = vectori.Y;
            base.m_textures[1, 2].Size.Y = vectori.Y;
            base.m_textures[0, 1].Size.X = vectori.X;
            base.m_textures[2, 1].Size.X = vectori.X;
            base.m_textures[1, 1].Size = vectori;
            int num3 = 0;
            while (num3 < 3)
            {
                int num4 = 0;
                while (true)
                {
                    if (num4 >= 3)
                    {
                        num3++;
                        break;
                    }
                    if ((num3 != 0) || (num4 != 0))
                    {
                        int x = (num4 > 0) ? (base.m_textures[num3, num4 - 1].Position.X + base.m_textures[num3, num4 - 1].Size.X) : base.m_textures[0, 0].Position.X;
                        base.m_textures[num3, num4].Position = new Vector2I(x, (num3 > 0) ? (base.m_textures[num3 - 1, num4].Position.Y + base.m_textures[num3 - 1, num4].Size.Y) : base.m_textures[0, 0].Position.Y);
                    }
                    num4++;
                }
            }
            this.phasesThresholds = new float[3];
            if (base.IsInverted)
            {
                orientation = base.Orientation;
                if (orientation == MyGuiProgressCompositeTexture.BarOrientation.HORIZONTAL)
                {
                    this.phasesThresholds[0] = ((float) (base.m_textures[0, 2].Size.X + base.m_textures[0, 1].Size.X)) / ((float) base.m_size.X);
                    this.phasesThresholds[1] = ((float) base.m_textures[0, 2].Size.X) / ((float) base.m_size.X);
                    this.phasesThresholds[2] = 0f;
                }
                else if (orientation == MyGuiProgressCompositeTexture.BarOrientation.VERTICAL)
                {
                    this.phasesThresholds[0] = ((float) (base.m_textures[2, 0].Size.Y + base.m_textures[1, 0].Size.Y)) / ((float) base.m_size.Y);
                    this.phasesThresholds[1] = ((float) base.m_textures[2, 0].Size.Y) / ((float) base.m_size.Y);
                    this.phasesThresholds[2] = 0f;
                }
            }
            else
            {
                orientation = base.Orientation;
                if (orientation == MyGuiProgressCompositeTexture.BarOrientation.HORIZONTAL)
                {
                    this.phasesThresholds[0] = ((float) (base.m_textures[0, 0].Size.X + base.m_textures[0, 1].Size.X)) / ((float) base.m_size.X);
                    this.phasesThresholds[1] = ((float) base.m_textures[0, 0].Size.X) / ((float) base.m_size.X);
                    this.phasesThresholds[2] = 0f;
                }
                else if (orientation == MyGuiProgressCompositeTexture.BarOrientation.VERTICAL)
                {
                    this.phasesThresholds[0] = ((float) (base.m_textures[0, 0].Size.Y + base.m_textures[1, 0].Size.Y)) / ((float) base.m_size.Y);
                    this.phasesThresholds[1] = ((float) base.m_textures[0, 0].Size.Y) / ((float) base.m_size.Y);
                    this.phasesThresholds[2] = 0f;
                }
            }
            base.m_positionsAndSizesDirty = false;
        }

        protected void SetTarget(ref RectangleF dest, ref Rectangle? source, MyGuiProgressCompositeTexture.TextureData texData, Vector2 progress)
        {
            if (base.IsInverted)
            {
                dest.X = texData.Position.X + ((int) ((texData.Size.X * (1f - progress.X)) + 0.5f));
                dest.Y = texData.Position.Y + ((int) ((texData.Size.Y * (1f - progress.Y)) + 0.5f));
                dest.Width = (int) ((texData.Size.X * progress.X) + 0.5f);
                dest.Height = (int) ((texData.Size.Y * progress.Y) + 0.5f);
                source = new Rectangle((int) ((texData.Texture.SizePx.X * (1f - progress.X)) + 0.5f), (int) ((texData.Texture.SizePx.Y * (1f - progress.Y)) + 0.5f), (int) (texData.Texture.SizePx.X * progress.X), (int) (texData.Texture.SizePx.Y * progress.Y));
            }
            else
            {
                dest.X = texData.Position.X;
                dest.Y = texData.Position.Y;
                dest.Width = (int) (texData.Size.X * progress.X);
                dest.Height = (int) (texData.Size.Y * progress.Y);
                source = new Rectangle(0, 0, (int) (texData.Texture.SizePx.X * progress.X), (int) (texData.Texture.SizePx.Y * progress.Y));
            }
        }
    }
}

