namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyFixedPoint : IXmlSerializable
    {
        private const int Places = 6;
        private const int Divider = 0xf4240;
        private static readonly string FormatSpecifier;
        private static readonly char[] TrimChars;
        public static readonly MyFixedPoint MinValue;
        public static readonly MyFixedPoint MaxValue;
        public static readonly MyFixedPoint Zero;
        public static readonly MyFixedPoint SmallestPossibleValue;
        public static readonly MyFixedPoint MaxIntValue;
        public static readonly MyFixedPoint MinIntValue;
        [ProtoMember(0x1f)]
        public long RawValue;
        private MyFixedPoint(long rawValue)
        {
            this.RawValue = rawValue;
        }

        public string SerializeString()
        {
            string str = this.RawValue.ToString(FormatSpecifier);
            string str2 = str.Substring(0, str.Length - 6);
            string str3 = str.Substring(str.Length - 6).TrimEnd(TrimChars);
            return ((str3.Length <= 0) ? str2 : (str2 + "." + str3));
        }

        public static MyFixedPoint DeserializeStringSafe(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if ((((ch < '0') || (ch > '9')) && (ch != '.')) && ((ch != '-') || (i != 0)))
                {
                    return (MyFixedPoint) double.Parse(text);
                }
            }
            try
            {
                return DeserializeString(text);
            }
            catch
            {
                return (MyFixedPoint) double.Parse(text);
            }
        }

        public static MyFixedPoint DeserializeString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new MyFixedPoint();
            }
            int index = text.IndexOf('.');
            if (index == -1)
            {
                return new MyFixedPoint(long.Parse(text) * 0xf4240L);
            }
            text = text.Replace(".", "");
            text = text.PadRight((index + 1) + 6, '0');
            text = text.Substring(0, index + 6);
            return new MyFixedPoint(long.Parse(text));
        }

        public static explicit operator MyFixedPoint(float d) => 
            ((((d * 1000000f) + 0.5f) < 9.223372E+18f) ? ((((d * 1000000f) + 0.5f) > -9.223372E+18f) ? new MyFixedPoint((long) ((d * 1000000f) + 0.5f)) : MinValue) : MaxValue);

        public static explicit operator MyFixedPoint(double d) => 
            ((((d * 1000000.0) + 0.5) < 9.2233720368547758E+18) ? ((((d * 1000000.0) + 0.5) > -9.2233720368547758E+18) ? new MyFixedPoint((long) ((d * 1000000.0) + 0.5)) : MinValue) : MaxValue);

        public static explicit operator MyFixedPoint(decimal d) => 
            new MyFixedPoint((long) ((d * 1000000M) + 0.5M));

        public static implicit operator MyFixedPoint(int i) => 
            new MyFixedPoint(i * 0xf4240L);

        public static explicit operator decimal(MyFixedPoint fp) => 
            (fp.RawValue / 1000000M);

        public static explicit operator float(MyFixedPoint fp) => 
            (((float) fp.RawValue) / 1000000f);

        public static explicit operator double(MyFixedPoint fp) => 
            (((double) fp.RawValue) / 1000000.0);

        public static explicit operator int(MyFixedPoint fp) => 
            ((int) (fp.RawValue / 0xf4240L));

        public static bool IsIntegral(MyFixedPoint fp) => 
            ((fp.RawValue % 0xf4240L) == 0L);

        public static MyFixedPoint Ceiling(MyFixedPoint a)
        {
            a.RawValue = (((a.RawValue + 0xf4240L) - 1L) / 0xf4240L) * 0xf4240L;
            return a;
        }

        public static MyFixedPoint Floor(MyFixedPoint a)
        {
            a.RawValue = (a.RawValue / 0xf4240L) * 0xf4240L;
            return a;
        }

        public static MyFixedPoint Min(MyFixedPoint a, MyFixedPoint b) => 
            ((a < b) ? a : b);

        public static MyFixedPoint Max(MyFixedPoint a, MyFixedPoint b) => 
            ((a > b) ? a : b);

        public static MyFixedPoint Round(MyFixedPoint a)
        {
            a.RawValue = (a.RawValue + 0x7a120L) / 0xf4240L;
            return a;
        }

        public static MyFixedPoint operator -(MyFixedPoint a) => 
            new MyFixedPoint(-a.RawValue);

        public static bool operator <(MyFixedPoint a, MyFixedPoint b) => 
            (a.RawValue < b.RawValue);

        public static bool operator >(MyFixedPoint a, MyFixedPoint b) => 
            (a.RawValue > b.RawValue);

        public static bool operator <=(MyFixedPoint a, MyFixedPoint b) => 
            (a.RawValue <= b.RawValue);

        public static bool operator >=(MyFixedPoint a, MyFixedPoint b) => 
            (a.RawValue >= b.RawValue);

        public static bool operator ==(MyFixedPoint a, MyFixedPoint b) => 
            (a.RawValue == b.RawValue);

        public static bool operator !=(MyFixedPoint a, MyFixedPoint b) => 
            (a.RawValue != b.RawValue);

        public static unsafe MyFixedPoint operator +(MyFixedPoint a, MyFixedPoint b)
        {
            long* numPtr1 = (long*) ref a.RawValue;
            numPtr1[0] += b.RawValue;
            return a;
        }

        public static unsafe MyFixedPoint operator -(MyFixedPoint a, MyFixedPoint b)
        {
            long* numPtr1 = (long*) ref a.RawValue;
            numPtr1[0] -= b.RawValue;
            return a;
        }

        public static MyFixedPoint operator *(MyFixedPoint a, MyFixedPoint b)
        {
            long num = a.RawValue / 0xf4240L;
            long num2 = b.RawValue / 0xf4240L;
            long num3 = a.RawValue % 0xf4240L;
            long num4 = b.RawValue % 0xf4240L;
            return new MyFixedPoint(((((num * num2) * 0xf4240L) + ((num3 * num4) / 0xf4240L)) + (num * num4)) + (num2 * num3));
        }

        public static MyFixedPoint operator *(MyFixedPoint a, float b) => 
            (a * ((MyFixedPoint) b));

        public static MyFixedPoint operator *(float a, MyFixedPoint b) => 
            (((MyFixedPoint) a) * b);

        public static MyFixedPoint operator *(MyFixedPoint a, int b) => 
            (a * b);

        public static MyFixedPoint operator *(int a, MyFixedPoint b) => 
            (a * b);

        public static MyFixedPoint AddSafe(MyFixedPoint a, MyFixedPoint b) => 
            new MyFixedPoint(AddSafeInternal(a.RawValue, b.RawValue));

        public static MyFixedPoint MultiplySafe(MyFixedPoint a, float b) => 
            MultiplySafe(a, (MyFixedPoint) b);

        public static MyFixedPoint MultiplySafe(MyFixedPoint a, int b) => 
            MultiplySafe(a, (MyFixedPoint) b);

        public static MyFixedPoint MultiplySafe(float a, MyFixedPoint b) => 
            MultiplySafe((MyFixedPoint) a, b);

        public static MyFixedPoint MultiplySafe(int a, MyFixedPoint b) => 
            MultiplySafe((MyFixedPoint) a, b);

        public static MyFixedPoint MultiplySafe(MyFixedPoint a, MyFixedPoint b)
        {
            long num = a.RawValue / 0xf4240L;
            long num2 = b.RawValue / 0xf4240L;
            long num3 = a.RawValue % 0xf4240L;
            long num4 = b.RawValue % 0xf4240L;
            long num7 = MultiplySafeInternal(num, num4);
            long num8 = MultiplySafeInternal(num2, num3);
            return new MyFixedPoint(AddSafeInternal(AddSafeInternal(AddSafeInternal((num3 * num4) / 0xf4240L, MultiplySafeInternal(num, num2 * 0xf4240L)), num7), num8));
        }

        private static long MultiplySafeInternal(long a, long b)
        {
            long num = a * b;
            if ((b == 0) || ((num / b) == a))
            {
                return num;
            }
            return (((Math.Sign(a) * Math.Sign(b)) == 1) ? 0x7fffffffffffffffL : -9223372036854775808L);
        }

        private static long AddSafeInternal(long a, long b)
        {
            int num = Math.Sign(a);
            if ((num * Math.Sign(b)) != 1)
            {
                return (a + b);
            }
            long num2 = a + b;
            return ((Math.Sign(num2) != num) ? ((num < 0) ? -9223372036854775808L : 0x7fffffffffffffffL) : num2);
        }

        public int ToIntSafe() => 
            ((this.RawValue <= MaxIntValue.RawValue) ? ((this.RawValue >= MinIntValue.RawValue) ? ((int) this) : ((int) MinIntValue)) : ((int) MaxIntValue));

        public override string ToString() => 
            this.SerializeString();

        public override int GetHashCode() => 
            ((int) this.RawValue);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            MyFixedPoint? nullable = obj as MyFixedPoint?;
            return ((nullable != null) && (this == nullable.Value));
        }

        XmlSchema IXmlSerializable.GetSchema() => 
            null;

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            string text = reader.ReadInnerXml();
            this.RawValue = DeserializeStringSafe(text).RawValue;
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteString(this.SerializeString());
        }

        static MyFixedPoint()
        {
            FormatSpecifier = "D" + 7;
            TrimChars = new char[] { '0' };
            MinValue = new MyFixedPoint(-9223372036854775808L);
            MaxValue = new MyFixedPoint(0x7fffffffffffffffL);
            Zero = new MyFixedPoint(0L);
            SmallestPossibleValue = new MyFixedPoint(1L);
            MaxIntValue = 0x7fffffff;
            MinIntValue = -2147483648;
        }
    }
}

