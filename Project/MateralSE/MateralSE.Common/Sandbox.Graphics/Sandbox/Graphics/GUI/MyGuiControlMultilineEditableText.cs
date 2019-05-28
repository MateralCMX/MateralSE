namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI.IME;
    using Sandbox.Gui.IME;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlMultilineEditableLabel))]
    public class MyGuiControlMultilineEditableText : MyGuiControlMultilineText, IMyImeActiveControl
    {
        private int m_previousTextSize;
        private List<int> m_lineInformation;
        private List<string> m_undoCache;
        private List<string> m_redoCache;
        private const int TAB_SIZE = 4;
        private const int MAX_UNDO_HISTORY = 50;
        private const char NEW_LINE = '\n';
        private const char BACKSPACE = '\b';
        private const char TAB = '\t';
        private const char CTLR_Z = '\x001a';
        private const char CTLR_Y = '\x0019';
        private int m_currentCarriageLine;
        private int m_previousCarriagePosition;
        private float m_fontHeight;
        private int m_currentCarriageColumn;

        public MyGuiControlMultilineEditableText(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? backgroundColor = new Vector4?(), string font = "Blue", float textScale = 0.8f, MyGuiDrawAlignEnum textAlign = 0, StringBuilder contents = null, bool drawScrollbarV = true, bool drawScrollbarH = true, MyGuiDrawAlignEnum textBoxAlign = 4, int? visibleLinesCount = new int?(), MyGuiCompositeTexture backgroundTexture = null, MyGuiBorderThickness? textPadding = new MyGuiBorderThickness?()) : base(position, size, backgroundColor, font, textScale, textAlign, contents, drawScrollbarV, drawScrollbarH, textBoxAlign, visibleLinesCount, true, false, backgroundTexture, textPadding)
        {
            this.m_lineInformation = new List<int>();
            this.m_undoCache = new List<string>();
            this.m_redoCache = new List<string>();
            this.m_fontHeight = MyGuiManager.GetFontHeight(base.Font, base.TextScaleWithLanguage);
            base.AllowFocusingElements = false;
            base.CanHaveFocus = true;
        }

        private void AddToRedo(string text)
        {
            this.m_redoCache.Add(text);
            if (this.m_redoCache.Count > 50)
            {
                this.m_redoCache.RemoveAt(50);
            }
        }

        private void AddToUndo(string text, bool clearRedo = true)
        {
            if (clearRedo)
            {
                this.m_redoCache.Clear();
            }
            this.m_undoCache.Add(text);
            if (this.m_undoCache.Count > 50)
            {
                this.m_undoCache.RemoveAt(0);
            }
        }

        private void ApplyBackspace()
        {
            if (base.CarriagePositionIndex > 0)
            {
                int num = base.CarriagePositionIndex - 1;
                base.CarriagePositionIndex = num;
                base.m_text.Remove(base.CarriagePositionIndex, 1);
                this.BuildLineInformation();
            }
        }

        private void ApplyBackspaceMultiple(int count)
        {
            if (base.CarriagePositionIndex >= count)
            {
                base.CarriagePositionIndex -= count;
                base.m_text.Remove(base.CarriagePositionIndex, count);
                this.BuildLineInformation();
            }
        }

        private void ApplyDelete()
        {
            if (base.CarriagePositionIndex < base.m_text.Length)
            {
                base.m_text.Remove(base.CarriagePositionIndex, 1);
            }
        }

        private void BuildLineInformation()
        {
            if (this.m_previousTextSize != base.m_text.Length)
            {
                this.m_previousTextSize = base.m_text.Length;
                this.m_currentCarriageLine = 0;
                base.m_carriagePositionIndex = MathHelper.Clamp(base.m_carriagePositionIndex, 0, this.Text.Length);
                this.m_lineInformation.Clear();
                this.m_lineInformation.Add(0);
                for (int i = 0; i < base.m_text.Length; i++)
                {
                    if (base.m_text[i] == '\n')
                    {
                        this.m_lineInformation.Add(i);
                    }
                }
                base.ScrollToShowCarriage();
            }
        }

        private int CalculateNewCarriageLine(int idx)
        {
            this.BuildLineInformation();
            for (int i = 1; i < this.m_lineInformation.Count; i++)
            {
                if (idx <= this.m_lineInformation[i])
                {
                    return Math.Max(0, i);
                }
            }
            return this.m_lineInformation.Count;
        }

        private int CalculateNewCarriagePos(int newRowEnd, int newRowStart)
        {
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.CaretRepositionReaction();
            }
            int num = Math.Min(newRowEnd - newRowStart, this.m_currentCarriageColumn);
            int idx = newRowStart + num;
            this.m_currentCarriageLine = this.CalculateNewCarriageLine(idx);
            return idx;
        }

        public bool CarriageMoved()
        {
            if (this.m_previousCarriagePosition == base.m_carriagePositionIndex)
            {
                return false;
            }
            this.m_previousCarriagePosition = base.m_carriagePositionIndex;
            return true;
        }

        protected override float ComputeRichLabelWidth() => 
            float.MaxValue;

        public void FocusEnded()
        {
            this.OnFocusChanged(false);
        }

        private int GetCarriageColumn(int idx)
        {
            int lineStartIndex = this.GetLineStartIndex(idx);
            return (idx - lineStartIndex);
        }

        protected override Vector2 GetCarriageOffset(int idx)
        {
            if (this.m_lineInformation.Count == 0)
            {
                return new Vector2(0f, 0f);
            }
            int start = this.m_lineInformation[this.m_lineInformation.Count - 1];
            Vector2 vector = new Vector2(-base.ScrollbarValueH, -base.ScrollbarValueV);
            int num2 = 0;
            while (true)
            {
                if (num2 < this.m_lineInformation.Count)
                {
                    if (idx > this.m_lineInformation[num2])
                    {
                        num2++;
                        continue;
                    }
                    num2 = Math.Max(0, --num2);
                    start = this.m_lineInformation[num2];
                }
                if ((idx - start) > 0)
                {
                    base.m_tmpOffsetMeasure.Clear();
                    base.m_tmpOffsetMeasure.AppendSubstring(this.Text, start, idx - start);
                    vector.X = MyGuiManager.MeasureString(base.Font, base.m_tmpOffsetMeasure, base.TextScaleWithLanguage).X - base.ScrollbarValueH;
                }
                vector.Y = (Math.Min(num2, this.m_lineInformation.Count - 1) * this.m_fontHeight) - base.ScrollbarValueV;
                return vector;
            }
        }

        public unsafe Vector2 GetCarriagePosition(int shiftX)
        {
            Vector2 carriageOffset = this.GetCarriageOffset(base.CarriagePositionIndex - shiftX);
            float* singlePtr1 = (float*) ref carriageOffset.Y;
            singlePtr1[0] += 0.025f;
            return carriageOffset;
        }

        protected override unsafe int GetCarriagePositionFromMouseCursor()
        {
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.CaretRepositionReaction();
            }
            Vector2 vector = (MyGuiManager.MouseCursorPosition - base.GetPositionAbsoluteTopLeft()) - base.m_textPadding.TopLeftOffset;
            float* singlePtr1 = (float*) ref vector.X;
            singlePtr1[0] += base.ScrollbarValueH;
            float* singlePtr2 = (float*) ref vector.Y;
            singlePtr2[0] += base.ScrollbarValueV;
            int idx = 0;
            int num2 = 0;
            num2 = 0;
            while (true)
            {
                if (num2 < this.m_lineInformation.Count)
                {
                    float num3 = this.m_fontHeight * num2;
                    if ((vector.Y <= num3) || (vector.Y >= (num3 + this.m_fontHeight)))
                    {
                        num2++;
                        continue;
                    }
                    int num4 = (((num2 + 1) >= this.m_lineInformation.Count) ? base.m_text.Length : this.m_lineInformation[num2 + 1]) - this.m_lineInformation[num2];
                    int start = this.m_lineInformation[num2];
                    float num6 = Math.Min(Vector2.Distance(new Vector2(0f, (1 + num2) * this.m_fontHeight), vector), float.MaxValue);
                    for (int i = 0; i < num4; i++)
                    {
                        base.m_tmpOffsetMeasure.Clear();
                        base.m_tmpOffsetMeasure.AppendSubstring(base.m_text, start, i + 1);
                        float x = MyGuiManager.MeasureString(base.Font, base.m_tmpOffsetMeasure, base.TextScaleWithLanguage).X;
                        float num9 = Vector2.Distance(new Vector2(x, vector.Y), vector);
                        if (num9 < num6)
                        {
                            num6 = num9;
                            idx = (start + i) + 1;
                        }
                    }
                }
                this.m_currentCarriageColumn = this.GetCarriageColumn(idx);
                this.m_currentCarriageLine = num2 + 1;
                return idx;
            }
        }

        public Vector2 GetCornerPosition() => 
            base.GetPositionAbsoluteTopLeft();

        public int GetCurrentCarriageLine() => 
            this.m_currentCarriageLine;

        private int GetFirstDiffIndex(string str1, string str2)
        {
            if ((str1 != null) && (str2 != null))
            {
                int num = Math.Min(str1.Length, str2.Length);
                for (int i = 0; i < num; i++)
                {
                    if (str1[i] != str2[i])
                    {
                        return (i + 1);
                    }
                }
            }
            return -1;
        }

        protected override int GetIndexOverCarriage(int idx)
        {
            int lineStartIndex = this.GetLineStartIndex(idx);
            int lineEndIndex = base.GetLineEndIndex(Math.Max(0, lineStartIndex - 1));
            return this.CalculateNewCarriagePos(lineEndIndex, this.GetLineStartIndex(Math.Max(0, lineStartIndex - 1)));
        }

        protected override int GetIndexUnderCarriage(int idx)
        {
            int lineEndIndex = base.GetLineEndIndex(idx);
            int newRowEnd = base.GetLineEndIndex(Math.Min(this.Text.Length, lineEndIndex + 1));
            return this.CalculateNewCarriagePos(newRowEnd, this.GetLineStartIndex(Math.Min(this.Text.Length, lineEndIndex + 1)));
        }

        protected override int GetLineStartIndex(int idx)
        {
            int num = this.Text.ToString().Substring(0, idx).LastIndexOf('\n') + 1;
            return ((num == -1) ? 0 : num);
        }

        public int GetMaxLength() => 
            0x7fffffff;

        public int GetSelectionLength() => 
            ((base.m_selection != null) ? base.m_selection.Length : 0);

        public int GetTextLength() => 
            this.Text.Length;

        public int GetTotalNumLines() => 
            this.m_lineInformation.Count;

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase ret = base.HandleInput();
            if (base.HasFocus && base.Selectable)
            {
                if (MyInput.Static.IsAnyCtrlKeyPressed())
                {
                    ThrottledKeyStatus keyStatus = base.m_keyThrottler.GetKeyStatus(MyKeys.Back);
                    if (keyStatus == ThrottledKeyStatus.PRESSED_AND_WAITING)
                    {
                        return this;
                    }
                    if (keyStatus == ThrottledKeyStatus.PRESSED_AND_READY)
                    {
                        if (!base.IsImeActive)
                        {
                            base.CarriagePositionIndex = base.GetPreviousSpace();
                            base.m_selection.SetEnd(this);
                            base.m_selection.EraseText(this);
                        }
                        return this;
                    }
                    keyStatus = base.m_keyThrottler.GetKeyStatus(MyKeys.Delete);
                    if (keyStatus == ThrottledKeyStatus.PRESSED_AND_WAITING)
                    {
                        return this;
                    }
                    if (keyStatus == ThrottledKeyStatus.PRESSED_AND_READY)
                    {
                        if (!base.IsImeActive)
                        {
                            base.CarriagePositionIndex = base.GetNextSpace();
                            base.m_selection.SetEnd(this);
                            base.m_selection.EraseText(this);
                        }
                        return this;
                    }
                }
                if (base.m_keyThrottler.IsNewPressAndThrottled(MyKeys.X) && MyInput.Static.IsAnyCtrlKeyPressed())
                {
                    if (MyImeProcessor.Instance != null)
                    {
                        MyImeProcessor.Instance.CaretRepositionReaction();
                    }
                    this.AddToUndo(base.m_text.ToString(), true);
                    base.m_selection.CutText(this);
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    return this;
                }
                if (base.m_keyThrottler.IsNewPressAndThrottled(MyKeys.V) && MyInput.Static.IsAnyCtrlKeyPressed())
                {
                    if (MyImeProcessor.Instance != null)
                    {
                        MyImeProcessor.Instance.CaretRepositionReaction();
                    }
                    this.AddToUndo(base.m_text.ToString(), true);
                    base.m_selection.PasteText(this);
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    return this;
                }
                if (base.m_keyThrottler.IsNewPressAndThrottled(MyKeys.Home))
                {
                    int lineStartIndex = this.GetLineStartIndex(base.CarriagePositionIndex);
                    int num2 = lineStartIndex;
                    while ((num2 < this.Text.Length) && (this.Text[num2] == ' '))
                    {
                        num2++;
                    }
                    if ((base.CarriagePositionIndex == num2) || (num2 == this.Text.Length))
                    {
                        base.CarriagePositionIndex = lineStartIndex;
                    }
                    else
                    {
                        base.CarriagePositionIndex = num2;
                    }
                    if (MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        base.CarriagePositionIndex = 0;
                    }
                    if (MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        base.m_selection.SetEnd(this);
                    }
                    else
                    {
                        base.m_selection.Reset(this);
                    }
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    return this;
                }
                if (base.m_keyThrottler.IsNewPressAndThrottled(MyKeys.End))
                {
                    int lineEndIndex = base.GetLineEndIndex(base.CarriagePositionIndex);
                    base.CarriagePositionIndex = lineEndIndex;
                    if (MyInput.Static.IsAnyCtrlKeyPressed())
                    {
                        base.CarriagePositionIndex = this.Text.Length;
                    }
                    if (MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        base.m_selection.SetEnd(this);
                    }
                    else
                    {
                        base.m_selection.Reset(this);
                    }
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    return this;
                }
                if ((MyInput.Static.IsKeyPress(MyKeys.Left) || MyInput.Static.IsKeyPress(MyKeys.Right)) && !base.IsImeActive)
                {
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                }
            }
            this.HandleTextInputBuffered(ref ret);
            return ret;
        }

        protected void HandleTextInputBuffered(ref MyGuiControlBase ret)
        {
            bool flag = false;
            foreach (char ch in MyInput.Static.TextInput)
            {
                if (!char.IsControl(ch))
                {
                    this.AddToUndo(base.m_text.ToString(), true);
                    if (base.m_selection.Length > 0)
                    {
                        base.m_selection.EraseText(this);
                    }
                    this.InsertCharInternal(ch);
                    flag = true;
                    continue;
                }
                if (ch == '\x001a')
                {
                    if (MyImeProcessor.Instance != null)
                    {
                        MyImeProcessor.Instance.CaretRepositionReaction();
                    }
                    this.Undo();
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    continue;
                }
                if (ch == '\x0019')
                {
                    if (MyImeProcessor.Instance != null)
                    {
                        MyImeProcessor.Instance.CaretRepositionReaction();
                    }
                    this.Redo();
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    continue;
                }
                if (ch == '\b')
                {
                    this.AddToUndo(base.m_text.ToString(), true);
                    if (base.m_selection.Length == 0)
                    {
                        this.ApplyBackspace();
                    }
                    else
                    {
                        base.m_selection.EraseText(this);
                    }
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    flag = true;
                    continue;
                }
                if (ch == '\r')
                {
                    this.AddToUndo(base.m_text.ToString(), true);
                    if (base.m_selection.Length != 0)
                    {
                        base.m_selection.EraseText(this);
                    }
                    this.InsertCharInternal('\n');
                    flag = true;
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    continue;
                }
                if (ch == '\t')
                {
                    this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                    this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                    this.AddToUndo(base.m_text.ToString(), true);
                    int num = 4 - (this.m_currentCarriageColumn % 4);
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= num)
                        {
                            flag = num > 0;
                            break;
                        }
                        this.InsertCharInternal(' ');
                        num2++;
                    }
                }
            }
            if (base.m_keyThrottler.GetKeyStatus(MyKeys.Delete) == ThrottledKeyStatus.PRESSED_AND_READY)
            {
                this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                this.AddToUndo(base.m_text.ToString(), true);
                if (base.m_selection.Length == 0)
                {
                    this.ApplyDelete();
                }
                else
                {
                    base.m_selection.EraseText(this);
                }
                flag = true;
            }
            if (flag)
            {
                this.OnTextChanged();
                this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                ret = this;
            }
        }

        public void InsertChar(bool compositionEnd, char character)
        {
            if (compositionEnd)
            {
                this.AddToUndo(base.m_text.ToString(), true);
            }
            if (character != '\t')
            {
                if (base.m_selection.Length != 0)
                {
                    base.m_selection.EraseText(this);
                }
                base.m_text.Insert(base.CarriagePositionIndex, character);
                int num3 = base.CarriagePositionIndex + 1;
                base.CarriagePositionIndex = num3;
            }
            else
            {
                this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
                this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
                this.AddToUndo(base.m_text.ToString(), true);
                int num = 4 - (this.m_currentCarriageColumn % 4);
                for (int i = 0; i < num; i++)
                {
                    this.InsertCharInternal(' ');
                }
            }
            this.OnTextChanged();
        }

        private void InsertCharInternal(char character)
        {
            base.m_text.Insert(base.CarriagePositionIndex, character);
            int num = base.CarriagePositionIndex + 1;
            base.CarriagePositionIndex = num;
        }

        public void InsertCharMultiple(bool compositionEnd, string chars)
        {
            if (compositionEnd)
            {
                this.AddToUndo(base.m_text.ToString(), true);
            }
            if (base.m_selection.Length != 0)
            {
                base.m_selection.EraseText(this);
            }
            base.m_text.Insert(base.CarriagePositionIndex, chars);
            base.CarriagePositionIndex += chars.Length;
            this.OnTextChanged();
        }

        public void KeypressBackspace(bool compositionEnd)
        {
            if (compositionEnd)
            {
                this.AddToUndo(base.m_text.ToString(), true);
            }
            if (base.m_selection.Length == 0)
            {
                this.ApplyBackspace();
            }
            else
            {
                base.m_selection.EraseText(this);
            }
            this.OnTextChanged();
            this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
            this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
        }

        public void KeypressBackspaceMultiple(bool compositionEnd, int count)
        {
            if (compositionEnd)
            {
                this.AddToUndo(base.m_text.ToString(), true);
            }
            if (base.m_selection.Length == 0)
            {
                this.ApplyBackspaceMultiple(count);
            }
            else
            {
                base.m_selection.EraseText(this);
            }
            this.OnTextChanged();
            this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
            this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
        }

        public void KeypressDelete(bool compositionEnd)
        {
            if (compositionEnd)
            {
                this.AddToUndo(base.m_text.ToString(), true);
            }
            if (base.m_selection.Length == 0)
            {
                this.ApplyDelete();
            }
            else
            {
                base.m_selection.EraseText(this);
            }
            this.OnTextChanged();
            this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
            this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
        }

        public void KeypressEnter(bool compositionEnd)
        {
            if (compositionEnd)
            {
                this.AddToUndo(base.m_text.ToString(), true);
                if (base.m_selection.Length != 0)
                {
                    base.m_selection.EraseText(this);
                }
            }
            this.InsertCharInternal('\n');
            this.m_currentCarriageLine = this.CalculateNewCarriageLine(base.CarriagePositionIndex);
            this.m_currentCarriageColumn = this.GetCarriageColumn(base.CarriagePositionIndex);
            this.OnTextChanged();
        }

        public void KeypressRedo()
        {
            this.Redo();
        }

        public void KeypressUndo()
        {
            this.Undo();
        }

        public int MeasureNumLines(string text)
        {
            int num = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    num++;
                }
            }
            return num;
        }

        internal override void OnFocusChanged(bool focus)
        {
            base.OnFocusChanged(focus);
            if (MyImeProcessor.Instance != null)
            {
                if (focus)
                {
                    MyImeProcessor.Instance.Activate(this);
                }
                else
                {
                    MyImeProcessor.Instance.Deactivate();
                }
            }
        }

        private void OnTextChanged()
        {
            this.BuildLineInformation();
            base.m_selection.Reset(this);
            base.m_label.Clear();
            base.AppendText(base.m_text);
            base.ScrollToShowCarriage();
            base.DelayCaretBlink();
        }

        private void Redo()
        {
            if (this.m_redoCache.Count > 0)
            {
                int currentIndex = this.UpdateCarriage(this.m_redoCache);
                base.CarriagePositionIndex--;
                this.AddToUndo(base.m_text.ToString(), false);
                this.UpdateEditorText(currentIndex, this.m_redoCache);
            }
        }

        private void Undo()
        {
            if (this.m_undoCache.Count > 0)
            {
                int currentIndex = this.UpdateCarriage(this.m_undoCache);
                this.AddToRedo(base.m_text.ToString());
                this.UpdateEditorText(currentIndex, this.m_undoCache);
            }
        }

        private int UpdateCarriage(List<string> array)
        {
            int num = array.Count - 1;
            int firstDiffIndex = this.GetFirstDiffIndex(array[num], base.m_text.ToString());
            if (array[num].Length < base.m_text.Length)
            {
                firstDiffIndex--;
            }
            if (array[num].Length > base.m_text.Length)
            {
                firstDiffIndex++;
            }
            this.CarriagePositionIndex = (firstDiffIndex == -1) ? array[num].Length : firstDiffIndex;
            return num;
        }

        private void UpdateEditorText(int currentIndex, List<string> array)
        {
            base.m_text.Clear();
            base.m_text.Append(array[currentIndex]);
            this.OnTextChanged();
            array.RemoveAt(currentIndex);
        }

        public override StringBuilder Text
        {
            get => 
                base.m_text;
            set
            {
                this.m_lineInformation.Clear();
                base.m_text.Clear();
                if (value != null)
                {
                    base.m_text.AppendStringBuilder(value);
                }
                this.BuildLineInformation();
                base.RefreshText(false);
            }
        }
    }
}

