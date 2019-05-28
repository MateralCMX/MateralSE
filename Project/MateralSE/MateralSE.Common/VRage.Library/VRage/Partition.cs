namespace VRage
{
    using System;
    using System.Runtime.CompilerServices;

    public static class Partition
    {
        private static readonly string[] m_letters = (from s in Enumerable.Range(0x41, 0x1a) select new string((char) s, 1)).ToArray<string>();

        public static T Select<T>(int num, T a, T b) => 
            (((num % 2) == 0) ? a : b);

        public static T Select<T>(int num, T a, T b, T c)
        {
            uint num2 = (uint) (num % 3);
            return ((num2 == 0) ? a : ((num2 == 1) ? b : c));
        }

        public static T Select<T>(int num, T a, T b, T c, T d)
        {
            uint num2 = (uint) (num % 4);
            return ((num2 == 0) ? a : ((num2 == 1) ? b : ((num2 == 2) ? c : d)));
        }

        public static T Select<T>(int num, T a, T b, T c, T d, T e)
        {
            uint num2 = (uint) (num % 5);
            return ((num2 == 0) ? a : ((num2 == 1) ? b : ((num2 == 2) ? c : ((num2 == 3) ? d : e))));
        }

        public static T Select<T>(int num, T a, T b, T c, T d, T e, T f)
        {
            switch ((num % 6))
            {
                case 0:
                    return a;

                case 1:
                    return b;

                case 2:
                    return c;

                case 3:
                    return d;

                case 4:
                    return e;
            }
            return f;
        }

        public static T Select<T>(int num, T a, T b, T c, T d, T e, T f, T g)
        {
            switch ((num % 7))
            {
                case 0:
                    return a;

                case 1:
                    return b;

                case 2:
                    return c;

                case 3:
                    return d;

                case 4:
                    return e;

                case 5:
                    return f;
            }
            return g;
        }

        public static T Select<T>(int num, T a, T b, T c, T d, T e, T f, T g, T h)
        {
            switch ((num % 8))
            {
                case 0:
                    return a;

                case 1:
                    return b;

                case 2:
                    return c;

                case 3:
                    return d;

                case 4:
                    return e;

                case 5:
                    return f;

                case 6:
                    return g;
            }
            return h;
        }

        public static T Select<T>(int num, T a, T b, T c, T d, T e, T f, T g, T h, T i)
        {
            switch ((num % 9))
            {
                case 0:
                    return a;

                case 1:
                    return b;

                case 2:
                    return c;

                case 3:
                    return d;

                case 4:
                    return e;

                case 5:
                    return f;

                case 6:
                    return g;

                case 7:
                    return h;
            }
            return i;
        }

        public static string SelectStringByLetter(char c)
        {
            if (((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')))
            {
                char ch1 = char.ToUpperInvariant(c);
                c = ch1;
                return m_letters[c - 'A'];
            }
            if ((c < '0') || (c > '9'))
            {
                return "Non-letter";
            }
            return "0-9";
        }

        public static string SelectStringGroupOfTenByLetter(char c)
        {
            char ch1 = char.ToUpperInvariant(c);
            c = ch1;
            if ((c >= '0') && (c <= '9'))
            {
                return "0-9";
            }
            if (((c == 'A') || (c == 'B')) || (c == 'C'))
            {
                return "A-C";
            }
            if (((c == 'D') || (c == 'E')) || (c == 'F'))
            {
                return "D-F";
            }
            if (((c == 'G') || (c == 'H')) || (c == 'I'))
            {
                return "G-I";
            }
            if (((c == 'J') || (c == 'K')) || (c == 'L'))
            {
                return "J-L";
            }
            if (((c == 'M') || (c == 'N')) || (c == 'O'))
            {
                return "M-O";
            }
            if (((c == 'P') || (c == 'Q')) || (c == 'R'))
            {
                return "P-R";
            }
            if (((c == 'S') || ((c == 'T') || (c == 'U'))) || (c == 'V'))
            {
                return "S-V";
            }
            if (((c == 'W') || ((c == 'X') || (c == 'Y'))) || (c == 'Z'))
            {
                return "W-Z";
            }
            return "Non-letter";
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Partition.<>c <>9 = new Partition.<>c();

            internal string <.cctor>b__11_0(int s) => 
                new string((char) s, 1);
        }
    }
}

