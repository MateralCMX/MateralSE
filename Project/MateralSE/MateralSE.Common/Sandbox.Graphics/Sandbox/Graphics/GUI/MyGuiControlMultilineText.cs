namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlMultilineLabel))]
    public class MyGuiControlMultilineText : MyGuiControlBase
    {
        protected MyGuiBorderThickness m_textPadding;
        private float m_textScale;
        private float m_textScaleWithLanguage;
        private static readonly StringBuilder m_letterA = new StringBuilder("A");
        private static readonly StringBuilder m_lineHeightMeasure = new StringBuilder("Ajqypdbfgjl");
        protected readonly StringBuilder m_tmpOffsetMeasure;
        protected readonly MyVScrollbar m_scrollbarV;
        protected readonly MyHScrollbar m_scrollbarH;
        private Vector2 m_scrollbarSizeV;
        private Vector2 m_scrollbarSizeH;
        protected MyRichLabel m_label;
        private bool m_drawScrollbarV;
        private bool m_drawScrollbarH;
        private float m_scrollbarOffsetV;
        private float m_scrollbarOffsetH;
        private bool m_showTextShadow;
        private bool m_selectable;
        protected MyKeyThrottler m_keyThrottler;
        protected int m_carriageBlinkerTimer;
        protected int m_carriagePositionIndex;
        protected MyGuiControlMultilineSelection m_selection;
        protected StringBuilder m_text;
        private MyStringId m_textEnum;
        private bool m_useEnum;
        private bool m_isImeActive;
        [CompilerGenerated]
        private LinkClicked OnLinkClicked;
        private string m_font;

        public event LinkClicked OnLinkClicked
        {
            [CompilerGenerated] add
            {
                LinkClicked onLinkClicked = this.OnLinkClicked;
                while (true)
                {
                    LinkClicked a = onLinkClicked;
                    LinkClicked clicked3 = (LinkClicked) Delegate.Combine(a, value);
                    onLinkClicked = Interlocked.CompareExchange<LinkClicked>(ref this.OnLinkClicked, clicked3, a);
                    if (ReferenceEquals(onLinkClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                LinkClicked onLinkClicked = this.OnLinkClicked;
                while (true)
                {
                    LinkClicked source = onLinkClicked;
                    LinkClicked clicked3 = (LinkClicked) Delegate.Remove(source, value);
                    onLinkClicked = Interlocked.CompareExchange<LinkClicked>(ref this.OnLinkClicked, clicked3, source);
                    if (ReferenceEquals(onLinkClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlMultilineText() : this(nullable, nullable, nullable2, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, nullable3, false, false, null, nullable4)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlMultilineText(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), string font = "Blue", float textScale = 0.8f, MyGuiDrawAlignEnum textAlign = 0, StringBuilder contents = null, bool drawScrollbarV = true, bool drawScrollbarH = true, MyGuiDrawAlignEnum textBoxAlign = 4, int? visibleLinesCount = new int?(), bool selectable = false, bool showTextShadow = false, MyGuiCompositeTexture backgroundTexture = null, MyGuiBorderThickness? textPadding = new MyGuiBorderThickness?()) : base(position, size, backgroundColor, null, backgroundTexture, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_tmpOffsetMeasure = new StringBuilder();
            this.m_useEnum = true;
            this.Font = font;
            this.TextScale = textScale;
            this.m_drawScrollbarV = drawScrollbarV;
            this.m_drawScrollbarH = drawScrollbarH;
            this.TextColor = new Color(Vector4.One);
            this.TextBoxAlign = textBoxAlign;
            this.m_selectable = selectable;
            MyGuiBorderThickness? nullable = textPadding;
            this.m_textPadding = (nullable != null) ? nullable.GetValueOrDefault() : new MyGuiBorderThickness(0f, 0f, 0f, 0f);
            this.m_scrollbarV = new MyVScrollbar(this);
            this.m_scrollbarSizeV = new Vector2(0.0334f, MyGuiConstants.COMBOBOX_VSCROLLBAR_SIZE.Y);
            this.m_scrollbarSizeV = MyGuiConstants.COMBOBOX_VSCROLLBAR_SIZE;
            this.m_scrollbarH = new MyHScrollbar(this);
            this.m_scrollbarSizeH = new Vector2(MyGuiConstants.COMBOBOX_HSCROLLBAR_SIZE.X, 0.0334f);
            this.m_scrollbarSizeH = MyGuiConstants.COMBOBOX_HSCROLLBAR_SIZE;
            float y = MyGuiManager.MeasureString(this.Font, m_lineHeightMeasure, this.TextScaleWithLanguage).Y;
            MyRichLabel label1 = new MyRichLabel(this, this.ComputeRichLabelWidth(), y, visibleLinesCount);
            label1.ShowTextShadow = showTextShadow;
            this.m_label = label1;
            this.m_label.AdjustingScissorRectangle += new ScissorRectangleHandler(this.AdjustScissorRectangleLabel);
            this.m_label.TextAlign = textAlign;
            this.m_label.CharactersDisplayed = -1;
            this.m_text = new StringBuilder();
            this.m_selection = new MyGuiControlMultilineSelection();
            if ((contents != null) && (contents.Length > 0))
            {
                this.Text = contents;
            }
            this.m_keyThrottler = new MyKeyThrottler();
        }

        private void AdjustScissorRectangle(ref RectangleF rectangle)
        {
        }

        private unsafe void AdjustScissorRectangle(ref RectangleF rectangle, float multWidth, float multHeight)
        {
            float width = rectangle.Width;
            float height = rectangle.Height;
            rectangle.Width *= multWidth;
            rectangle.Height *= multHeight;
            float num3 = rectangle.Width - width;
            float num4 = rectangle.Height - height;
            float* singlePtr1 = (float*) ref rectangle.Position.X;
            singlePtr1[0] -= num3 / 2f;
            float* singlePtr2 = (float*) ref rectangle.Position.Y;
            singlePtr2[0] -= num4 / 2f;
        }

        private void AdjustScissorRectangleLabel(ref RectangleF rectangle)
        {
        }

        public void AppendImage(string texture, Vector2 size, Vector4 color)
        {
            this.m_label.Append(texture, size, color);
            this.m_useEnum = false;
            this.RecalculateScrollBar();
        }

        public void AppendLine()
        {
            this.m_label.AppendLine();
            this.RecalculateScrollBar();
        }

        public void AppendLink(string url, string text)
        {
            this.m_label.AppendLink(url, text, this.TextScaleWithLanguage, new Action<string>(this.OnLinkClickedInternal));
            this.m_useEnum = false;
            this.RecalculateScrollBar();
        }

        public void AppendText(string text)
        {
            this.AppendText(text, this.Font, this.TextScaleWithLanguage, this.TextColor.ToVector4());
        }

        public void AppendText(StringBuilder text)
        {
            this.AppendText(text, this.Font, this.TextScaleWithLanguage, this.TextColor.ToVector4());
        }

        public void AppendText(string text, string font, float scale, Vector4 color)
        {
            this.m_label.Append(text, font, scale, color);
            this.m_useEnum = false;
            this.RecalculateScrollBar();
        }

        public void AppendText(StringBuilder text, string font, float scale, Vector4 color)
        {
            this.m_label.Append(text, font, scale, color);
            this.RecalculateScrollBar();
        }

        private bool CarriageVisible()
        {
            Vector2 carriageOffset = this.GetCarriageOffset(this.CarriagePositionIndex);
            float carriageHeight = this.GetCarriageHeight();
            if ((carriageOffset.Y < 0f) || (carriageOffset.X < 0f))
            {
                return false;
            }
            Vector2 textSizeWithScrolling = this.TextSizeWithScrolling;
            return ((carriageOffset.X <= textSizeWithScrolling.X) && ((carriageOffset.Y + carriageHeight) <= textSizeWithScrolling.Y));
        }

        public void Clear()
        {
            this.m_label.Clear();
            this.m_scrollbarV.SetPage(0f);
            this.m_scrollbarH.SetPage(0f);
            this.RecalculateScrollBar();
        }

        protected virtual float ComputeRichLabelWidth()
        {
            float num = base.Size.X - MyGuiConstants.MULTILINE_LABEL_BORDER.X;
            if (this.m_drawScrollbarV)
            {
                num -= this.m_scrollbarSizeV.X;
            }
            return num;
        }

        public void DeactivateIme()
        {
            this.m_isImeActive = false;
        }

        protected void DelayCaretBlink()
        {
            this.m_carriageBlinkerTimer = 0;
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            MyRectangle2D textArea = new MyRectangle2D(this.m_textPadding.TopLeftOffset, base.Size - this.m_textPadding.SizeChange);
            Vector2* vectorPtr1 = (Vector2*) ref textArea.LeftTop;
            vectorPtr1[0] += base.GetPositionAbsoluteTopLeft();
            Vector2 carriageOffset = this.GetCarriageOffset(this.CarriagePositionIndex);
            RectangleF rectangle = new RectangleF(textArea.LeftTop, textArea.Size);
            RectangleF* efPtr1 = (RectangleF*) ref rectangle;
            efPtr1.X -= 0.001f;
            RectangleF* efPtr2 = (RectangleF*) ref rectangle;
            efPtr2.Y -= 0.001f;
            this.AdjustScissorRectangle(ref rectangle);
            using (MyGuiManager.UsingScissorRectangle(ref rectangle))
            {
                this.DrawSelectionBackgrounds(textArea, backgroundTransitionAlpha);
                this.DrawText(this.m_scrollbarV.Value, this.m_scrollbarH.Value, transitionAlpha);
                if (base.HasFocus && this.Selectable)
                {
                    int num = this.m_carriageBlinkerTimer % 60;
                    if ((num >= 0) && (num <= 0x2d))
                    {
                        MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", textArea.LeftTop + carriageOffset, 1, this.GetCarriageHeight(), ApplyColorMaskModifiers(Vector4.One, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true);
                    }
                }
                this.m_carriageBlinkerTimer++;
            }
            if (this.m_drawScrollbarV)
            {
                this.m_scrollbarV.Draw(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha));
            }
            if (this.m_drawScrollbarH)
            {
                this.m_scrollbarH.Draw(ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha));
            }
        }

        private void DrawSelectionBackgrounds(MyRectangle2D textArea, float transitionAlpha)
        {
            char[] separator = new char[] { '\n' };
            int start = this.m_selection.Start;
            foreach (string str in this.Text.ToString().Substring(this.m_selection.Start, this.m_selection.Length).Split(separator))
            {
                Vector2 normalizedCoord = textArea.LeftTop + this.GetCarriageOffset(start);
                Vector2 vector2 = this.GetCarriageOffset(start + str.Length) - this.GetCarriageOffset(start);
                Vector2 normalizedSize = new Vector2(vector2.X, this.GetCarriageHeight());
                MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", normalizedCoord, normalizedSize, ApplyColorMaskModifiers(new Vector4(1f, 1f, 1f, 0.5f), base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                start += str.Length + 1;
            }
        }

        private unsafe void DrawText(float offsetY, float offsetX, float alphamask)
        {
            Vector2 position = base.GetPositionAbsoluteTopLeft() + this.m_textPadding.TopLeftOffset;
            Vector2 drawSizeMax = base.Size - this.m_textPadding.SizeChange;
            if (this.m_drawScrollbarV && this.m_scrollbarV.Visible)
            {
                float* singlePtr1 = (float*) ref drawSizeMax.X;
                singlePtr1[0] -= this.m_scrollbarV.Size.X;
            }
            if (this.m_drawScrollbarH && this.m_scrollbarH.Visible)
            {
                float* singlePtr2 = (float*) ref drawSizeMax.Y;
                singlePtr2[0] -= this.m_scrollbarH.Size.Y;
            }
            Vector2 textSize = this.TextSize;
            if (textSize.X < drawSizeMax.X)
            {
                switch (this.TextBoxAlign)
                {
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    {
                        float* singlePtr3 = (float*) ref position.X;
                        singlePtr3[0] += (drawSizeMax.X - textSize.X) * 0.5f;
                        break;
                    }
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    {
                        float* singlePtr4 = (float*) ref position.X;
                        singlePtr4[0] += drawSizeMax.X - textSize.X;
                        break;
                    }
                    default:
                        break;
                }
                drawSizeMax.X = textSize.X;
            }
            if (textSize.Y < drawSizeMax.Y)
            {
                switch (this.TextBoxAlign)
                {
                    case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    {
                        float* singlePtr5 = (float*) ref position.Y;
                        singlePtr5[0] += (drawSizeMax.Y - textSize.Y) * 0.5f;
                        break;
                    }
                    case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    {
                        float* singlePtr6 = (float*) ref position.Y;
                        singlePtr6[0] += drawSizeMax.Y - textSize.Y;
                        break;
                    }
                    default:
                        break;
                }
                drawSizeMax.Y = textSize.Y;
            }
            this.m_label.Draw(position, offsetY, offsetX, drawSizeMax, alphamask);
        }

        private float GetCarriageHeight() => 
            MyGuiManager.MeasureString(this.Font, m_letterA, this.TextScaleWithLanguage).Y;

        protected virtual Vector2 GetCarriageOffset(int idx)
        {
            Vector2 vector = new Vector2(-this.m_scrollbarH.Value, -this.m_scrollbarV.Value) + this.m_textPadding.TopLeftOffset;
            int lineStartIndex = this.GetLineStartIndex(idx);
            if ((idx - lineStartIndex) > 0)
            {
                this.m_tmpOffsetMeasure.Clear();
                this.m_tmpOffsetMeasure.AppendSubstring(this.Text, lineStartIndex, idx - lineStartIndex);
                vector.X = MyGuiManager.MeasureString(this.Font, this.m_tmpOffsetMeasure, this.TextScaleWithLanguage).X - this.m_scrollbarH.Value;
            }
            if ((lineStartIndex - 1) > 0)
            {
                this.m_tmpOffsetMeasure.Clear();
                this.m_tmpOffsetMeasure.AppendSubstring(this.Text, 0, lineStartIndex - 1);
                vector.Y = MyGuiManager.MeasureString(this.Font, this.m_tmpOffsetMeasure, this.TextScaleWithLanguage).Y - this.m_scrollbarV.Value;
            }
            return vector;
        }

        protected virtual int GetCarriagePositionFromMouseCursor()
        {
            Vector2 vector = (MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft()) - this.m_textPadding.TopLeftOffset;
            int num = 0;
            float maxValue = float.MaxValue;
            for (int i = 0; i <= this.m_text.Length; i++)
            {
                float num4 = Vector2.Distance(this.GetCarriageOffset(i), vector);
                if (num4 < maxValue)
                {
                    maxValue = num4;
                    num = i;
                }
            }
            return num;
        }

        protected virtual int GetIndexOverCarriage(int idx)
        {
            int lineStartIndex = this.GetLineStartIndex(idx);
            int num2 = lineStartIndex;
            if (lineStartIndex > 0)
            {
                num2 = this.GetLineStartIndex(lineStartIndex - 1);
            }
            this.GetLineEndIndex(idx);
            return (((num2 + idx) - lineStartIndex) - ((num2 == 0) ? 1 : 0));
        }

        protected virtual int GetIndexUnderCarriage(int idx)
        {
            int lineStartIndex = this.GetLineStartIndex(idx);
            return (((this.GetLineEndIndex(idx) + idx) - lineStartIndex) + ((lineStartIndex == 0) ? 1 : 0));
        }

        protected int GetLineEndIndex(int idx)
        {
            if (idx == this.Text.Length)
            {
                return this.Text.Length;
            }
            int index = this.Text.ToString().Substring(idx).IndexOf('\n');
            return ((index == -1) ? this.Text.Length : (idx + index));
        }

        protected virtual int GetLineStartIndex(int idx)
        {
            int num = this.Text.ToString().Substring(0, idx).LastIndexOf('\n');
            return ((num == -1) ? 0 : num);
        }

        protected int GetNextSpace()
        {
            if (this.CarriagePositionIndex == this.m_text.Length)
            {
                return this.m_text.Length;
            }
            int index = this.m_text.ToString().Substring(this.CarriagePositionIndex + 1).IndexOf(" ");
            int num2 = this.m_text.ToString().Substring(this.CarriagePositionIndex + 1).IndexOf("\n");
            if ((index == -1) && (num2 == -1))
            {
                return this.m_text.Length;
            }
            if (index == -1)
            {
                index = 0x7fffffff;
            }
            if (num2 == -1)
            {
                num2 = 0x7fffffff;
            }
            return ((this.CarriagePositionIndex + Math.Min(index, num2)) + 1);
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlMultilineLabel objectBuilder = (MyObjectBuilder_GuiControlMultilineLabel) base.GetObjectBuilder();
            objectBuilder.TextScale = this.TextScale;
            objectBuilder.TextColor = this.TextColor.ToVector4();
            objectBuilder.TextAlign = (int) this.TextAlign;
            objectBuilder.TextBoxAlign = (int) this.TextBoxAlign;
            objectBuilder.Font = this.Font;
            if (this.m_useEnum)
            {
                objectBuilder.Text = this.TextEnum.ToString();
            }
            else
            {
                objectBuilder.Text = this.Text.ToString();
            }
            return objectBuilder;
        }

        protected int GetPreviousSpace()
        {
            if (this.CarriagePositionIndex != 0)
            {
                int num = this.m_text.ToString().Substring(0, this.CarriagePositionIndex).LastIndexOf(" ");
                int num2 = this.m_text.ToString().Substring(0, this.CarriagePositionIndex).LastIndexOf("\n");
                if ((num != -1) || (num2 != -1))
                {
                    return Math.Max(num, num2);
                }
            }
            return 0;
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base.HasFocus && this.Selectable)
            {
                int carriagePositionIndex;
                ThrottledKeyStatus keyStatus = this.m_keyThrottler.GetKeyStatus(MyKeys.Left);
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_WAITING)
                {
                    this.DelayCaretBlink();
                    return this;
                }
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_READY)
                {
                    this.DelayCaretBlink();
                    if (!this.IsImeActive)
                    {
                        if (MyInput.Static.IsAnyCtrlKeyPressed())
                        {
                            this.CarriagePositionIndex = this.GetPreviousSpace();
                        }
                        else
                        {
                            carriagePositionIndex = this.CarriagePositionIndex;
                            this.CarriagePositionIndex = carriagePositionIndex - 1;
                        }
                        if (MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            this.m_selection.SetEnd(this);
                        }
                        else
                        {
                            this.m_selection.Reset(this);
                        }
                    }
                    return this;
                }
                keyStatus = this.m_keyThrottler.GetKeyStatus(MyKeys.Right);
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_WAITING)
                {
                    this.DelayCaretBlink();
                    return this;
                }
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_READY)
                {
                    this.DelayCaretBlink();
                    if (!this.IsImeActive)
                    {
                        if (MyInput.Static.IsAnyCtrlKeyPressed())
                        {
                            this.CarriagePositionIndex = this.GetNextSpace();
                        }
                        else
                        {
                            carriagePositionIndex = this.CarriagePositionIndex + 1;
                            this.CarriagePositionIndex = carriagePositionIndex;
                        }
                        if (MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            this.m_selection.SetEnd(this);
                        }
                        else
                        {
                            this.m_selection.Reset(this);
                        }
                    }
                    return this;
                }
                keyStatus = this.m_keyThrottler.GetKeyStatus(MyKeys.Down);
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_WAITING)
                {
                    this.DelayCaretBlink();
                    return this;
                }
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_READY)
                {
                    this.DelayCaretBlink();
                    if (!this.IsImeActive)
                    {
                        this.CarriagePositionIndex = this.GetIndexUnderCarriage(this.CarriagePositionIndex);
                        if (MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            this.m_selection.SetEnd(this);
                        }
                        else
                        {
                            this.m_selection.Reset(this);
                        }
                    }
                    return this;
                }
                keyStatus = this.m_keyThrottler.GetKeyStatus(MyKeys.Up);
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_WAITING)
                {
                    this.DelayCaretBlink();
                    return this;
                }
                if (keyStatus == ThrottledKeyStatus.PRESSED_AND_READY)
                {
                    this.DelayCaretBlink();
                    if (!this.IsImeActive)
                    {
                        this.CarriagePositionIndex = this.GetIndexOverCarriage(this.CarriagePositionIndex);
                        if (MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            this.m_selection.SetEnd(this);
                        }
                        else
                        {
                            this.m_selection.Reset(this);
                        }
                    }
                    return this;
                }
                if (!this.IsImeActive)
                {
                    if (this.m_keyThrottler.IsNewPressAndThrottled(MyKeys.C) && MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        this.m_selection.CopyText(this);
                    }
                    if (this.m_keyThrottler.IsNewPressAndThrottled(MyKeys.A) && MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        this.m_selection.SelectAll(this);
                        return this;
                    }
                }
            }
            bool flag = false;
            int num = MyInput.Static.DeltaMouseScrollWheelValue();
            if ((base.IsMouseOver && (num != 0)) && (this.m_scrollbarV.Visible || this.m_scrollbarH.Visible))
            {
                this.m_scrollbarV.ChangeValue(-0.0005f * num);
                flag = true;
            }
            if (this.m_drawScrollbarV && (this.m_scrollbarV.HandleInput() | flag))
            {
                return this;
            }
            if (this.m_drawScrollbarH && (this.m_scrollbarH.HandleInput() | flag))
            {
                return this;
            }
            if (base.IsMouseOver && this.m_label.HandleInput(base.GetPositionAbsoluteTopLeft(), this.m_scrollbarV.Value, this.m_scrollbarH.Value))
            {
                return this;
            }
            if (this.Selectable)
            {
                if (MyInput.Static.IsNewLeftMousePressed())
                {
                    if (base.IsMouseOver)
                    {
                        this.m_selection.Dragging = true;
                        this.CarriagePositionIndex = this.GetCarriagePositionFromMouseCursor();
                        if (MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            this.m_selection.SetEnd(this);
                        }
                        else
                        {
                            this.m_selection.Reset(this);
                        }
                        return this;
                    }
                    this.m_selection.Reset(this);
                }
                else if (MyInput.Static.IsNewLeftMouseReleased())
                {
                    this.m_selection.Dragging = false;
                }
                else if (this.m_selection.Dragging)
                {
                    if (base.IsMouseOver)
                    {
                        this.CarriagePositionIndex = this.GetCarriagePositionFromMouseCursor();
                        this.m_selection.SetEnd(this);
                    }
                    else if (base.HasFocus)
                    {
                        Vector2 mouseCursorPosition = MyGuiManager.MouseCursorPosition;
                        Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
                        if (mouseCursorPosition.Y < positionAbsoluteTopLeft.Y)
                        {
                            this.m_scrollbarV.ChangeValue(base.Position.Y - mouseCursorPosition.Y);
                        }
                        else if (mouseCursorPosition.Y > (positionAbsoluteTopLeft.Y + base.Size.Y))
                        {
                            this.m_scrollbarV.ChangeValue((mouseCursorPosition.Y - positionAbsoluteTopLeft.Y) - base.Size.Y);
                        }
                        if (mouseCursorPosition.X < positionAbsoluteTopLeft.X)
                        {
                            this.m_scrollbarH.ChangeValue(base.Position.X - mouseCursorPosition.X);
                        }
                        else if (mouseCursorPosition.X > (positionAbsoluteTopLeft.X + base.Size.X))
                        {
                            this.m_scrollbarH.ChangeValue((mouseCursorPosition.X - positionAbsoluteTopLeft.X) - base.Size.X);
                        }
                    }
                }
            }
            return base2;
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            MyStringId id;
            base.Init(objectBuilder);
            this.m_label.MaxLineWidth = this.ComputeRichLabelWidth();
            MyObjectBuilder_GuiControlMultilineLabel label = (MyObjectBuilder_GuiControlMultilineLabel) objectBuilder;
            this.TextAlign = (MyGuiDrawAlignEnum) label.TextAlign;
            this.TextBoxAlign = (MyGuiDrawAlignEnum) label.TextBoxAlign;
            this.TextScale = label.TextScale;
            this.TextColor = new Color(label.TextColor);
            this.Font = label.Font;
            if (Enum.TryParse<MyStringId>(label.Text, out id))
            {
                this.TextEnum = id;
            }
            else
            {
                this.Text = new StringBuilder(label.Text);
            }
        }

        private void OnLinkClickedInternal(string url)
        {
            if (this.OnLinkClicked != null)
            {
                this.OnLinkClicked(this, url);
            }
        }

        protected override void OnSizeChanged()
        {
            if (this.m_label != null)
            {
                this.m_label.MaxLineWidth = this.ComputeRichLabelWidth();
                this.RefreshText(this.m_useEnum);
            }
            if (this.m_drawScrollbarV || this.m_drawScrollbarH)
            {
                this.RecalculateScrollBar();
            }
            base.OnSizeChanged();
        }

        public void Parse()
        {
            this.Clear();
            this.Parse(this.Text.ToString(), this.Font, this.TextScale, this.TextColor);
        }

        public void Parse(string text, MyFontEnum font, float textScale, Color textColor)
        {
            char[] separator = new char[] { '[', ']' };
            bool flag = false;
            string[] strArray = text.ToString().Replace("[[", @"\u005B").Replace("]]", @"\u005D").ToString().Split(separator);
            for (int i = 0; i < strArray.Length; i++)
            {
                string str = strArray[i].Replace(@"\u005B", "[").Replace(@"\u005D", "]");
                if (flag)
                {
                    this.AppendText(str, (string) font, textScale, Color.Yellow.ToVector4());
                }
                else
                {
                    this.AppendText(str, (string) font, textScale, (Vector4) textColor);
                }
                flag = !flag;
            }
        }

        public void RecalculateScrollBar()
        {
            float y = this.m_label.Size.Y;
            bool flag = (base.Size.Y - this.m_textPadding.SizeChange.Y) < y;
            float x = this.m_label.Size.X;
            bool flag2 = (base.Size.X - this.m_textPadding.SizeChange.X) < x;
            this.m_scrollbarV.Visible = flag;
            this.m_scrollbarV.Init(y, (base.Size.Y - (flag2 ? this.m_scrollbarH.Size.Y : 0f)) - this.m_textPadding.SizeChange.Y);
            this.m_scrollbarV.Layout(new Vector2((0.5f * base.Size.X) - this.m_scrollbarV.Size.X, -0.5f * base.Size.Y), flag2 ? (base.Size.Y - this.m_scrollbarH.Size.Y) : base.Size.Y);
            if (!this.m_drawScrollbarV)
            {
                if (((this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM) || (this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM)) || (this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM))
                {
                    this.m_scrollbarV.Value = 0f;
                }
                else if (((this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP) || (this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP)) || (this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP))
                {
                    this.m_scrollbarV.Value = y;
                }
            }
            this.m_scrollbarH.Visible = flag2;
            this.m_scrollbarH.Init(x, (base.Size.X - (flag ? this.m_scrollbarV.Size.X : 0f)) - this.m_textPadding.SizeChange.X);
            this.m_scrollbarH.Layout(new Vector2(-0.5f * base.Size.X, (0.5f * base.Size.Y) - this.m_scrollbarH.Size.Y), base.Size.X - this.m_scrollbarV.Size.X);
            if (!this.m_drawScrollbarH)
            {
                if (this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
                {
                    goto TR_0000;
                }
                else if ((this.TextAlign != MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) && (this.TextAlign != MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM))
                {
                    if (((this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) || (this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER)) || (this.TextAlign == MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM))
                    {
                        this.m_scrollbarH.Value = x;
                    }
                }
                else
                {
                    goto TR_0000;
                }
            }
            return;
        TR_0000:
            this.m_scrollbarH.Value = 0f;
        }

        public void RefreshText(bool useEnum)
        {
            if (this.m_label != null)
            {
                this.m_label.Clear();
                this.m_useEnum = useEnum;
                if (useEnum)
                {
                    this.AppendText(MyTexts.Get(this.TextEnum));
                }
                else
                {
                    this.AppendText(this.Text);
                }
                if (this.Text.Length < this.CarriagePositionIndex)
                {
                    this.CarriagePositionIndex = this.Text.Length;
                }
                this.m_selection.Reset(this);
            }
        }

        public void ScrollToShowCarriage()
        {
            Vector2 carriageOffset = this.GetCarriageOffset(this.CarriagePositionIndex);
            float carriageHeight = this.GetCarriageHeight();
            Vector2 textSizeWithScrolling = this.TextSizeWithScrolling;
            if ((carriageOffset.Y + carriageHeight) > (textSizeWithScrolling.Y - 0.01f))
            {
                this.m_scrollbarV.ChangeValue((carriageOffset.Y + carriageHeight) - textSizeWithScrolling.Y);
            }
            if (carriageOffset.Y < 0f)
            {
                this.m_scrollbarV.ChangeValue(carriageOffset.Y);
            }
            if (carriageOffset.X > (textSizeWithScrolling.X - 0.01f))
            {
                this.m_scrollbarH.ChangeValue(carriageOffset.X - textSizeWithScrolling.X);
            }
            if (carriageOffset.X < 0f)
            {
                this.m_scrollbarH.ChangeValue(carriageOffset.X);
            }
        }

        public void SetScrollbarPageH(float page)
        {
            this.m_scrollbarOffsetH = 0f;
            this.m_scrollbarH.SetPage(page);
            this.RecalculateScrollBar();
        }

        public void SetScrollbarPageV(float page)
        {
            this.m_scrollbarOffsetV = 0f;
            this.m_scrollbarV.SetPage(page);
            this.RecalculateScrollBar();
        }

        protected int CarriagePositionIndex
        {
            get => 
                this.m_carriagePositionIndex;
            set
            {
                int num = MathHelper.Clamp(value, 0, this.Text.Length);
                if (this.m_carriagePositionIndex != num)
                {
                    this.m_carriagePositionIndex = num;
                    if (!this.CarriageVisible())
                    {
                        this.ScrollToShowCarriage();
                    }
                }
            }
        }

        public bool Selectable =>
            this.m_selectable;

        public virtual StringBuilder Text
        {
            get => 
                this.m_text;
            set
            {
                this.m_text.Clear();
                if (value != null)
                {
                    this.m_text.AppendStringBuilder(value);
                }
                this.RefreshText(false);
            }
        }

        public MyStringId TextEnum
        {
            get => 
                this.m_textEnum;
            set
            {
                this.m_textEnum = value;
                this.RefreshText(true);
            }
        }

        public bool IsImeActive
        {
            get => 
                this.m_isImeActive;
            set => 
                (this.m_isImeActive = value);
        }

        public string Font
        {
            get => 
                this.m_font;
            set
            {
                if (this.m_font != value)
                {
                    this.m_font = value;
                    this.RefreshText(this.m_useEnum);
                }
            }
        }

        public Color TextColor { get; set; }

        public Vector2 TextSize =>
            this.m_label.Size;

        public Vector2 TextSizeWithScrolling
        {
            get
            {
                Vector2 size = base.Size;
                if (this.m_scrollbarV.Visible)
                {
                    float* singlePtr1 = (float*) ref size.X;
                    singlePtr1[0] -= this.m_scrollbarV.Size.X;
                }
                if (this.m_scrollbarH.Visible)
                {
                    float* singlePtr2 = (float*) ref size.Y;
                    singlePtr2[0] -= this.m_scrollbarH.Size.Y;
                }
                return size;
            }
        }

        public int NumberOfRows =>
            this.m_label.NumberOfRows;

        public MyGuiBorderThickness TextPadding
        {
            get => 
                this.m_textPadding;
            set
            {
                this.m_textPadding = value;
                this.RecalculateScrollBar();
            }
        }

        public int CharactersDisplayed
        {
            get => 
                this.m_label.CharactersDisplayed;
            set => 
                (this.m_label.CharactersDisplayed = value);
        }

        public float ScrollbarOffsetV
        {
            get => 
                this.m_scrollbarOffsetV;
            set
            {
                this.m_scrollbarOffsetV = value;
                this.m_scrollbarV.ChangeValue(this.m_scrollbarOffsetV);
                this.RecalculateScrollBar();
            }
        }

        public float ScrollbarOffsetH
        {
            get => 
                this.m_scrollbarOffsetH;
            set
            {
                this.m_scrollbarOffsetH = value;
                this.m_scrollbarH.ChangeValue(this.m_scrollbarOffsetH);
                this.RecalculateScrollBar();
            }
        }

        public float TextScale
        {
            get => 
                this.m_textScale;
            set
            {
                this.m_textScale = value;
                this.TextScaleWithLanguage = value * MyGuiManager.LanguageTextScale;
            }
        }

        public float TextScaleWithLanguage
        {
            get => 
                this.m_textScaleWithLanguage;
            private set => 
                (this.m_textScaleWithLanguage = value);
        }

        public bool ShowTextShadow =>
            this.m_showTextShadow;

        public MyGuiDrawAlignEnum TextAlign
        {
            get => 
                this.m_label.TextAlign;
            set => 
                (this.m_label.TextAlign = value);
        }

        public MyGuiDrawAlignEnum TextBoxAlign { get; set; }

        protected float ScrollbarValueV =>
            this.m_scrollbarV.Value;

        protected float ScrollbarValueH =>
            this.m_scrollbarH.Value;

        protected float SetScrollbarValueV
        {
            set => 
                (this.m_scrollbarV.Value = value);
        }

        protected float SetScrollbarValueH
        {
            set => 
                (this.m_scrollbarH.Value = value);
        }

        protected class MyGuiControlMultilineSelection
        {
            protected int m_startIndex = 0;
            protected int m_endIndex = 0;
            private string ClipboardText;
            private bool m_dragging;

            public void CopyText(MyGuiControlMultilineText sender)
            {
                this.ClipboardText = Regex.Replace(sender.Text.ToString().Substring(this.Start, this.Length), "\n", "\r\n");
                if (!string.IsNullOrEmpty(this.ClipboardText))
                {
                    MyClipboardHelper.SetClipboard(this.ClipboardText);
                }
            }

            public void CutText(MyGuiControlMultilineText sender)
            {
                this.CopyText(sender);
                this.EraseText(sender);
            }

            public void EraseText(MyGuiControlMultilineText sender)
            {
                if (this.Start != this.End)
                {
                    StringBuilder builder = new StringBuilder(sender.Text.ToString().Substring(0, this.Start));
                    sender.CarriagePositionIndex = this.Start;
                    sender.Text = builder.Append(new StringBuilder(sender.Text.ToString().Substring(this.End)));
                }
            }

            private void PasteFromClipboard()
            {
                this.ClipboardText = Clipboard.GetText();
            }

            public void PasteText(MyGuiControlMultilineText sender)
            {
                this.EraseText(sender);
                string str = sender.Text.ToString().Substring(0, sender.CarriagePositionIndex);
                string str2 = sender.Text.ToString().Substring(sender.CarriagePositionIndex);
                Thread thread1 = new Thread(new ThreadStart(this.PasteFromClipboard));
                thread1.ApartmentState = ApartmentState.STA;
                thread1.Start();
                thread1.Join();
                sender.Text = new StringBuilder(str).Append(Regex.Replace(this.ClipboardText, "\r\n", "\n")).Append(str2);
                sender.CarriagePositionIndex = str.Length + this.ClipboardText.Length;
                this.Reset(sender);
            }

            public void Reset(MyGuiControlMultilineText sender)
            {
                this.m_startIndex = this.m_endIndex = MathHelper.Clamp(sender.CarriagePositionIndex, 0, sender.Text.Length);
            }

            public void SelectAll(MyGuiControlMultilineText sender)
            {
                this.m_startIndex = 0;
                this.m_endIndex = sender.Text.Length;
                sender.CarriagePositionIndex = sender.Text.Length;
            }

            public void SetEnd(MyGuiControlMultilineText sender)
            {
                this.m_endIndex = MathHelper.Clamp(sender.CarriagePositionIndex, 0, sender.Text.Length);
            }

            public bool Dragging
            {
                get => 
                    this.m_dragging;
                set => 
                    (this.m_dragging = value);
            }

            public int Start =>
                Math.Min(this.m_startIndex, this.m_endIndex);

            public int End =>
                Math.Max(this.m_startIndex, this.m_endIndex);

            public int Length =>
                (this.End - this.Start);
        }
    }
}

