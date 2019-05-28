namespace Sandbox.Graphics.GUI
{
    using ObjectBuilders.Definitions.GUI;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiCompositeTexture
    {
        private MyGuiSizedTexture m_leftTop;
        private MyGuiSizedTexture m_leftCenter;
        private MyGuiSizedTexture m_leftBottom;
        private MyGuiSizedTexture m_centerTop;
        private MyGuiSizedTexture m_center;
        private MyGuiSizedTexture m_centerBottom;
        private MyGuiSizedTexture m_rightTop;
        private MyGuiSizedTexture m_rightCenter;
        private MyGuiSizedTexture m_rightBottom;
        private bool m_sizeLimitsDirty;
        private Vector2 m_minSizeGui = Vector2.Zero;
        private Vector2 m_maxSizeGui = (Vector2.One * (float) 1.0 / (float) 0.0);

        public MyGuiCompositeTexture(string centerTexture = null)
        {
            MyGuiSizedTexture texture = new MyGuiSizedTexture {
                Texture = centerTexture
            };
            this.Center = texture;
        }

        public static MyGuiCompositeTexture CreateFromDefinition(MyStringHash textureKey)
        {
            MyObjectBuilder_GuiTexture texture3;
            MyGuiSizedTexture texture4;
            MyGuiCompositeTexture texture = new MyGuiCompositeTexture(null);
            MyObjectBuilder_CompositeTexture compositeTexture = MyGuiTextures.Static.GetCompositeTexture(textureKey);
            if (compositeTexture == null)
            {
                return null;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.Center, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.Center = texture4;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.LeftBottom, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.LeftBottom = texture4;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.LeftTop, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.LeftTop = texture4;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.RightCenter, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.RightCenter = texture4;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.RightBottom, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.RightBottom = texture4;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.RightTop, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.RightTop = texture4;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.CenterBottom, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.CenterBottom = texture4;
            }
            if (MyGuiTextures.Static.TryGetTexture(compositeTexture.CenterTop, out texture3))
            {
                texture4 = new MyGuiSizedTexture {
                    Texture = texture3.Path,
                    SizePx = (Vector2) texture3.SizePx
                };
                texture.CenterTop = texture4;
            }
            return texture;
        }

        public unsafe void Draw(Vector2 positionTopLeft, float innerHeight, Color colorMask)
        {
            Vector2 screenSizeFromNormalizedSize;
            Rectangle rectangle;
            Vector2 screenCoordinateFromNormalizedCoordinate = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(positionTopLeft, false);
            positionTopLeft = screenCoordinateFromNormalizedCoordinate;
            rectangle.X = (int) positionTopLeft.X;
            rectangle.Y = (int) positionTopLeft.Y;
            rectangle.Width = 0;
            rectangle.Height = 0;
            if (!string.IsNullOrEmpty(this.m_leftTop.Texture))
            {
                screenSizeFromNormalizedSize = MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_leftTop.SizeGui, false);
                rectangle.Width = (int) screenSizeFromNormalizedSize.X;
                rectangle.Height = (int) screenSizeFromNormalizedSize.Y;
                MyGuiManager.DrawSprite(this.m_leftTop.Texture, rectangle, colorMask, true);
            }
            int* numPtr1 = (int*) ref rectangle.Y;
            numPtr1[0] += rectangle.Height;
            if (!string.IsNullOrEmpty(this.m_leftCenter.Texture))
            {
                screenSizeFromNormalizedSize = MyGuiManager.GetScreenSizeFromNormalizedSize(new Vector2(this.m_leftCenter.SizeGui.X, innerHeight), false);
                rectangle.Width = (int) screenSizeFromNormalizedSize.X;
                rectangle.Height = (int) screenSizeFromNormalizedSize.Y;
                MyGuiManager.DrawSprite(this.m_leftCenter.Texture, rectangle, colorMask, true);
            }
            int* numPtr2 = (int*) ref rectangle.Y;
            numPtr2[0] += rectangle.Height;
            if (!string.IsNullOrEmpty(this.m_leftBottom.Texture))
            {
                screenSizeFromNormalizedSize = MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_leftBottom.SizeGui, false);
                rectangle.Width = (int) screenSizeFromNormalizedSize.X;
                rectangle.Height = (int) screenSizeFromNormalizedSize.Y;
                MyGuiManager.DrawSprite(this.m_leftBottom.Texture, rectangle, colorMask, true);
            }
        }

        public void Draw(Vector2 positionLeftTop, Vector2 size, Color colorMask, float textureScale = 1f)
        {
            Rectangle rectangle;
            Vector2I vectori7;
            Vector2I vectori8;
            Vector2I vectori10;
            Vector2I vectori12;
            Vector2I vectori13;
            Vector2 vector1 = Vector2.Clamp(size, this.MinSizeGui * textureScale, this.MaxSizeGui * textureScale);
            size = vector1;
            Vector2I screenSize = vectori7 = vectori8 = Vector2I.Zero;
            Vector2I vectori9 = vectori10 = Vector2I.Zero;
            Vector2I vectori11 = vectori12 = vectori13 = Vector2I.Zero;
            Vector2I vectori5 = new Vector2I(MyGuiManager.GetScreenSizeFromNormalizedSize(size, false));
            Vector2I screenPos = new Vector2I(MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(positionLeftTop, false));
            Vector2I vectori4 = screenPos + vectori5;
            Vector2I vectori2 = new Vector2I(screenPos.X, vectori4.Y);
            Vector2I vectori3 = new Vector2I(vectori4.X, screenPos.Y);
            if (!string.IsNullOrEmpty(this.m_leftTop.Texture))
            {
                screenSize = (Vector2I) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_leftTop.SizeGui, false) * textureScale);
                SetTargetRectangle(out rectangle, ref screenPos, ref screenSize, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                MyGuiManager.DrawSprite(this.m_leftTop.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_leftBottom.Texture))
            {
                vectori11 = (Vector2I) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_leftBottom.SizeGui, false) * textureScale);
                SetTargetRectangle(out rectangle, ref vectori2, ref vectori11, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
                MyGuiManager.DrawSprite(this.m_leftBottom.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_rightTop.Texture))
            {
                vectori8 = (Vector2I) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_rightTop.SizeGui, false) * textureScale);
                SetTargetRectangle(out rectangle, ref vectori3, ref vectori8, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                MyGuiManager.DrawSprite(this.m_rightTop.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_rightBottom.Texture))
            {
                vectori13 = (Vector2I) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_rightBottom.SizeGui, false) * textureScale);
                SetTargetRectangle(out rectangle, ref vectori4, ref vectori13, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
                MyGuiManager.DrawSprite(this.m_rightBottom.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_centerTop.Texture))
            {
                vectori7.X = vectori5.X - (screenSize.X + vectori8.X);
                vectori7.Y = (int) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_centerTop.SizeGui, false).Y * textureScale);
                Vector2I vectori14 = screenPos + new Vector2I(screenSize.X, 0);
                SetTargetRectangle(out rectangle, ref vectori14, ref vectori7, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                MyGuiManager.DrawSprite(this.m_centerTop.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_centerBottom.Texture))
            {
                vectori12.X = vectori5.X - (vectori11.X + vectori13.X);
                vectori12.Y = (int) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_centerBottom.SizeGui, false).Y * textureScale);
                Vector2I vectori15 = vectori2 + new Vector2I(vectori11.X, 0);
                SetTargetRectangle(out rectangle, ref vectori15, ref vectori12, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
                MyGuiManager.DrawSprite(this.m_centerBottom.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_leftCenter.Texture))
            {
                vectori9.X = (int) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_leftCenter.SizeGui, false).X * textureScale);
                vectori9.Y = vectori5.Y - (screenSize.Y + vectori11.Y);
                Vector2I vectori16 = screenPos + new Vector2I(0, screenSize.Y);
                SetTargetRectangle(out rectangle, ref vectori16, ref vectori9, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                MyGuiManager.DrawSprite(this.m_leftCenter.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_rightCenter.Texture))
            {
                vectori10.X = (int) (MyGuiManager.GetScreenSizeFromNormalizedSize(this.m_rightCenter.SizeGui, false).X * textureScale);
                vectori10.Y = vectori5.Y - (vectori8.Y + vectori13.Y);
                Vector2I vectori17 = vectori3 + new Vector2I(0, vectori8.Y);
                SetTargetRectangle(out rectangle, ref vectori17, ref vectori10, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                MyGuiManager.DrawSprite(this.m_rightCenter.Texture, rectangle, colorMask, true);
            }
            if (!string.IsNullOrEmpty(this.m_center.Texture))
            {
                int x = MathHelper.Max(screenSize.X, vectori9.X, vectori11.X);
                int y = MathHelper.Max(screenSize.Y, vectori7.Y, vectori8.Y);
                Vector2I vectori18 = vectori5 - new Vector2I(x + MathHelper.Max(vectori8.X, vectori10.X, vectori13.X), y + MathHelper.Max(vectori11.Y, vectori12.Y, vectori13.Y));
                Vector2I vectori19 = screenPos + new Vector2I(x, y);
                SetTargetRectangle(out rectangle, ref vectori19, ref vectori18, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                MyGuiManager.DrawSprite(this.m_center.Texture, rectangle, colorMask, true);
            }
        }

        public static implicit operator MyGuiCompositeTexture(SerializableCompositeTexture texture)
        {
            MyGuiCompositeTexture texture2 = new MyGuiCompositeTexture(texture.Center);
            float x = (texture.Size.X - texture.BorderSizes.Left) - texture.BorderSizes.Right;
            float y = (texture.Size.Y - texture.BorderSizes.Top) - texture.BorderSizes.Bottom;
            MyGuiSizedTexture texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(texture.BorderSizes.Left, texture.BorderSizes.Top),
                Texture = texture.LeftTop
            };
            MyGuiSizedTexture texture3 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(texture.BorderSizes.Left, y),
                Texture = texture.LeftCenter
            };
            MyGuiSizedTexture texture4 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(texture.BorderSizes.Left, texture.BorderSizes.Bottom),
                Texture = texture.LeftBottom
            };
            MyGuiSizedTexture texture5 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(x, texture.BorderSizes.Top),
                Texture = texture.CenterTop
            };
            MyGuiSizedTexture texture6 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(x, y),
                Texture = texture.Center
            };
            MyGuiSizedTexture texture7 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(x, texture.BorderSizes.Bottom),
                Texture = texture.CenterBottom
            };
            MyGuiSizedTexture texture8 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(texture.BorderSizes.Right, texture.BorderSizes.Top),
                Texture = texture.RightTop
            };
            MyGuiSizedTexture texture9 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(texture.BorderSizes.Right, y),
                Texture = texture.RightCenter
            };
            MyGuiSizedTexture texture10 = texture12;
            texture12 = new MyGuiSizedTexture {
                SizePx = new Vector2(texture.BorderSizes.Right, texture.BorderSizes.Bottom),
                Texture = texture.RightBottom
            };
            MyGuiSizedTexture texture11 = texture12;
            if (texture.LeftTop != null)
            {
                texture2.LeftTop = texture3;
            }
            if (texture.LeftCenter != null)
            {
                texture2.LeftCenter = texture4;
            }
            if (texture.LeftBottom != null)
            {
                texture2.LeftBottom = texture5;
            }
            if (texture.CenterTop != null)
            {
                texture2.CenterTop = texture6;
            }
            if (texture.Center != null)
            {
                texture2.Center = texture7;
            }
            if (texture.CenterBottom != null)
            {
                texture2.CenterBottom = texture8;
            }
            if (texture.RightTop != null)
            {
                texture2.RightTop = texture9;
            }
            if (texture.RightCenter != null)
            {
                texture2.RightCenter = texture10;
            }
            if (texture.RightBottom != null)
            {
                texture2.RightBottom = texture11;
            }
            return texture2;
        }

        public static implicit operator SerializableCompositeTexture(MyGuiCompositeTexture texture)
        {
            SerializableCompositeTexture texture2 = new SerializableCompositeTexture {
                BorderSizes = { 
                    Left = texture.LeftCenter.SizePx.X,
                    Right = texture.RightCenter.SizePx.X,
                    Top = texture.CenterTop.SizePx.Y,
                    Bottom = texture.CenterBottom.SizePx.Y
                }
            };
            texture2.Size = new Vector2((texture2.BorderSizes.Left + texture.Center.SizePx.X) + texture2.BorderSizes.Right, (texture2.BorderSizes.Top + texture.Center.SizePx.Y) + texture2.BorderSizes.Bottom);
            texture2.LeftTop = texture.LeftTop.Texture;
            texture2.LeftCenter = texture.LeftCenter.Texture;
            texture2.LeftBottom = texture.LeftBottom.Texture;
            texture2.CenterTop = texture.CenterTop.Texture;
            texture2.Center = texture.Center.Texture;
            texture2.CenterBottom = texture.CenterBottom.Texture;
            texture2.RightTop = texture.RightTop.Texture;
            texture2.RightCenter = texture.RightCenter.Texture;
            texture2.RightBottom = texture.RightBottom.Texture;
            return texture2;
        }

        private void RefreshSizeLimits()
        {
            this.m_minSizeGui.X = Math.Max((float) (this.m_leftTop.SizeGui.X + this.m_rightTop.SizeGui.X), (float) (this.m_leftBottom.SizeGui.X + this.m_rightBottom.SizeGui.X));
            this.m_minSizeGui.Y = Math.Max((float) (this.m_leftTop.SizeGui.Y + this.m_leftBottom.SizeGui.Y), (float) (this.m_rightTop.SizeGui.Y + this.m_rightBottom.SizeGui.Y));
            if (this.m_center.Texture != null)
            {
                this.m_maxSizeGui.X = float.PositiveInfinity;
                this.m_maxSizeGui.Y = float.PositiveInfinity;
            }
            else
            {
                float positiveInfinity;
                float positiveInfinity;
                if ((this.m_centerTop.Texture != null) || (this.m_centerBottom.Texture != null))
                {
                    positiveInfinity = float.PositiveInfinity;
                }
                else
                {
                    positiveInfinity = this.m_minSizeGui.X;
                }
                this.m_maxSizeGui.X = positiveInfinity;
                if ((this.m_leftCenter.Texture != null) || (this.m_rightCenter.Texture != null))
                {
                    positiveInfinity = float.PositiveInfinity;
                }
                else
                {
                    positiveInfinity = this.m_minSizeGui.Y;
                }
                this.m_maxSizeGui.Y = positiveInfinity;
                if (((this.m_leftTop.Texture == null) && ((this.m_centerTop.Texture == null) && ((this.m_rightTop.Texture == null) && ((this.m_leftCenter.Texture == null) && ((this.m_center.Texture == null) && ((this.m_rightCenter.Texture == null) && ((this.m_leftBottom.Texture == null) && (this.m_centerBottom.Texture == null)))))))) && (this.m_rightBottom.Texture == null))
                {
                    this.m_maxSizeGui = Vector2.PositiveInfinity;
                }
            }
            this.m_sizeLimitsDirty = false;
        }

        private static void SetTargetRectangle(out Rectangle target, ref Vector2I screenPos, ref Vector2I screenSize, MyGuiDrawAlignEnum posAlign)
        {
            Vector2I vectori = MyUtils.GetCoordTopLeftFromAligned(screenPos, screenSize, posAlign);
            target.X = vectori.X;
            target.Y = vectori.Y;
            target.Width = screenSize.X;
            target.Height = screenSize.Y;
        }

        public MyGuiSizedTexture LeftTop
        {
            get => 
                this.m_leftTop;
            set
            {
                this.m_leftTop = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture LeftCenter
        {
            get => 
                this.m_leftCenter;
            set
            {
                this.m_leftCenter = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture LeftBottom
        {
            get => 
                this.m_leftBottom;
            set
            {
                this.m_leftBottom = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture CenterTop
        {
            get => 
                this.m_centerTop;
            set
            {
                this.m_centerTop = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture Center
        {
            get => 
                this.m_center;
            set
            {
                this.m_center = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture CenterBottom
        {
            get => 
                this.m_centerBottom;
            set
            {
                this.m_centerBottom = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture RightTop
        {
            get => 
                this.m_rightTop;
            set
            {
                this.m_rightTop = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture RightCenter
        {
            get => 
                this.m_rightCenter;
            set
            {
                this.m_rightCenter = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public MyGuiSizedTexture RightBottom
        {
            get => 
                this.m_rightBottom;
            set
            {
                this.m_rightBottom = value;
                this.m_sizeLimitsDirty = true;
            }
        }

        public Vector2 MinSizeGui
        {
            get
            {
                if (this.m_sizeLimitsDirty)
                {
                    this.RefreshSizeLimits();
                }
                return this.m_minSizeGui;
            }
        }

        public Vector2 MaxSizeGui
        {
            get
            {
                if (this.m_sizeLimitsDirty)
                {
                    this.RefreshSizeLimits();
                }
                return this.m_maxSizeGui;
            }
        }
    }
}

