namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using Sandbox.Gui.RichTextLabel;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Utils;
    using VRageMath;

    public class MyRichLabel
    {
        private static readonly string[] LINE_SEPARATORS = new string[] { "\n", "\r\n" };
        private const char m_wordSeparator = ' ';
        [CompilerGenerated]
        private ScissorRectangleHandler AdjustingScissorRectangle;
        private MyGuiControlBase m_parent;
        private bool m_sizeDirty;
        private Vector2 m_size;
        private float m_maxLineWidth;
        private float m_minLineHeight;
        private List<MyRichLabelLine> m_lineSeparators;
        private int m_lineSeparatorsCount;
        private int m_lineSeparatorsCapacity;
        private int m_lineSeparatorFirst;
        private MyRichLabelLine m_currentLine;
        private float m_currentLineRestFreeSpace;
        private StringBuilder m_helperSb;
        private MyRichLabelLine m_emptyLine;
        private int m_visibleLinesCount;
        private List<MyRichLabelText> m_richTextsPool;
        private int m_richTextsOffset;
        private int m_richTexsCapacity;
        public MyGuiDrawAlignEnum TextAlign;

        public event ScissorRectangleHandler AdjustingScissorRectangle
        {
            [CompilerGenerated] add
            {
                ScissorRectangleHandler adjustingScissorRectangle = this.AdjustingScissorRectangle;
                while (true)
                {
                    ScissorRectangleHandler a = adjustingScissorRectangle;
                    ScissorRectangleHandler handler3 = (ScissorRectangleHandler) Delegate.Combine(a, value);
                    adjustingScissorRectangle = Interlocked.CompareExchange<ScissorRectangleHandler>(ref this.AdjustingScissorRectangle, handler3, a);
                    if (ReferenceEquals(adjustingScissorRectangle, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                ScissorRectangleHandler adjustingScissorRectangle = this.AdjustingScissorRectangle;
                while (true)
                {
                    ScissorRectangleHandler source = adjustingScissorRectangle;
                    ScissorRectangleHandler handler3 = (ScissorRectangleHandler) Delegate.Remove(source, value);
                    adjustingScissorRectangle = Interlocked.CompareExchange<ScissorRectangleHandler>(ref this.AdjustingScissorRectangle, handler3, source);
                    if (ReferenceEquals(adjustingScissorRectangle, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyRichLabel(MyGuiControlBase parent, float maxLineWidth, float minLineHeight, int? linesCountMax = new int?())
        {
            this.m_parent = parent;
            this.m_maxLineWidth = maxLineWidth;
            this.m_minLineHeight = minLineHeight;
            this.m_helperSb = new StringBuilder(0x100);
            this.m_visibleLinesCount = (linesCountMax == null) ? 0x7fffffff : linesCountMax.Value;
            this.Init();
        }

        public void Append(string texture, Vector2 size, Vector4 color)
        {
            MyRichLabelImage part = new MyRichLabelImage(texture, size, color);
            if (part.Size.X > this.m_currentLineRestFreeSpace)
            {
                this.AppendLine();
            }
            this.AppendPart(part);
        }

        public void Append(string text, string font, float scale, Vector4 color)
        {
            string[] strArray = text.Split(LINE_SEPARATORS, StringSplitOptions.None);
            for (int i = 0; i < strArray.Length; i++)
            {
                this.AppendParagraph(strArray[i], font, scale, color);
                if (i < (strArray.Length - 1))
                {
                    this.AppendLine();
                }
            }
        }

        public void Append(StringBuilder text, string font, float scale, Vector4 color)
        {
            this.Append(text.ToString(), font, scale, color);
        }

        public void AppendLine()
        {
            if (this.m_lineSeparatorsCount == this.m_visibleLinesCount)
            {
                this.m_lineSeparatorFirst = this.GetIndexSafe(1);
                this.m_currentLine = this.m_lineSeparators[this.GetIndexSafe(this.m_lineSeparatorsCount)];
                this.m_currentLine.ClearParts();
            }
            else
            {
                this.ReallocateLines();
                this.m_lineSeparatorsCount++;
                this.m_currentLine = this.m_lineSeparators[this.GetIndexSafe(this.m_lineSeparatorsCount)];
            }
            this.m_currentLineRestFreeSpace = this.m_maxLineWidth;
            this.ReallocateRichTexts();
            this.m_sizeDirty = true;
        }

        public void AppendLine(string texture, Vector2 size, Vector4 color)
        {
            this.Append(texture, size, color);
            this.AppendLine();
        }

        public void AppendLine(StringBuilder text, string font, float scale, Vector4 color)
        {
            this.Append(text, font, scale, color);
            this.AppendLine();
        }

        public void AppendLink(string url, string text, float scale, Action<string> onClick)
        {
            MyRichLabelLink part = new MyRichLabelLink(url, text, scale, onClick);
            this.AppendPart(part);
        }

        private void AppendParagraph(string paragraph, string font, float scale, Vector4 color)
        {
            int num;
            this.m_helperSb.Clear();
            this.m_helperSb.Append(paragraph);
            if (MyGuiManager.MeasureString(font, this.m_helperSb, scale).X < this.m_currentLineRestFreeSpace)
            {
                this.ReallocateRichTexts();
                num = this.m_richTextsOffset + 1;
                this.m_richTextsOffset = num;
                this.m_richTextsPool[num].Init(this.m_helperSb.ToString(), font, scale, color);
                this.AppendPart(this.m_richTextsPool[this.m_richTextsOffset]);
            }
            else
            {
                this.ReallocateRichTexts();
                num = this.m_richTextsOffset + 1;
                this.m_richTextsOffset = num;
                this.m_richTextsPool[num].Init("", font, scale, color);
                char[] separator = new char[] { ' ' };
                string[] strArray = paragraph.Split(separator);
                if (paragraph.StartsWith(" "))
                {
                    strArray[1] = " " + strArray[1];
                }
                if (paragraph.EndsWith(" "))
                {
                    strArray[strArray.Length - 2] = strArray[strArray.Length - 2] + " ";
                }
                int index = 0;
                while (index < strArray.Length)
                {
                    if (strArray[index].Trim().Length == 0)
                    {
                        index++;
                        continue;
                    }
                    this.m_helperSb.Clear();
                    if (this.m_richTextsPool[this.m_richTextsOffset].Text.Length > 0)
                    {
                        this.m_helperSb.Append(' ');
                    }
                    this.m_helperSb.Append(strArray[index]);
                    if (MyGuiManager.MeasureString(font, this.m_helperSb, scale).X <= (this.m_currentLineRestFreeSpace - this.m_richTextsPool[this.m_richTextsOffset].Size.X))
                    {
                        this.m_richTextsPool[this.m_richTextsOffset].Append(this.m_helperSb.ToString());
                        index++;
                    }
                    else
                    {
                        if (((this.m_currentLine == null) || this.m_currentLine.IsEmpty()) && (this.m_richTextsPool[this.m_richTextsOffset].Text.Length == 0))
                        {
                            int length = MyGuiManager.ComputeNumCharsThatFit(font, this.m_helperSb, scale, this.m_currentLineRestFreeSpace);
                            this.m_richTextsPool[this.m_richTextsOffset].Append(strArray[index].Substring(0, length));
                            strArray[index] = strArray[index].Substring(length);
                        }
                        this.AppendPart(this.m_richTextsPool[this.m_richTextsOffset]);
                        this.ReallocateRichTexts();
                        num = this.m_richTextsOffset + 1;
                        this.m_richTextsOffset = num;
                        this.m_richTextsPool[num].Init("", font, scale, color);
                        if (index < strArray.Length)
                        {
                            this.AppendLine();
                        }
                    }
                }
                if (this.m_richTextsPool[this.m_richTextsOffset].Text.Length > 0)
                {
                    this.AppendPart(this.m_richTextsPool[this.m_richTextsOffset]);
                }
            }
        }

        private void AppendPart(MyRichLabelPart part)
        {
            this.m_currentLine = this.m_lineSeparators[this.GetIndexSafe(this.m_lineSeparatorsCount)];
            this.m_currentLine.AddPart(part);
            this.m_currentLineRestFreeSpace = this.m_maxLineWidth - this.m_currentLine.Size.X;
            this.m_sizeDirty = true;
        }

        public void Clear()
        {
            this.m_lineSeparators.Clear();
            this.Init();
        }

        public unsafe bool Draw(Vector2 position, float offsetY, float offsetX, Vector2 drawSizeMax, float alphamask)
        {
            RectangleF rectangle = new RectangleF(position, drawSizeMax);
            this.OnAdjustingScissorRectangle(ref rectangle);
            Vector2 size = this.Size;
            int charactersLeft = (this.CharactersDisplayed == -1) ? 0x7fffffff : this.CharactersDisplayed;
            using (MyGuiManager.UsingScissorRectangle(ref rectangle))
            {
                Vector2 zero = Vector2.Zero;
                float num2 = 0f;
                switch (this.TextAlign)
                {
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                        zero.X = (0.5f * drawSizeMax.X) - (0.5f * size.X);
                        num2 = 0.5f;
                        break;

                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                        zero.X = drawSizeMax.X - size.X;
                        num2 = 1f;
                        break;

                    default:
                        break;
                }
                switch (this.TextAlign)
                {
                    case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                        zero.Y = (0.5f * drawSizeMax.Y) - (0.5f * size.Y);
                        break;

                    case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                        zero.Y = drawSizeMax.Y - size.Y;
                        break;

                    default:
                        break;
                }
                float* singlePtr1 = (float*) ref zero.Y;
                singlePtr1[0] -= offsetY;
                float* singlePtr2 = (float*) ref zero.X;
                singlePtr2[0] -= offsetX;
                for (int i = 0; i <= this.m_lineSeparatorsCount; i++)
                {
                    MyRichLabelLine line = this.m_lineSeparators[this.GetIndexSafe(i)];
                    Vector2 vector3 = line.Size;
                    Vector2 vector4 = position + zero;
                    float* singlePtr3 = (float*) ref vector4.X;
                    singlePtr3[0] += num2 * (size.X - vector3.X);
                    if (charactersLeft > 0)
                    {
                        line.Draw(vector4, alphamask, ref charactersLeft);
                    }
                    float* singlePtr4 = (float*) ref zero.Y;
                    singlePtr4[0] += vector3.Y;
                }
            }
            if (charactersLeft > 0)
            {
                this.CharactersDisplayed = -1;
            }
            return true;
        }

        private int GetIndexSafe(int index) => 
            ((index + this.m_lineSeparatorFirst) % (this.m_visibleLinesCount + 1));

        internal unsafe bool HandleInput(Vector2 position, float offsetV, float offsetH)
        {
            float* singlePtr1 = (float*) ref position.X;
            singlePtr1[0] -= offsetH;
            float* singlePtr2 = (float*) ref position.Y;
            singlePtr2[0] -= offsetV;
            for (int i = 0; i <= this.m_lineSeparatorsCount; i++)
            {
                int indexSafe = this.GetIndexSafe(i);
                if ((indexSafe < 0) || (indexSafe >= this.m_lineSeparators.Count))
                {
                    return false;
                }
                MyRichLabelLine line = this.m_lineSeparators[indexSafe];
                if (line.HandleInput(position))
                {
                    return true;
                }
                float* singlePtr3 = (float*) ref position.Y;
                singlePtr3[0] += line.Size.Y;
            }
            return false;
        }

        private void Init()
        {
            this.m_helperSb.Clear();
            this.m_sizeDirty = true;
            this.m_size = Vector2.Zero;
            this.m_lineSeparatorsCount = 0;
            this.m_lineSeparatorsCapacity = 0x20;
            this.m_richTextsOffset = 0;
            this.m_richTexsCapacity = 0x20;
            this.m_currentLineRestFreeSpace = this.m_maxLineWidth;
            this.m_lineSeparatorFirst = 0;
            this.m_lineSeparators = new List<MyRichLabelLine>(this.m_lineSeparatorsCapacity);
            for (int i = 0; i < this.m_lineSeparatorsCapacity; i++)
            {
                this.m_lineSeparators.Add(new MyRichLabelLine(this.m_minLineHeight));
            }
            this.m_currentLine = this.m_lineSeparators[0];
            this.m_richTextsPool = new List<MyRichLabelText>(this.m_richTexsCapacity);
            for (int j = 0; j < this.m_richTexsCapacity; j++)
            {
                MyRichLabelText item = new MyRichLabelText();
                item.ShowTextShadow = this.ShowTextShadow;
                item.Tag = this.m_parent.Name;
                this.m_richTextsPool.Add(item);
            }
        }

        private void OnAdjustingScissorRectangle(ref RectangleF rectangle)
        {
            ScissorRectangleHandler adjustingScissorRectangle = this.AdjustingScissorRectangle;
            if (adjustingScissorRectangle != null)
            {
                adjustingScissorRectangle(ref rectangle);
            }
        }

        private void ReallocateLines()
        {
            if ((this.m_lineSeparatorsCount + 1) >= this.m_lineSeparatorsCapacity)
            {
                this.m_lineSeparatorsCapacity *= 2;
                this.m_lineSeparators.Capacity = this.m_lineSeparatorsCapacity;
                for (int i = this.m_lineSeparatorsCount + 1; i < this.m_lineSeparatorsCapacity; i++)
                {
                    this.m_lineSeparators.Add(new MyRichLabelLine(this.m_minLineHeight));
                }
            }
        }

        private void ReallocateRichTexts()
        {
            if ((this.m_richTextsOffset + 1) >= this.m_richTexsCapacity)
            {
                this.m_richTexsCapacity *= 2;
                this.m_richTextsPool.Capacity = this.m_richTexsCapacity;
                for (int i = this.m_richTextsOffset + 1; i < this.m_richTexsCapacity; i++)
                {
                    MyRichLabelText item = new MyRichLabelText();
                    item.ShowTextShadow = this.ShowTextShadow;
                    item.Tag = this.m_parent.Name;
                    this.m_richTextsPool.Add(item);
                }
            }
        }

        public int NumberOfRows =>
            (this.m_lineSeparatorsCount + 1);

        public Vector2 Size
        {
            get
            {
                if (!this.m_sizeDirty)
                {
                    return this.m_size;
                }
                this.m_size = Vector2.Zero;
                int index = 0;
                while (true)
                {
                    if (index <= this.m_lineSeparatorsCount)
                    {
                        int indexSafe = this.GetIndexSafe(index);
                        if (indexSafe < this.m_lineSeparators.Count)
                        {
                            Vector2 size = this.m_lineSeparators[indexSafe].Size;
                            float* singlePtr1 = (float*) ref this.m_size.Y;
                            singlePtr1[0] += size.Y;
                            this.m_size.X = MathHelper.Max(this.m_size.X, size.X);
                            index++;
                            continue;
                        }
                    }
                    this.m_sizeDirty = false;
                    return this.m_size;
                }
            }
        }

        public float MaxLineWidth
        {
            get => 
                this.m_maxLineWidth;
            set => 
                (this.m_maxLineWidth = value);
        }

        public bool ShowTextShadow { get; set; }

        public int CharactersDisplayed { get; set; }
    }
}

