namespace VRage.Library.Utils
{
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Compiler;

    public static class MyEnum<T> where T: struct, IComparable, IFormattable, IConvertible
    {
        public static readonly T[] Values;
        public static readonly Type UnderlyingType;
        private static readonly Dictionary<int, string> m_names;

        static MyEnum()
        {
            MyEnum<T>.Values = (T[]) Enum.GetValues(typeof(T));
            MyEnum<T>.UnderlyingType = typeof(T).UnderlyingSystemType;
            MyEnum<T>.m_names = new Dictionary<int, string>();
        }

        public static string GetName(T value)
        {
            string str;
            int index = Array.IndexOf<T>(MyEnum<T>.Values, value);
            if (!MyEnum<T>.m_names.TryGetValue(index, out str))
            {
                str = value.ToString();
                MyEnum<T>.m_names[index] = str;
            }
            return str;
        }

        public static unsafe ulong GetValue(T value)
        {
            ulong num = 0UL;
            Utilities.Write<T>((IntPtr) &num, ref value);
            return num;
        }

        public static unsafe T SetValue(ulong value)
        {
            T data = default(T);
            Utilities.Read<T>((IntPtr) &value, ref data);
            return data;
        }

        public static unsafe void SetValue(ref T loc, ulong value)
        {
            Utilities.Read<T>((IntPtr) &value, ref loc);
        }

        public static string Name =>
            TypeNameHelper<T>.Name;

        [StructLayout(LayoutKind.Sequential, Size=1)]
        public struct Range
        {
            public static readonly T Min;
            public static readonly T Max;
            static Range()
            {
                T[] values = MyEnum<T>.Values;
                Comparer<T> comparer = Comparer<T>.Default;
                if (values.Length != 0)
                {
                    MyEnum<T>.Range.Max = values[0];
                    MyEnum<T>.Range.Min = values[0];
                    for (int i = 1; i < values.Length; i++)
                    {
                        T y = values[i];
                        if (comparer.Compare(MyEnum<T>.Range.Max, y) < 0)
                        {
                            MyEnum<T>.Range.Max = y;
                        }
                        if (comparer.Compare(MyEnum<T>.Range.Min, y) > 0)
                        {
                            MyEnum<T>.Range.Min = y;
                        }
                    }
                }
            }
        }
    }
}

