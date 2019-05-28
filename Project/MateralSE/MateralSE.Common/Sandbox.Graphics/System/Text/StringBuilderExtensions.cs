namespace System.Text
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.CompilerServices;

    public static class StringBuilderExtensions
    {
        [ThreadStatic]
        private static StringBuilder m_tmp;

        private static int AppendWord(StringBuilder from, StringBuilder to, int wordPos)
        {
            int num = wordPos;
            bool flag = false;
            while (num < from.Length)
            {
                int num1;
                char ch = from[num];
                if ((ch == ' ') || (ch == '\r'))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = (int) (ch == '\n');
                }
                bool flag2 = (bool) num1;
                if (!flag2 & flag)
                {
                    break;
                }
                flag |= flag2;
                to.Append(ch);
                num++;
            }
            return num;
        }

        public static void Autowrap(this StringBuilder sb, float width, string font, float textScale)
        {
            int wordPos = 0;
            int num2 = 0;
            if (m_tmp == null)
            {
                m_tmp = new StringBuilder(sb.Length);
            }
            m_tmp.Clear();
            while (true)
            {
                int length = m_tmp.Length;
                int num4 = wordPos;
                wordPos = AppendWord(sb, m_tmp, wordPos);
                if (wordPos == num4)
                {
                    sb.Clear().AppendStringBuilder(m_tmp);
                    return;
                }
                num2++;
                if (MyGuiManager.MeasureString(font, m_tmp, textScale).X > width)
                {
                    if (num2 == 1)
                    {
                        m_tmp.AppendLine();
                        wordPos = MoveSpaces(sb, wordPos);
                        num2 = 0;
                        continue;
                    }
                    m_tmp.Length = length;
                    wordPos = num4;
                    m_tmp.AppendLine();
                    wordPos = MoveSpaces(sb, wordPos);
                    num2 = 0;
                    width = MyGuiManager.MeasureString(font, m_tmp, textScale).X;
                }
            }
        }

        public static bool EqualsStrFast(this string str, StringBuilder sb) => 
            sb.EqualsStrFast(str);

        public static bool EqualsStrFast(this StringBuilder sb, string str)
        {
            if (sb.Length != str.Length)
            {
                return false;
            }
            for (int i = 0; i < str.Length; i++)
            {
                if (sb[i] != str[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static int MoveSpaces(StringBuilder from, int pos)
        {
            while ((pos < from.Length) && (from[pos] == ' '))
            {
                pos++;
            }
            return pos;
        }
    }
}

