namespace System
{
    using System.Runtime.CompilerServices;

    public static class StringExtensions
    {
        public static bool Contains(this string text, string testSequence, StringComparison comparison) => 
            (text.IndexOf(testSequence, comparison) != -1);

        public static unsafe bool Equals(this string text, char* compareTo, int length)
        {
            int index = Math.Min(length, text.Length);
            for (int i = 0; i < index; i++)
            {
                if (text[i] != compareTo[i])
                {
                    return false;
                }
            }
            return ((length <= index) || (compareTo[index] == '\0'));
        }

        public static unsafe long GetHashCode64(this string self)
        {
            char* chPtr = (char*) self;
            if (chPtr != null)
            {
                chPtr += RuntimeHelpers.OffsetToStringData;
            }
            int length = self.Length;
            long* numPtr = (long*) chPtr;
            long num2 = 0x18a22d2e039L;
            ushort* numPtr2 = (ushort*) chPtr;
            while (length >= 4)
            {
                num2 = (((num2 << 5) + num2) + (num2 >> 0x3b)) ^ numPtr[0];
                numPtr++;
                numPtr2 += 4;
                length -= 4;
            }
            if (length > 0)
            {
                long num3 = 0L;
                while (true)
                {
                    if (length <= 0)
                    {
                        num2 = (((num2 << 5) + num2) + (num2 >> 0x3b)) ^ num3;
                        break;
                    }
                    numPtr2++;
                    num3 = (num3 << 0x10) | numPtr2[0];
                    length--;
                }
            }
            return num2;
        }
    }
}

