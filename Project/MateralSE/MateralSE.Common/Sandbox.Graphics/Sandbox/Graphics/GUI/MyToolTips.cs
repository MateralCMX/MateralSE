namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Specialized;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Utils;
    using VRageMath;

    public class MyToolTips
    {
        private MyGuiControlBase m_tooltipControl;
        public readonly ObservableCollection<MyColoredText> ToolTips;
        public bool RecalculateOnChange;
        public Vector2 Size;
        public string Background;
        public Color? ColorMask;

        public MyToolTips()
        {
            this.RecalculateOnChange = true;
            this.Background = null;
            this.ColorMask = null;
            this.ToolTips = new ObservableCollection<MyColoredText>();
            this.ToolTips.CollectionChanged += new NotifyCollectionChangedEventHandler(this.ToolTips_CollectionChanged);
            this.Size = new Vector2(-1f);
            this.HighlightColor = Color.Orange.ToVector4();
        }

        public MyToolTips(string toolTip) : this()
        {
            this.AddToolTip(toolTip, 0.7f, "Blue");
        }

        public void AddToolTip(string toolTip, float textScale = 0.7f, string font = "Blue")
        {
            if (toolTip != null)
            {
                Color? highlightColor = null;
                Vector2? offset = null;
                this.ToolTips.Add(new MyColoredText(toolTip, new Color?(Color.White), highlightColor, font, textScale, offset));
            }
        }

        public unsafe void Draw(Vector2 mousePosition)
        {
            Vector2 vector = mousePosition + MyGuiConstants.TOOL_TIP_RELATIVE_DEFAULT_POSITION;
            if (this.Size.X > -1f)
            {
                Vector2 vector2 = new Vector2(0.005f, 0.002f);
                Vector2 normalizedSize = this.Size + (2f * vector2);
                Vector2 normalizedCoord = vector - new Vector2(vector2.X, 0f);
                Rectangle rectangle = MyGuiManager.FullscreenHudEnabled ? MyGuiManager.GetFullscreenRectangle() : MyGuiManager.GetSafeFullscreenRectangle();
                Vector2 vector5 = MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate(new Vector2((float) rectangle.Left, (float) rectangle.Top)) + new Vector2(MyGuiConstants.TOOLTIP_DISTANCE_FROM_BORDER);
                Vector2 vector6 = MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate(new Vector2((float) rectangle.Right, (float) rectangle.Bottom)) - new Vector2(MyGuiConstants.TOOLTIP_DISTANCE_FROM_BORDER);
                if ((normalizedCoord.X + normalizedSize.X) > vector6.X)
                {
                    normalizedCoord.X = vector6.X - normalizedSize.X;
                }
                if ((normalizedCoord.Y + normalizedSize.Y) > vector6.Y)
                {
                    normalizedCoord.Y = vector6.Y - normalizedSize.Y;
                }
                if (normalizedCoord.X < vector5.X)
                {
                    normalizedCoord.X = vector5.X;
                }
                if (normalizedCoord.Y < vector5.Y)
                {
                    normalizedCoord.Y = vector5.Y;
                }
                if (this.Highlight)
                {
                    Vector2 vector8 = new Vector2(0.003f, 0.004f);
                    Vector2 positionLeftTop = normalizedCoord - vector8;
                    MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL.Draw(positionLeftTop, normalizedSize + (2f * vector8), this.HighlightColor, 1f);
                }
                if (this.TooltipControl != null)
                {
                    this.TooltipControl.Position = normalizedCoord;
                    this.TooltipControl.Update();
                    this.TooltipControl.Draw(1f, 1f);
                }
                else
                {
                    Color? colorMask = this.ColorMask;
                    Color color = (colorMask != null) ? colorMask.GetValueOrDefault() : MyGuiConstants.THEMED_GUI_BACKGROUND_COLOR;
                    color.A = 230;
                    MyGuiManager.DrawSpriteBatch(@"Textures\GUI\TooltipBackground.dds", normalizedCoord, normalizedSize, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                    Vector2 normalizedPosition = normalizedCoord + new Vector2(vector2.X, (normalizedSize.Y / 2f) - (this.Size.Y / 2f));
                    foreach (MyColoredText text in this.ToolTips)
                    {
                        text.Draw(normalizedPosition, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, 1f, false, 1f);
                        float* singlePtr1 = (float*) ref normalizedPosition.Y;
                        singlePtr1[0] += text.Size.Y;
                    }
                }
            }
        }

        public void RecalculateSize()
        {
            float num = 0f;
            float num2 = 4f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            bool flag = true;
            for (int i = 0; i < this.ToolTips.Count; i++)
            {
                if (this.ToolTips[i].Text.Length > 0)
                {
                    flag = false;
                }
                Vector2 vector = MyGuiManager.MeasureString("Blue", this.ToolTips[i].Text, this.ToolTips[i].ScaleWithLanguage);
                num = Math.Max(this.Size.X, vector.X);
                num2 += vector.Y;
            }
            if (flag)
            {
                this.Size.X = -1f;
                this.Size.Y = -1f;
            }
            else
            {
                this.Size.X = num;
                this.Size.Y = num2;
            }
        }

        private void ToolTips_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.RecalculateOnChange)
            {
                this.RecalculateSize();
            }
        }

        public bool Highlight { get; set; }

        public Vector4 HighlightColor { get; set; }

        public MyGuiControlBase TooltipControl
        {
            get => 
                this.m_tooltipControl;
            set
            {
                this.m_tooltipControl = value;
                if (this.m_tooltipControl != null)
                {
                    this.Size = this.m_tooltipControl.Size;
                }
                else
                {
                    this.RecalculateSize();
                }
            }
        }

        public bool HasContent =>
            ((this.m_tooltipControl != null) || (this.ToolTips.Count > 0));
    }
}

