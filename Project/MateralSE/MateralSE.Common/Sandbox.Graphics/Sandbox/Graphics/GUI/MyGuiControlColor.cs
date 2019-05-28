namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlColor : MyGuiControlBase
    {
        [CompilerGenerated]
        private Action<MyGuiControlColor> OnChange;
        private const float SLIDER_WIDTH = 0.09f;
        private VRageMath.Color m_color;
        private MyGuiControlLabel m_textLabel;
        private MyGuiControlSlider m_RSlider;
        private MyGuiControlSlider m_GSlider;
        private MyGuiControlSlider m_BSlider;
        private MyGuiControlLabel m_RLabel;
        private MyGuiControlLabel m_GLabel;
        private MyGuiControlLabel m_BLabel;
        private Vector2 m_minSize;
        private MyStringId m_caption;
        private bool m_canChangeColor;
        private bool m_placeSlidersVertically;

        public event Action<MyGuiControlColor> OnChange
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlColor> onChange = this.OnChange;
                while (true)
                {
                    Action<MyGuiControlColor> a = onChange;
                    Action<MyGuiControlColor> action3 = (Action<MyGuiControlColor>) Delegate.Combine(a, value);
                    onChange = Interlocked.CompareExchange<Action<MyGuiControlColor>>(ref this.OnChange, action3, a);
                    if (ReferenceEquals(onChange, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlColor> onChange = this.OnChange;
                while (true)
                {
                    Action<MyGuiControlColor> source = onChange;
                    Action<MyGuiControlColor> action3 = (Action<MyGuiControlColor>) Delegate.Remove(source, value);
                    onChange = Interlocked.CompareExchange<Action<MyGuiControlColor>>(ref this.OnChange, action3, source);
                    if (ReferenceEquals(onChange, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlColor(string text, float textScale, Vector2 position, VRageMath.Color color, VRageMath.Color defaultColor, MyStringId dialogAmountCaption, bool placeSlidersVertically = false, string font = "Blue") : base(new Vector2?(position), nullable, nullable2, null, null, false, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_canChangeColor = true;
            this.m_color = color;
            this.m_placeSlidersVertically = placeSlidersVertically;
            this.m_textLabel = this.MakeLabel(textScale, font);
            this.m_textLabel.Text = text.ToString();
            this.m_caption = dialogAmountCaption;
            this.m_RSlider = this.MakeSlider(font, defaultColor.R);
            this.m_GSlider = this.MakeSlider(font, defaultColor.G);
            this.m_BSlider = this.MakeSlider(font, defaultColor.B);
            this.m_RSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_RSlider.ValueChanged, delegate (MyGuiControlSlider sender) {
                if (this.m_canChangeColor)
                {
                    this.m_color.R = (byte) sender.Value;
                    this.UpdateTexts();
                    if (this.OnChange != null)
                    {
                        this.OnChange(this);
                    }
                }
            });
            this.m_GSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_GSlider.ValueChanged, delegate (MyGuiControlSlider sender) {
                if (this.m_canChangeColor)
                {
                    this.m_color.G = (byte) sender.Value;
                    this.UpdateTexts();
                    if (this.OnChange != null)
                    {
                        this.OnChange(this);
                    }
                }
            });
            this.m_BSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_BSlider.ValueChanged, delegate (MyGuiControlSlider sender) {
                if (this.m_canChangeColor)
                {
                    this.m_color.B = (byte) sender.Value;
                    this.UpdateTexts();
                    if (this.OnChange != null)
                    {
                        this.OnChange(this);
                    }
                }
            });
            this.m_RLabel = this.MakeLabel(textScale, font);
            this.m_GLabel = this.MakeLabel(textScale, font);
            this.m_BLabel = this.MakeLabel(textScale, font);
            this.m_RSlider.Value = this.m_color.R;
            this.m_GSlider.Value = this.m_color.G;
            this.m_BSlider.Value = this.m_color.B;
            base.Elements.Add(this.m_textLabel);
            base.Elements.Add(this.m_RSlider);
            base.Elements.Add(this.m_GSlider);
            base.Elements.Add(this.m_BSlider);
            base.Elements.Add(this.m_RLabel);
            base.Elements.Add(this.m_GLabel);
            base.Elements.Add(this.m_BLabel);
            this.UpdateTexts();
            this.RefreshInternals();
            base.Size = this.m_minSize;
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            Vector2 positionAbsoluteTopRight = base.GetPositionAbsoluteTopRight();
            Vector2 normalizedSize = new Vector2(this.m_BSlider.Size.X, this.m_textLabel.Size.Y);
            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", positionAbsoluteTopRight, normalizedSize, ApplyColorMaskModifiers(this.m_color.ToVector4(), true, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false, true);
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            float* singlePtr1 = (float*) ref positionAbsoluteTopRight.X;
            singlePtr1[0] -= normalizedSize.X;
            VRageMath.Color white = VRageMath.Color.White;
            VRageMath.Color* colorPtr1 = (VRageMath.Color*) ref white;
            colorPtr1.A = (byte) (white.A * transitionAlpha);
            MyGuiManager.DrawBorders(positionAbsoluteTopRight, normalizedSize, white, base.BorderSize);
        }

        public VRageMath.Color GetColor() => 
            this.m_color;

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 == null)
            {
                base2 = base.HandleInputElements();
            }
            return base2;
        }

        private MyGuiControlLabel MakeLabel(float scale, string font)
        {
            Vector2? position = null;
            position = null;
            return new MyGuiControlLabel(position, position, string.Empty, new Vector4?(base.ColorMask), 0.8f * scale, font, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
        }

        private MyGuiControlSlider MakeSlider(string font, byte defaultVal)
        {
            Vector4? color = new Vector4?(base.ColorMask);
            string labelFont = font;
            MyGuiControlSlider slider1 = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 0f, 255f, 121f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, new float?((float) defaultVal), color, null, 1, 0.8f, 0f, labelFont, null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
            slider1.SliderClicked = new Func<MyGuiControlSlider, bool>(this.OnSliderClicked);
            return slider1;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshInternals();
        }

        private bool OnSliderClicked(MyGuiControlSlider who)
        {
            if (!MyInput.Static.IsAnyCtrlKeyPressed())
            {
                return false;
            }
            float num2 = who.Value;
            MyGuiScreenDialogAmount screen = new MyGuiScreenDialogAmount(0f, 255f, this.m_caption, 3, true, new float?(num2), 0f, 0f);
            screen.OnConfirmed += delegate (float v) {
                who.Value = v;
            };
            MyScreenManager.AddScreen(screen);
            return true;
        }

        private unsafe void RefreshInternals()
        {
            Vector2 vector = (Vector2) (-0.5f * base.Size);
            Vector2 zero = Vector2.Zero;
            if (this.m_placeSlidersVertically)
            {
                zero.X = Math.Max(this.m_textLabel.Size.X, this.m_GSlider.MinSize.X + 0.06f);
                zero.Y = (this.m_textLabel.Size.Y * 1.1f) + (3f * Math.Max(this.m_GSlider.Size.Y, this.m_GLabel.Size.Y));
            }
            else
            {
                zero.X = MathHelper.Max(this.m_textLabel.Size.X, 3f * (this.m_GSlider.MinSize.X + 0.06f));
                zero.Y = ((this.m_textLabel.Size.Y * 1.1f) + this.m_RSlider.Size.Y) + this.m_RLabel.Size.Y;
            }
            if ((base.Size.X < zero.X) || (base.Size.Y < zero.Y))
            {
                base.Size = Vector2.Max(base.Size, zero);
            }
            else
            {
                Vector2 vector5;
                this.m_textLabel.Position = vector;
                float* singlePtr1 = (float*) ref vector.Y;
                singlePtr1[0] += this.m_textLabel.Size.Y * 1.1f;
                if (this.m_placeSlidersVertically)
                {
                    Vector2 vector3 = new Vector2(base.Size.X - 0.06f, this.m_RSlider.MinSize.Y);
                    float num = Math.Max(this.m_RLabel.Size.Y, this.m_RSlider.Size.Y);
                    this.m_BSlider.Size = vector5 = vector3;
                    this.m_RSlider.Size = this.m_GSlider.Size = vector5;
                    this.m_RLabel.Position = vector + (new Vector2(0f, 0.5f) * num);
                    this.m_RSlider.Position = new Vector2(vector.X + base.Size.X, vector.Y + (0.5f * num));
                    this.m_RLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                    this.m_RSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
                    float* singlePtr2 = (float*) ref vector.Y;
                    singlePtr2[0] += num;
                    this.m_GLabel.Position = vector + (new Vector2(0f, 0.5f) * num);
                    this.m_GSlider.Position = new Vector2(vector.X + base.Size.X, vector.Y + (0.5f * num));
                    this.m_GLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                    this.m_GSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
                    float* singlePtr3 = (float*) ref vector.Y;
                    singlePtr3[0] += num;
                    this.m_BLabel.Position = vector + (new Vector2(0f, 0.5f) * num);
                    this.m_BSlider.Position = new Vector2(vector.X + base.Size.X, vector.Y + (0.5f * num));
                    this.m_BLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                    this.m_BSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
                    float* singlePtr4 = (float*) ref vector.Y;
                    singlePtr4[0] += num;
                }
                else
                {
                    MyGuiDrawAlignEnum enum3;
                    float x = MathHelper.Max(this.m_RLabel.Size.X, this.m_RSlider.MinSize.X, base.Size.X / 3f);
                    Vector2 vector6 = new Vector2(x, this.m_RSlider.Size.Y);
                    this.m_BSlider.Size = vector5 = vector6;
                    this.m_RSlider.Size = this.m_GSlider.Size = vector5;
                    Vector2 vector7 = vector;
                    this.m_RSlider.Position = vector7;
                    float* singlePtr5 = (float*) ref vector7.X;
                    singlePtr5[0] += x;
                    this.m_GSlider.Position = vector7;
                    float* singlePtr6 = (float*) ref vector7.X;
                    singlePtr6[0] += x;
                    this.m_BSlider.Position = vector7;
                    float* singlePtr7 = (float*) ref vector.Y;
                    singlePtr7[0] += this.m_RSlider.Size.Y;
                    this.m_RLabel.Position = vector;
                    float* singlePtr8 = (float*) ref vector.X;
                    singlePtr8[0] += x;
                    this.m_GLabel.Position = vector;
                    float* singlePtr9 = (float*) ref vector.X;
                    singlePtr9[0] += x;
                    this.m_BLabel.Position = vector;
                    float* singlePtr10 = (float*) ref vector.X;
                    singlePtr10[0] += x;
                    this.m_BLabel.OriginAlign = enum3 = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    this.m_RLabel.OriginAlign = this.m_GLabel.OriginAlign = enum3;
                    this.m_BSlider.OriginAlign = enum3 = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    this.m_RSlider.OriginAlign = this.m_GSlider.OriginAlign = enum3;
                }
            }
        }

        public void SetColor(VRageMath.Color color)
        {
            this.m_color = color;
            this.UpdateSliders();
            if ((this.m_color != color) && (this.OnChange != null))
            {
                this.OnChange(this);
            }
        }

        public void SetColor(Vector3 color)
        {
            this.SetColor(new VRageMath.Color(color));
        }

        public void SetColor(Vector4 color)
        {
            this.SetColor(new VRageMath.Color(color));
        }

        private void UpdateSliders()
        {
            this.m_canChangeColor = false;
            this.m_RSlider.Value = this.m_color.R;
            this.m_GSlider.Value = this.m_color.G;
            this.m_BSlider.Value = this.m_color.B;
            this.UpdateTexts();
            this.m_canChangeColor = true;
        }

        private void UpdateTexts()
        {
            this.m_RLabel.Text = $"R: {this.m_color.R}";
            this.m_GLabel.Text = $"G: {this.m_color.G}";
            this.m_BLabel.Text = $"B: {this.m_color.B}";
        }

        public VRageMath.Color Color =>
            this.m_color;
    }
}

