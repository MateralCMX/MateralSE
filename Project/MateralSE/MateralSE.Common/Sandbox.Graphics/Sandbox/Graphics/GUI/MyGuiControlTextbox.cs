namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI.IME;
    using Sandbox.Gui.IME;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlTextbox))]
    public class MyGuiControlTextbox : MyGuiControlBase, IMyImeActiveControl
    {
        private static StyleDefinition[] m_styles = new StyleDefinition[MyUtils.GetMaxValueFromEnum<MyGuiControlTextboxStyleEnum>() + 1];
        private int m_carriageBlinkerTimer;
        private int m_carriagePositionIndex;
        private bool m_drawBackground;
        private bool m_formattedAlready;
        private int m_maxLength;
        private List<MyKeys> m_pressedKeys;
        private Vector4 m_textColor;
        private float m_textScale;
        private float m_textScaleWithLanguage;
        private bool m_hadFocusLastTime;
        private float m_slidingWindowOffset;
        private MyRectangle2D m_textAreaRelative;
        private MyGuiCompositeTexture m_compositeBackground;
        private StringBuilder m_text;
        private MyGuiControlTextboxSelection m_selection;
        private bool m_isImeActive;
        public MyGuiControlTextboxType Type;
        private MyGuiControlTextboxStyleEnum m_visualStyle;
        private StyleDefinition m_styleDef;
        private StyleDefinition m_customStyle;
        private bool m_useCustomStyle;
        private static MyKeyThrottler m_keyThrottler;
        [CompilerGenerated]
        private Action<MyGuiControlTextbox> TextChanged;
        [CompilerGenerated]
        private Action<MyGuiControlTextbox> EnterPressed;

        public event Action<MyGuiControlTextbox> EnterPressed
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlTextbox> enterPressed = this.EnterPressed;
                while (true)
                {
                    Action<MyGuiControlTextbox> a = enterPressed;
                    Action<MyGuiControlTextbox> action3 = (Action<MyGuiControlTextbox>) Delegate.Combine(a, value);
                    enterPressed = Interlocked.CompareExchange<Action<MyGuiControlTextbox>>(ref this.EnterPressed, action3, a);
                    if (ReferenceEquals(enterPressed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlTextbox> enterPressed = this.EnterPressed;
                while (true)
                {
                    Action<MyGuiControlTextbox> source = enterPressed;
                    Action<MyGuiControlTextbox> action3 = (Action<MyGuiControlTextbox>) Delegate.Remove(source, value);
                    enterPressed = Interlocked.CompareExchange<Action<MyGuiControlTextbox>>(ref this.EnterPressed, action3, source);
                    if (ReferenceEquals(enterPressed, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyGuiControlTextbox> TextChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlTextbox> textChanged = this.TextChanged;
                while (true)
                {
                    Action<MyGuiControlTextbox> a = textChanged;
                    Action<MyGuiControlTextbox> action3 = (Action<MyGuiControlTextbox>) Delegate.Combine(a, value);
                    textChanged = Interlocked.CompareExchange<Action<MyGuiControlTextbox>>(ref this.TextChanged, action3, a);
                    if (ReferenceEquals(textChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlTextbox> textChanged = this.TextChanged;
                while (true)
                {
                    Action<MyGuiControlTextbox> source = textChanged;
                    Action<MyGuiControlTextbox> action3 = (Action<MyGuiControlTextbox>) Delegate.Remove(source, value);
                    textChanged = Interlocked.CompareExchange<Action<MyGuiControlTextbox>>(ref this.TextChanged, action3, source);
                    if (ReferenceEquals(textChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyGuiControlTextbox()
        {
            StyleDefinition definition1 = new StyleDefinition();
            definition1.NormalTexture = MyGuiConstants.TEXTURE_TEXTBOX;
            definition1.HighlightTexture = MyGuiConstants.TEXTURE_TEXTBOX_HIGHLIGHT;
            definition1.NormalFont = "Blue";
            definition1.HighlightFont = "White";
            m_styles[0] = definition1;
            StyleDefinition definition2 = new StyleDefinition();
            definition2.NormalTexture = MyGuiConstants.TEXTURE_TEXTBOX;
            definition2.HighlightTexture = MyGuiConstants.TEXTURE_TEXTBOX_HIGHLIGHT;
            definition2.NormalFont = "Debug";
            definition2.HighlightFont = "Debug";
            m_styles[1] = definition2;
            m_keyThrottler = new MyKeyThrottler();
        }

        public MyGuiControlTextbox() : this(nullable, null, 0x200, nullable2, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default)
        {
        }

        public MyGuiControlTextbox(Vector2? position = new Vector2?(), string defaultText = null, int maxLength = 0x200, Vector4? textColor = new Vector4?(), float textScale = 0.8f, MyGuiControlTextboxType type = 0, MyGuiControlTextboxStyleEnum visualStyle = 0) : base(position, new Vector2?(new Vector2(512f, 48f) / MyGuiConstants.GUI_OPTIMAL_SIZE), nullable, null, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_pressedKeys = new List<MyKeys>(10);
            this.m_text = new StringBuilder();
            this.m_selection = new MyGuiControlTextboxSelection();
            Vector4? nullable = null;
            base.Name = "Textbox";
            this.Type = type;
            this.m_carriagePositionIndex = 0;
            this.m_carriageBlinkerTimer = 0;
            nullable = textColor;
            this.m_textColor = (nullable != null) ? nullable.GetValueOrDefault() : Vector4.One;
            this.TextScale = textScale;
            this.m_maxLength = maxLength;
            this.Text = defaultText ?? "";
            this.m_visualStyle = visualStyle;
            this.RefreshVisualStyle();
            this.m_slidingWindowOffset = 0f;
        }

        private void ApplyBackspace()
        {
            if (this.CarriagePositionIndex > 0)
            {
                int num = this.CarriagePositionIndex - 1;
                this.CarriagePositionIndex = num;
                this.m_text.Remove(this.CarriagePositionIndex, 1);
            }
        }

        private void ApplyBackspaceMultiple(int count)
        {
            if (this.CarriagePositionIndex >= count)
            {
                this.CarriagePositionIndex -= count;
                this.m_text.Remove(this.CarriagePositionIndex, count);
            }
        }

        private void ApplyDelete()
        {
            if (this.CarriagePositionIndex < this.m_text.Length)
            {
                this.m_text.Remove(this.CarriagePositionIndex, 1);
            }
        }

        public void ApplyStyle(StyleDefinition style)
        {
            this.m_useCustomStyle = true;
            this.m_customStyle = style;
            this.RefreshVisualStyle();
        }

        public void DeactivateIme()
        {
            this.m_isImeActive = false;
        }

        private unsafe void DebugDraw()
        {
            MyRectangle2D textAreaRelative = this.m_textAreaRelative;
            Vector2* vectorPtr1 = (Vector2*) ref textAreaRelative.LeftTop;
            vectorPtr1[0] += base.GetPositionAbsoluteTopLeft();
            MyGuiManager.DrawBorders(textAreaRelative.LeftTop, textAreaRelative.Size, Color.White, 1);
        }

        private void DelayCaretBlink()
        {
            this.m_carriageBlinkerTimer = 0;
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if (base.Visible)
            {
                this.m_compositeBackground.Draw(base.GetPositionAbsoluteTopLeft(), base.Size, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha), 1f);
                base.Draw(transitionAlpha, backgroundTransitionAlpha);
                MyRectangle2D textAreaRelative = this.m_textAreaRelative;
                Vector2* vectorPtr1 = (Vector2*) ref textAreaRelative.LeftTop;
                vectorPtr1[0] += base.GetPositionAbsoluteTopLeft();
                float carriageOffset = this.GetCarriageOffset(this.CarriagePositionIndex);
                RectangleF normalizedRectangle = new RectangleF(textAreaRelative.LeftTop, new Vector2(textAreaRelative.Size.X, textAreaRelative.Size.Y * 2f));
                using (MyGuiManager.UsingScissorRectangle(ref normalizedRectangle))
                {
                    this.RefreshSlidingWindow();
                    if (this.m_selection.Length > 0)
                    {
                        float num2 = this.GetCarriageOffset(this.m_selection.End) - this.GetCarriageOffset(this.m_selection.Start);
                        MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", new Vector2(textAreaRelative.LeftTop.X + this.GetCarriageOffset(this.m_selection.Start), textAreaRelative.LeftTop.Y), new Vector2(num2 + 0.002f, textAreaRelative.Size.Y * 1.38f), ApplyColorMaskModifiers(new Vector4(1f, 1f, 1f, 0.5f), base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true);
                    }
                    MyGuiManager.DrawString(this.TextFont, new StringBuilder(this.GetModifiedText()), new Vector2(textAreaRelative.LeftTop.X + this.m_slidingWindowOffset, textAreaRelative.LeftTop.Y), this.TextScaleWithLanguage, new Color?(ApplyColorMaskModifiers(this.m_textColor, base.Enabled, transitionAlpha)), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
                    if (base.HasFocus)
                    {
                        int num3 = this.m_carriageBlinkerTimer % 60;
                        if ((num3 >= 0) && (num3 <= 0x2d))
                        {
                            if (this.CarriagePositionIndex == 0)
                            {
                                carriageOffset += 0.0005f;
                            }
                            MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", new Vector2(textAreaRelative.LeftTop.X + carriageOffset, base.GetPositionAbsoluteTopLeft().Y), 1, base.Size.Y, ApplyColorMaskModifiers(Vector4.One, base.Enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true);
                        }
                    }
                    this.m_carriageBlinkerTimer++;
                }
            }
        }

        public void FocusEnded()
        {
            this.OnFocusChanged(false);
        }

        public float GetCarriageOffset(int index)
        {
            string str = this.GetModifiedText().Substring(0, index);
            return ((str.Length == 0) ? this.m_slidingWindowOffset : (MyGuiManager.MeasureString("Blue", new StringBuilder(str), this.TextScaleWithLanguage).X + this.m_slidingWindowOffset));
        }

        public unsafe Vector2 GetCarriagePosition(int shiftX)
        {
            Vector2 vector;
            int num = this.Text.Length - shiftX;
            Vector2* vectorPtr1 = (Vector2*) new Vector2(this.GetCarriageOffset((num >= 0) ? num : 0), 0f);
            vectorPtr1 = (Vector2*) ref vector;
            float* singlePtr1 = (float*) ref vector.X;
            singlePtr1[0] += 0.009f;
            return vector;
        }

        private int GetCarriagePositionFromMouseCursor()
        {
            this.RefreshSlidingWindow();
            float num = (MyGuiManager.MouseCursorPosition.X - base.GetPositionAbsoluteTopLeft().X) - this.m_textAreaRelative.LeftTop.X;
            int num2 = 0;
            float maxValue = float.MaxValue;
            for (int i = 0; i <= this.m_text.Length; i++)
            {
                float carriageOffset = this.GetCarriageOffset(i);
                float num6 = Math.Abs((float) (num - carriageOffset));
                if (num6 < maxValue)
                {
                    maxValue = num6;
                    num2 = i;
                }
            }
            return num2;
        }

        public Vector2 GetCornerPosition() => 
            base.GetPositionAbsoluteBottomLeft();

        public int GetMaxLength() => 
            this.m_maxLength;

        private string GetModifiedText()
        {
            switch (this.Type)
            {
                case MyGuiControlTextboxType.Normal:
                case MyGuiControlTextboxType.DigitsOnly:
                    return this.Text;

                case MyGuiControlTextboxType.Password:
                    return new string('*', this.m_text.Length);
            }
            return this.Text;
        }

        private int GetNextSpace()
        {
            if (this.CarriagePositionIndex == this.m_text.Length)
            {
                return this.m_text.Length;
            }
            int index = this.m_text.ToString().Substring(this.CarriagePositionIndex + 1).IndexOf(" ");
            return ((index != -1) ? ((this.CarriagePositionIndex + index) + 1) : this.m_text.Length);
        }

        private int GetPreviousSpace()
        {
            if (this.CarriagePositionIndex == 0)
            {
                return 0;
            }
            int num = this.m_text.ToString().Substring(0, this.CarriagePositionIndex).LastIndexOf(" ");
            return ((num != -1) ? num : 0);
        }

        public int GetSelectionLength() => 
            ((this.m_selection != null) ? this.m_selection.Length : 0);

        public void GetText(StringBuilder result)
        {
            result.AppendStringBuilder(this.m_text);
        }

        public int GetTextLength() => 
            this.Text.Length;

        public static StyleDefinition GetVisualStyle(MyGuiControlTextboxStyleEnum style) => 
            m_styles[(int) style];

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase ret = base.HandleInput();
            try
            {
                if ((ret == null) && base.Enabled)
                {
                    if (!MyInput.Static.IsNewLeftMousePressed())
                    {
                        if (MyInput.Static.IsNewLeftMouseReleased())
                        {
                            this.m_selection.Dragging = false;
                        }
                        else if (this.m_selection.Dragging)
                        {
                            if (MyImeProcessor.Instance != null)
                            {
                                MyImeProcessor.Instance.CaretRepositionReaction();
                            }
                            this.CarriagePositionIndex = this.GetCarriagePositionFromMouseCursor();
                            this.m_selection.SetEnd(this);
                            ret = this;
                        }
                    }
                    else if (!base.IsMouseOver)
                    {
                        this.m_selection.Reset(this);
                    }
                    else
                    {
                        if (MyImeProcessor.Instance != null)
                        {
                            MyImeProcessor.Instance.CaretRepositionReaction();
                        }
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
                        ret = this;
                    }
                    if (!base.HasFocus)
                    {
                        if (((this.Type == MyGuiControlTextboxType.DigitsOnly) && !this.m_formattedAlready) && (this.m_text.Length != 0))
                        {
                            decimal decimalFromString = MyValueFormatter.GetDecimalFromString(this.Text, 1);
                            int decimalDigits = ((decimalFromString - decimal.Truncate(decimalFromString)) > 0M) ? 1 : 0;
                            this.m_text.Clear().Append(MyValueFormatter.GetFormatedFloat((float) decimalFromString, decimalDigits, ""));
                            this.CarriagePositionIndex = this.m_text.Length;
                            this.m_formattedAlready = true;
                        }
                    }
                    else
                    {
                        int carriagePositionIndex;
                        if (!MyInput.Static.IsAnyCtrlKeyPressed())
                        {
                            this.HandleTextInputBuffered(ref ret);
                        }
                        if (m_keyThrottler.GetKeyStatus(MyKeys.Left) == ThrottledKeyStatus.PRESSED_AND_READY)
                        {
                            ret = this;
                            if (!this.m_isImeActive)
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
                                this.DelayCaretBlink();
                            }
                        }
                        if (m_keyThrottler.GetKeyStatus(MyKeys.Right) == ThrottledKeyStatus.PRESSED_AND_READY)
                        {
                            ret = this;
                            if (!this.m_isImeActive)
                            {
                                if (MyInput.Static.IsAnyCtrlKeyPressed())
                                {
                                    this.CarriagePositionIndex = this.GetNextSpace();
                                }
                                else
                                {
                                    carriagePositionIndex = this.CarriagePositionIndex;
                                    this.CarriagePositionIndex = carriagePositionIndex + 1;
                                }
                                if (MyInput.Static.IsAnyShiftKeyPressed())
                                {
                                    this.m_selection.SetEnd(this);
                                }
                                else
                                {
                                    this.m_selection.Reset(this);
                                }
                                this.DelayCaretBlink();
                            }
                        }
                        if (((m_keyThrottler.GetKeyStatus(MyKeys.Back) == ThrottledKeyStatus.PRESSED_AND_READY) && MyInput.Static.IsAnyCtrlKeyPressed()) && !this.m_isImeActive)
                        {
                            ret = this;
                            this.CarriagePositionIndex = this.GetPreviousSpace();
                            this.m_selection.SetEnd(this);
                            this.m_selection.EraseText(this);
                        }
                        if (((m_keyThrottler.GetKeyStatus(MyKeys.Delete) == ThrottledKeyStatus.PRESSED_AND_READY) && MyInput.Static.IsAnyCtrlKeyPressed()) && !this.m_isImeActive)
                        {
                            ret = this;
                            this.CarriagePositionIndex = this.GetNextSpace();
                            this.m_selection.SetEnd(this);
                            this.m_selection.EraseText(this);
                        }
                        if (!this.IsImeActive)
                        {
                            if (m_keyThrottler.IsNewPressAndThrottled(MyKeys.Home))
                            {
                                this.CarriagePositionIndex = 0;
                                if (MyInput.Static.IsAnyShiftKeyPressed())
                                {
                                    this.m_selection.SetEnd(this);
                                }
                                else
                                {
                                    this.m_selection.Reset(this);
                                }
                                ret = this;
                                this.DelayCaretBlink();
                            }
                            if (m_keyThrottler.IsNewPressAndThrottled(MyKeys.End))
                            {
                                this.CarriagePositionIndex = this.m_text.Length;
                                if (MyInput.Static.IsAnyShiftKeyPressed())
                                {
                                    this.m_selection.SetEnd(this);
                                }
                                else
                                {
                                    this.m_selection.Reset(this);
                                }
                                ret = this;
                                this.DelayCaretBlink();
                            }
                            if (m_keyThrottler.IsNewPressAndThrottled(MyKeys.X) && MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                this.m_selection.CutText(this);
                            }
                            if (m_keyThrottler.IsNewPressAndThrottled(MyKeys.C) && MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                this.m_selection.CopyText(this);
                            }
                            if (m_keyThrottler.IsNewPressAndThrottled(MyKeys.V) && MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                this.m_selection.PasteText(this);
                            }
                            if (m_keyThrottler.IsNewPressAndThrottled(MyKeys.A) && MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                this.m_selection.SelectAll(this);
                            }
                            if (MyInput.Static.IsNewKeyPressed(MyKeys.Enter) && (this.EnterPressed != null))
                            {
                                this.EnterPressed(this);
                            }
                        }
                        this.m_formattedAlready = false;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
            }
            this.m_hadFocusLastTime = base.HasFocus;
            return ret;
        }

        private void HandleTextInputBuffered(ref MyGuiControlBase ret)
        {
            bool flag = false;
            foreach (char ch in MyInput.Static.TextInput)
            {
                if (!this.IsSkipCharacter((MyKeys) ((byte) ch)))
                {
                    if (char.IsControl(ch))
                    {
                        if (ch != '\b')
                        {
                            continue;
                        }
                        this.KeypressBackspace(true);
                        flag = true;
                        continue;
                    }
                    if (this.m_selection.Length > 0)
                    {
                        this.m_selection.EraseText(this);
                    }
                    this.InsertChar(true, ch);
                    flag = true;
                }
            }
            if (m_keyThrottler.GetKeyStatus(MyKeys.Delete) == ThrottledKeyStatus.PRESSED_AND_READY)
            {
                this.KeypressDelete(true);
                flag = true;
            }
            if (flag)
            {
                this.OnTextChanged();
                ret = this;
            }
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            this.m_slidingWindowOffset = 0f;
            this.m_carriagePositionIndex = 0;
        }

        public void InsertChar(bool conpositionEnd, char character)
        {
            if (this.m_selection.Length > 0)
            {
                this.m_selection.EraseText(this);
            }
            if (this.m_text.Length < this.m_maxLength)
            {
                this.m_text.Insert(this.CarriagePositionIndex, character);
                int num = this.CarriagePositionIndex + 1;
                this.CarriagePositionIndex = num;
                this.OnTextChanged();
            }
        }

        public void InsertCharMultiple(bool conpositionEnd, string chars)
        {
            if (this.m_selection.Length > 0)
            {
                this.m_selection.EraseText(this);
            }
            if ((this.m_text.Length + chars.Length) <= this.m_maxLength)
            {
                this.m_text.Insert(this.CarriagePositionIndex, chars);
                this.CarriagePositionIndex += chars.Length;
            }
        }

        public bool IsSkipCharacter(MyKeys character)
        {
            if (this.SkipCombinations != null)
            {
                foreach (MySkipCombination combination in this.SkipCombinations)
                {
                    if (((combination.Alt == MyInput.Static.IsAnyAltKeyPressed()) && ((combination.Ctrl == MyInput.Static.IsAnyCtrlKeyPressed()) && (combination.Shift == MyInput.Static.IsAnyShiftKeyPressed()))) && ((combination.Keys == null) || combination.Keys.Contains<MyKeys>(character)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void KeypressBackspace(bool compositionEnd)
        {
            if (this.m_selection.Length == 0)
            {
                this.ApplyBackspace();
            }
            else
            {
                this.m_selection.EraseText(this);
            }
            this.OnTextChanged();
        }

        public void KeypressBackspaceMultiple(bool conpositionEnd, int count)
        {
            if (this.m_selection.Length == 0)
            {
                this.ApplyBackspaceMultiple(count);
            }
            else
            {
                this.m_selection.EraseText(this);
            }
            this.OnTextChanged();
        }

        public void KeypressDelete(bool compositionEnd)
        {
            if (this.m_selection.Length == 0)
            {
                this.ApplyDelete();
            }
            else
            {
                this.m_selection.EraseText(this);
            }
            this.OnTextChanged();
        }

        public void KeypressEnter(bool compositionEnd)
        {
            if (this.EnterPressed != null)
            {
                this.EnterPressed(this);
            }
        }

        public void KeypressRedo()
        {
        }

        public void KeypressUndo()
        {
        }

        public void MoveCarriageToEnd()
        {
            this.CarriagePositionIndex = this.m_text.Length;
        }

        internal override void OnFocusChanged(bool focus)
        {
            if (focus)
            {
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Tab))
                {
                    this.MoveCarriageToEnd();
                    this.m_selection.SelectAll(this);
                }
                if (MyImeProcessor.Instance != null)
                {
                    MyImeProcessor.Instance.Activate(this);
                }
            }
            else
            {
                if (this.EnterPressed != null)
                {
                    this.EnterPressed(this);
                }
                this.m_selection.Reset(this);
                if (MyImeProcessor.Instance != null)
                {
                    MyImeProcessor.Instance.Deactivate();
                }
            }
            base.OnFocusChanged(focus);
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            this.RefreshInternals();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.RefreshTextArea();
            this.RefreshSlidingWindow();
        }

        private void OnTextChanged()
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(this);
            }
            this.RefreshSlidingWindow();
            this.m_selection.Reset(this);
            this.DelayCaretBlink();
        }

        private void RefreshInternals()
        {
            if (base.HasHighlight)
            {
                this.m_compositeBackground = this.m_styleDef.HighlightTexture;
                base.MinSize = this.m_compositeBackground.MinSizeGui * this.TextScale;
                base.MaxSize = this.m_compositeBackground.MaxSizeGui * this.TextScale;
                this.TextFont = this.m_styleDef.HighlightFont;
            }
            else
            {
                this.m_compositeBackground = this.m_styleDef.NormalTexture;
                base.MinSize = this.m_compositeBackground.MinSizeGui * this.TextScale;
                base.MaxSize = this.m_compositeBackground.MaxSizeGui * this.TextScale;
                this.TextFont = this.m_styleDef.NormalFont;
            }
            this.RefreshTextArea();
        }

        private void RefreshSlidingWindow()
        {
            float carriageOffset = this.GetCarriageOffset(this.CarriagePositionIndex);
            MyRectangle2D textAreaRelative = this.m_textAreaRelative;
            if (carriageOffset < 0f)
            {
                this.m_slidingWindowOffset -= carriageOffset;
            }
            else if (carriageOffset > textAreaRelative.Size.X)
            {
                this.m_slidingWindowOffset -= carriageOffset - textAreaRelative.Size.X;
            }
        }

        private void RefreshTextArea()
        {
            this.m_textAreaRelative = new MyRectangle2D(MyGuiConstants.TEXTBOX_TEXT_OFFSET, base.Size - (2f * MyGuiConstants.TEXTBOX_TEXT_OFFSET));
        }

        private void RefreshVisualStyle()
        {
            this.m_styleDef = !this.m_useCustomStyle ? GetVisualStyle(this.VisualStyle) : this.m_customStyle;
            this.RefreshInternals();
        }

        public void SelectAll()
        {
            if (this.m_selection != null)
            {
                this.m_selection.SelectAll(this);
            }
        }

        public void SetText(StringBuilder source)
        {
            this.m_text.Clear().AppendStringBuilder(source);
            if (this.CarriagePositionIndex >= this.m_text.Length)
            {
                this.CarriagePositionIndex = this.m_text.Length;
            }
            this.OnTextChanged();
        }

        public bool TextEquals(StringBuilder text) => 
            (this.m_text.CompareTo(text) == 0);

        public bool IsImeActive
        {
            get => 
                this.m_isImeActive;
            set => 
                (this.m_isImeActive = value);
        }

        public int MaxLength
        {
            get => 
                this.m_maxLength;
            set
            {
                this.m_maxLength = value;
                if (this.m_text.Length > this.m_maxLength)
                {
                    this.m_text.Remove(this.m_maxLength, this.m_text.Length - this.m_maxLength);
                }
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

        [Obsolete("Do not use this, it allocates! Use SetText instead!")]
        public string Text
        {
            get => 
                this.m_text.ToString();
            set
            {
                this.m_text.Clear().Append(value);
                if (this.CarriagePositionIndex >= this.m_text.Length)
                {
                    this.CarriagePositionIndex = this.m_text.Length;
                }
                this.OnTextChanged();
            }
        }

        public MyGuiControlTextboxStyleEnum VisualStyle
        {
            get => 
                this.m_visualStyle;
            set
            {
                this.m_visualStyle = value;
                this.RefreshVisualStyle();
            }
        }

        public int CarriagePositionIndex
        {
            get => 
                this.m_carriagePositionIndex;
            private set => 
                (this.m_carriagePositionIndex = MathHelper.Clamp(value, 0, this.Text.Length));
        }

        public MySkipCombination[] SkipCombinations { get; set; }

        public string TextFont { get; private set; }

        private class MyGuiControlTextboxSelection
        {
            private int m_startIndex = 0;
            private int m_endIndex = 0;
            private string ClipboardText;
            private bool m_dragging;

            public void CopyText(MyGuiControlTextbox sender)
            {
                this.ClipboardText = sender.Text.Substring(this.Start, this.Length);
                if (!string.IsNullOrEmpty(this.ClipboardText))
                {
                    MyClipboardHelper.SetClipboard(this.ClipboardText);
                }
            }

            public void CutText(MyGuiControlTextbox sender)
            {
                this.CopyText(sender);
                this.EraseText(sender);
            }

            public void EraseText(MyGuiControlTextbox sender)
            {
                if (this.Start != this.End)
                {
                    StringBuilder builder = new StringBuilder(sender.Text.Substring(0, this.Start));
                    sender.CarriagePositionIndex = this.Start;
                    sender.Text = builder.Append(new StringBuilder(sender.Text.Substring(this.End))).ToString();
                }
            }

            private void PasteFromClipboard()
            {
                this.ClipboardText = Clipboard.GetText();
            }

            public void PasteText(MyGuiControlTextbox sender)
            {
                string str4;
                this.EraseText(sender);
                string str = sender.Text.Substring(0, sender.CarriagePositionIndex);
                string str2 = sender.Text.Substring(sender.CarriagePositionIndex);
                Thread thread1 = new Thread(new ThreadStart(this.PasteFromClipboard));
                thread1.ApartmentState = ApartmentState.STA;
                thread1.Start();
                thread1.Join();
                string str3 = this.ClipboardText.Replace("\n", "");
                if ((str3.Length + sender.Text.Length) <= sender.MaxLength)
                {
                    str4 = str3;
                }
                else
                {
                    int length = sender.MaxLength - sender.Text.Length;
                    str4 = (length <= 0) ? "" : str3.Substring(0, length);
                }
                sender.Text = new StringBuilder(str).Append(str4).Append(str2).ToString();
                sender.CarriagePositionIndex = str.Length + str4.Length;
                this.Reset(sender);
            }

            public void Reset(MyGuiControlTextbox sender)
            {
                this.m_startIndex = this.m_endIndex = MathHelper.Clamp(sender.CarriagePositionIndex, 0, sender.Text.Length);
            }

            public void SelectAll(MyGuiControlTextbox sender)
            {
                this.m_startIndex = 0;
                this.m_endIndex = sender.Text.Length;
                sender.CarriagePositionIndex = sender.Text.Length;
            }

            public void SetEnd(MyGuiControlTextbox sender)
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

        [StructLayout(LayoutKind.Sequential)]
        public struct MySkipCombination
        {
            public bool Alt;
            public bool Ctrl;
            public bool Shift;
            public MyKeys[] Keys;
        }

        public class StyleDefinition
        {
            public string NormalFont;
            public string HighlightFont;
            public MyGuiCompositeTexture NormalTexture;
            public MyGuiCompositeTexture HighlightTexture;
        }
    }
}

