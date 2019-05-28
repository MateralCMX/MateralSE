namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Library.Utils;
    using VRageMath;
    using VRageRender;

    public class MyStatControlCircularProgressBar : MyStatControlBase
    {
        private static MyGameTimer TIMER = new MyGameTimer();
        private float m_progression;
        private Vector2 m_segmentOrigin;
        private Vector2 m_segmentSize;
        private Vector2 m_invScale;
        private int m_animatedSegmentIndex;
        private double m_animationTimeSwitchedSegment;
        private double m_animationTimeStarted;
        private bool m_animating;
        private readonly MyGuiSizedTexture m_backgroundTexture;
        private readonly MyGuiSizedTexture m_texture;
        private float m_textureRotationAngle;
        private float m_textureRotationOffset;

        public MyStatControlCircularProgressBar(MyStatControls parent, MyObjectBuilder_GuiTexture texture, MyObjectBuilder_GuiTexture backgroundTexture = null) : base(parent)
        {
            MyGuiSizedTexture texture2;
            if (backgroundTexture != null)
            {
                texture2 = new MyGuiSizedTexture {
                    Texture = backgroundTexture.Path,
                    SizePx = (Vector2) backgroundTexture.SizePx
                };
                this.m_backgroundTexture = texture2;
            }
            texture2 = new MyGuiSizedTexture {
                Texture = texture.Path,
                SizePx = (Vector2) texture.SizePx
            };
            this.m_texture = texture2;
            this.ShowEmptySegments = true;
            this.EmptySegmentColorMask = new Vector4(1f, 1f, 1f, 0.5f);
            this.FullSegmentColorMask = Vector4.One;
            this.AnimatedSegmentColorMask = new Vector4(1f, 1f, 1f, 0.8f);
            this.NumberOfSegments = 10;
            this.AnimationDelay = 2000.0;
            this.SegmentAnimationMs = 50.0;
            this.m_textureRotationAngle = 0.36f;
            this.m_segmentOrigin = new Vector2(this.m_texture.SizePx.X / 2f, this.m_texture.SizePx.Y / 2f);
        }

        public override void Draw(float transitionAlpha)
        {
            float num = base.StatCurrent / base.StatMaxValue;
            double milliseconds = 0.0;
            if (this.Animate)
            {
                milliseconds = TIMER.Elapsed.Milliseconds;
                if (!this.m_animating && ((milliseconds - this.m_animationTimeStarted) > this.AnimationDelay))
                {
                    this.m_animating = true;
                    this.m_animationTimeStarted = milliseconds;
                    this.m_animatedSegmentIndex = 0;
                    this.m_animationTimeSwitchedSegment = milliseconds;
                }
            }
            Rectangle? sourceRectangle = null;
            RectangleF destination = new RectangleF {
                Position = base.Position + new Vector2(-this.SegmentOrigin.X, this.SegmentOrigin.Y),
                Size = this.SegmentSize
            };
            float num3 = 1f / ((float) this.NumberOfSegments);
            for (int i = 0; i < this.NumberOfSegments; i++)
            {
                Vector2 rightVector = new Vector2((float) Math.Cos((double) ((this.m_textureRotationAngle * i) + this.m_textureRotationOffset)), (float) Math.Sin((double) ((this.m_textureRotationAngle * i) + this.m_textureRotationOffset)));
                Vector4 emptySegmentColorMask = this.EmptySegmentColorMask;
                sourceRectangle = null;
                destination.Position = base.Position + new Vector2(-this.SegmentOrigin.X, this.SegmentOrigin.Y);
                destination.Size = this.SegmentSize;
                Vector2 origin = base.Position + (base.Size / 2f);
                if (this.ShowEmptySegments)
                {
                    MyRenderProxy.DrawSprite(this.m_texture.Texture, ref destination, true, ref sourceRectangle, emptySegmentColorMask, 0f, rightVector, ref origin, SpriteEffects.None, 0f, true, null);
                }
                bool flag = true;
                if (this.m_animating && (this.m_animatedSegmentIndex == i))
                {
                    emptySegmentColorMask = this.AnimatedSegmentColorMask;
                    if ((milliseconds - this.m_animationTimeSwitchedSegment) > this.SegmentAnimationMs)
                    {
                        this.m_animationTimeSwitchedSegment = milliseconds;
                        this.m_animatedSegmentIndex++;
                    }
                    flag = false;
                }
                else if (i < (num * this.NumberOfSegments))
                {
                    if ((num / ((i + 1) * num3)) < 1f)
                    {
                        float y = (num % num3) * this.NumberOfSegments;
                        float num6 = 1f - y;
                        sourceRectangle = new Rectangle(0, (int) (num6 * this.m_texture.SizePx.Y), (int) this.m_texture.SizePx.X, (int) (y * this.m_texture.SizePx.Y));
                        destination.Size = this.SegmentSize * new Vector2(1f, y);
                        destination.Position = base.Position + new Vector2(-this.SegmentOrigin.X, this.SegmentOrigin.Y + (this.SegmentSize.Y * num6));
                    }
                    emptySegmentColorMask = this.FullSegmentColorMask;
                    flag = false;
                }
                if (this.m_animatedSegmentIndex >= this.NumberOfSegments)
                {
                    this.m_animating = false;
                }
                if (!flag)
                {
                    MyRenderProxy.DrawSprite(this.m_texture.Texture, ref destination, true, ref sourceRectangle, emptySegmentColorMask, 0f, rightVector, ref origin, SpriteEffects.None, 0f, true, null);
                }
            }
            if (!string.IsNullOrEmpty(this.m_backgroundTexture.Texture))
            {
                destination = new RectangleF(base.Position - (base.Size / 2f), base.Size);
                MyRenderProxy.DrawSprite(this.m_texture.Texture, ref destination, true, ref sourceRectangle, Color.White, 0f, Vector2.UnitX, ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
            }
        }

        private void RecalcScale()
        {
            this.m_invScale = (Vector2) (1f / (this.m_segmentSize / this.m_texture.SizePx));
        }

        public int NumberOfSegments { get; set; }

        public bool Animate { get; set; }

        public double SegmentAnimationMs { get; set; }

        public double AnimationDelay { get; set; }

        public bool ShowEmptySegments { get; set; }

        public Vector4 EmptySegmentColorMask { get; set; }

        public Vector4 FullSegmentColorMask { get; set; }

        public Vector4 AnimatedSegmentColorMask { get; set; }

        public float TextureRotationAngle
        {
            get => 
                MathHelper.ToDegrees(this.m_textureRotationAngle);
            set => 
                (this.m_textureRotationAngle = MathHelper.ToRadians(value));
        }

        public float TextureRotationOffset
        {
            get => 
                MathHelper.ToDegrees(this.m_textureRotationOffset);
            set => 
                (this.m_textureRotationOffset = MathHelper.ToRadians(value));
        }

        public Vector2 SegmentSize
        {
            get => 
                this.m_segmentSize;
            set
            {
                this.m_segmentSize = value;
                this.RecalcScale();
            }
        }

        public Vector2 SegmentOrigin
        {
            get => 
                this.m_segmentOrigin;
            set
            {
                this.m_segmentOrigin = value;
                this.RecalcScale();
            }
        }
    }
}

