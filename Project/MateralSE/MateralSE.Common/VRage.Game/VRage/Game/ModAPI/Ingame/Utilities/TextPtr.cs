namespace VRage.Game.ModAPI.Ingame.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{ToDebugString(),nq}")]
    public struct TextPtr
    {
        public readonly string Content;
        public readonly int Index;
        public static implicit operator int(TextPtr ptr) => 
            ptr.Index;

        public static implicit operator string(TextPtr ptr) => 
            ptr.Content;

        public static TextPtr operator +(TextPtr ptr, int offset) => 
            new TextPtr(ptr.Content, ptr.Index + offset);

        public static TextPtr operator ++(TextPtr ptr) => 
            new TextPtr(ptr.Content, ptr.Index + 1);

        public static TextPtr operator -(TextPtr ptr, int offset) => 
            new TextPtr(ptr.Content, ptr.Index - offset);

        public static TextPtr operator --(TextPtr ptr) => 
            new TextPtr(ptr.Content, ptr.Index - 1);

        public TextPtr(string content)
        {
            this.Content = content;
            this.Index = 0;
        }

        public TextPtr(string content, int index)
        {
            this.Content = content;
            this.Index = index;
        }

        public bool IsOutOfBounds() => 
            ((this.Index < 0) || (this.Index >= this.Content.Length));

        public char Char =>
            (this.IsOutOfBounds() ? '\0' : this.Content[this.Index]);
        public bool IsEmpty =>
            (this.Content != null);
        public int FindLineNo()
        {
            string content = this.Content;
            int index = this.Index;
            int num2 = 1;
            for (int i = 0; i < index; i++)
            {
                if (content[i] == '\n')
                {
                    num2++;
                }
            }
            return num2;
        }

        public TextPtr Find(string str)
        {
            if (this.IsOutOfBounds())
            {
                return this;
            }
            int index = this.Content.IndexOf(str, this.Index, StringComparison.InvariantCulture);
            return ((index != -1) ? new TextPtr(this.Content, index) : new TextPtr(this.Content, this.Content.Length));
        }

        public TextPtr Find(char ch)
        {
            if (this.IsOutOfBounds())
            {
                return this;
            }
            int index = this.Content.IndexOf(ch, this.Index);
            return ((index != -1) ? new TextPtr(this.Content, index) : new TextPtr(this.Content, this.Content.Length));
        }

        public TextPtr FindInLine(char ch)
        {
            if (this.IsOutOfBounds())
            {
                return this;
            }
            string content = this.Content;
            int length = this.Content.Length;
            int index = this.Index;
            while (true)
            {
                if (index < length)
                {
                    char ch2 = content[index];
                    if (ch2 == ch)
                    {
                        return new TextPtr(content, index);
                    }
                    if ((ch2 != '\r') && (ch2 != '\n'))
                    {
                        index++;
                        continue;
                    }
                }
                return new TextPtr(this.Content, this.Content.Length);
            }
        }

        public TextPtr FindEndOfLine(bool skipNewline = false)
        {
            int length = this.Content.Length;
            if (this.Index >= length)
            {
                return this;
            }
            TextPtr ptr = this;
            while (true)
            {
                if (ptr.Index < length)
                {
                    if (!ptr.IsNewLine())
                    {
                        ptr += 1;
                        continue;
                    }
                    if (skipNewline)
                    {
                        if (ptr.Char == '\r')
                        {
                            ptr += 1;
                        }
                        ptr += 1;
                    }
                }
                return ptr;
            }
        }

        public bool StartsWithCaseInsensitive(string what)
        {
            TextPtr ptr = this;
            foreach (char ch in what)
            {
                if (char.ToUpper(ptr.Char) != char.ToUpper(ch))
                {
                    return false;
                }
                ptr += 1;
            }
            return true;
        }

        public bool StartsWith(string what)
        {
            TextPtr ptr = this;
            foreach (char ch in what)
            {
                if (ptr.Char != ch)
                {
                    return false;
                }
                ptr += 1;
            }
            return true;
        }

        public TextPtr SkipWhitespace(bool skipNewline = false)
        {
            TextPtr ptr = this;
            int length = ptr.Content.Length;
            if (skipNewline)
            {
                while (true)
                {
                    char c = ptr.Char;
                    if ((ptr.Index >= length) || !char.IsWhiteSpace(c))
                    {
                        return ptr;
                    }
                    ptr += 1;
                }
            }
            while (true)
            {
                char c = ptr.Char;
                if (((ptr.Index >= length) || ptr.IsNewLine()) || !char.IsWhiteSpace(c))
                {
                    return ptr;
                }
                ptr += 1;
            }
        }

        public bool IsEndOfLine() => 
            ((this.Index >= this.Content.Length) || this.IsNewLine());

        public bool IsStartOfLine()
        {
            if (this.Index > 0)
            {
                return (this - 1).IsNewLine();
            }
            return true;
        }

        public bool IsNewLine()
        {
            char ch = this.Char;
            return ((ch == '\n') || ((ch == '\r') && ((this.Index < (this.Content.Length - 1)) && (this.Content[this.Index + 1] == '\n'))));
        }

        public TextPtr TrimStart()
        {
            string content = this.Content;
            int index = this.Index;
            int length = content.Length;
            while (true)
            {
                if (index < length)
                {
                    char ch = content[index];
                    if ((ch == ' ') || (ch == '\t'))
                    {
                        index++;
                        continue;
                    }
                }
                return new TextPtr(content, index);
            }
        }

        public TextPtr TrimEnd()
        {
            string content = this.Content;
            int num = this.Index - 1;
            while (true)
            {
                if (num >= 0)
                {
                    char ch = content[num];
                    if ((ch == ' ') || (ch == '\t'))
                    {
                        num--;
                        continue;
                    }
                }
                return new TextPtr(content, num + 1);
            }
        }

        private string ToDebugString()
        {
            if (this.Index < 0)
            {
                return "<before>";
            }
            if (this.Index >= this.Content.Length)
            {
                return "<after>";
            }
            int num = this.Index + 0x25;
            string input = (num <= this.Content.Length) ? (this.Content.Substring(this.Index, num - this.Index) + "...") : this.Content.Substring(this.Index, this.Content.Length - this.Index);
            return Regex.Replace(input, @"[\r\t\n]", delegate (Match match) {
                string str = match.Value;
                return (str == "\r") ? @"\r" : ((str == "\t") ? @"\t" : ((str == "\n") ? @"\n" : match.Value));
            });
        }
        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly TextPtr.<>c <>9 = new TextPtr.<>c();
            public static MatchEvaluator <>9__28_0;

            internal string <ToDebugString>b__28_0(Match match)
            {
                string str = match.Value;
                return ((str == "\r") ? @"\r" : ((str == "\t") ? @"\t" : ((str == "\n") ? @"\n" : match.Value)));
            }
        }
    }
}

