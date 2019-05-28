namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyGuiProgressCompositeTexture
    {
        protected readonly TextureData[,] m_textures = new TextureData[3, 3];
        protected bool m_positionsAndSizesDirty = true;
        protected Vector2I m_position = Vector2I.Zero;
        protected Vector2I m_size = Vector2I.Zero;

        public virtual unsafe void Draw(float progression, Color colorMask)
        {
            if (this.m_positionsAndSizesDirty)
            {
                this.RefreshPositionsAndSizes();
            }
            float single1 = MyMath.Clamp(progression, 0f, 1f);
            progression = single1;
            Rectangle rect = new Rectangle();
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
                    TextureData texData = this.m_textures[num, num2];
                    if (!string.IsNullOrEmpty(texData.Texture.Texture))
                    {
                        if ((num != 1) || (num2 != 1))
                        {
                            this.SetTarget(ref rect, texData);
                            MyGuiManager.DrawSprite(texData.Texture.Texture, rect, colorMask, true);
                        }
                        else
                        {
                            int x;
                            int y;
                            int num5;
                            int num6;
                            if (this.Orientation != BarOrientation.HORIZONTAL)
                            {
                                y = this.m_textures[1, 1].Size.Y;
                                num6 = ((int) (this.m_textures[1, 0].Size.Y * progression)) + 1;
                            }
                            else
                            {
                                x = this.m_textures[1, 1].Size.X;
                                y = x;
                                num5 = this.m_textures[0, 1].Size.X;
                                num6 = ((int) (num5 * progression)) + 1;
                            }
                            this.SetTarget(ref rect, texData);
                            if (this.IsInverted)
                            {
                                if (this.Orientation == BarOrientation.HORIZONTAL)
                                {
                                    int* numPtr1 = (int*) ref rect.X;
                                    numPtr1[0] += num5 - x;
                                }
                                else
                                {
                                    int* numPtr2 = (int*) ref rect.Y;
                                    numPtr2[0] += num5 - x;
                                }
                            }
                            while (true)
                            {
                                if (y >= num6)
                                {
                                    int num7 = y - num6;
                                    int num8 = x - num7;
                                    if (num7 > 1)
                                    {
                                        if (this.Orientation == BarOrientation.HORIZONTAL)
                                        {
                                            rect.Width = num8;
                                            if (this.IsInverted)
                                            {
                                                int* numPtr3 = (int*) ref rect.X;
                                                numPtr3[0] += x;
                                            }
                                        }
                                        else
                                        {
                                            rect.Height = num8;
                                            if (this.IsInverted)
                                            {
                                                int* numPtr4 = (int*) ref rect.Y;
                                                numPtr4[0] += x;
                                            }
                                        }
                                        MyGuiManager.DrawSprite(texData.Texture.Texture, rect, colorMask, true);
                                    }
                                    break;
                                }
                                MyGuiManager.DrawSprite(texData.Texture.Texture, rect, colorMask, true);
                                if (this.Orientation == BarOrientation.HORIZONTAL)
                                {
                                    rect.X = !this.IsInverted ? (texData.Position.X + y) : ((texData.Position.X + num5) - y);
                                }
                                else
                                {
                                    rect.Y = !this.IsInverted ? (texData.Position.Y + y) : ((texData.Position.Y + num5) - y);
                                }
                                y += x;
                            }
                        }
                    }
                    num2++;
                }
            }
        }

        protected virtual void RefreshPositionsAndSizes()
        {
            this.m_textures[0, 0].Position = this.m_position;
            Vector2I vectori = (this.m_size - this.m_textures[0, 0].Size) - this.m_textures[2, 2].Size;
            this.m_textures[1, 0].Size.Y = vectori.Y;
            this.m_textures[1, 2].Size.Y = vectori.Y;
            this.m_textures[0, 1].Size.X = vectori.X;
            this.m_textures[2, 1].Size.X = vectori.X;
            this.m_textures[1, 1].Size.Y = vectori.Y;
            Vector2I size = this.m_textures[1, 1].Size;
            this.m_textures[1, 1].Size = vectori;
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
                    if ((num != 0) || (num2 != 0))
                    {
                        int x = (num2 > 0) ? (this.m_textures[num, num2 - 1].Position.X + this.m_textures[num, num2 - 1].Size.X) : this.m_textures[0, 0].Position.X;
                        this.m_textures[num, num2].Position = new Vector2I(x, (num > 0) ? (this.m_textures[num - 1, num2].Position.Y + this.m_textures[num - 1, num2].Size.Y) : this.m_textures[0, 0].Position.Y);
                    }
                    num2++;
                }
            }
            this.m_textures[1, 1].Size = size;
            this.m_positionsAndSizesDirty = false;
        }

        protected void SetTarget(ref Rectangle rect, TextureData texData)
        {
            rect.X = texData.Position.X;
            rect.Y = texData.Position.Y;
            rect.Width = texData.Size.X;
            rect.Height = texData.Size.Y;
        }

        private Vector2I ToVector2I(Vector2 source) => 
            new Vector2I((int) source.X, (int) source.Y);

        public bool IsInverted { get; set; }

        public BarOrientation Orientation { get; set; }

        public MyGuiSizedTexture LeftTop
        {
            get => 
                this.m_textures[0, 0].Texture;
            set
            {
                this.m_textures[0, 0].Texture = value;
                this.m_textures[0, 0].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture LeftCenter
        {
            get => 
                this.m_textures[1, 0].Texture;
            set
            {
                this.m_textures[1, 0].Texture = value;
                this.m_textures[1, 0].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture LeftBottom
        {
            get => 
                this.m_textures[2, 0].Texture;
            set
            {
                this.m_textures[2, 0].Texture = value;
                this.m_textures[2, 0].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture CenterTop
        {
            get => 
                this.m_textures[0, 1].Texture;
            set
            {
                this.m_textures[0, 1].Texture = value;
                this.m_textures[0, 1].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture Center
        {
            get => 
                this.m_textures[1, 1].Texture;
            set
            {
                this.m_textures[1, 1].Texture = value;
                this.m_textures[1, 1].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture CenterBottom
        {
            get => 
                this.m_textures[2, 1].Texture;
            set
            {
                this.m_textures[2, 1].Texture = value;
                this.m_textures[2, 1].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture RightTop
        {
            get => 
                this.m_textures[0, 2].Texture;
            set
            {
                this.m_textures[0, 2].Texture = value;
                this.m_textures[0, 2].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture RightCenter
        {
            get => 
                this.m_textures[1, 2].Texture;
            set
            {
                this.m_textures[1, 2].Texture = value;
                this.m_textures[1, 2].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public MyGuiSizedTexture RightBottom
        {
            get => 
                this.m_textures[2, 2].Texture;
            set
            {
                this.m_textures[2, 2].Texture = value;
                this.m_textures[2, 2].Size = this.ToVector2I(value.SizePx);
                this.m_positionsAndSizesDirty = true;
            }
        }

        public Vector2I Position
        {
            get => 
                this.m_position;
            set
            {
                this.m_position = value;
                this.m_positionsAndSizesDirty = true;
            }
        }

        public Vector2I Size
        {
            get => 
                this.m_size;
            set
            {
                this.m_size = value;
                this.m_positionsAndSizesDirty = true;
            }
        }

        public enum BarOrientation
        {
            HORIZONTAL,
            VERTICAL
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct TextureData
        {
            public Vector2I Position;
            public Vector2I Size;
            public MyGuiSizedTexture Texture;
            public override string ToString()
            {
                object[] objArray1 = new object[] { "Position: ", this.Position, " Size: ", this.Size };
                return string.Concat(objArray1);
            }
        }
    }
}

