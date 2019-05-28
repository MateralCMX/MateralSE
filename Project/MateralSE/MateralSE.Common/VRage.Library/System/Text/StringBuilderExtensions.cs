namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public static class StringBuilderExtensions
    {
        private static readonly char[] ms_digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static readonly uint ms_default_decimal_places = 5;
        private static readonly char ms_default_pad_char = '0';

        public static StringBuilder Concat(this StringBuilder string_builder, double double_val)
        {
            string_builder.Concat(double_val, ms_default_decimal_places, 0, ms_default_pad_char, false);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, int int_val)
        {
            string_builder.Concat(int_val, 0, ms_default_pad_char, 10, true);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, float float_val)
        {
            string_builder.Concat(float_val, ms_default_decimal_places, 0, ms_default_pad_char, false);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val)
        {
            string_builder.Concat((long) uint_val, 0, ms_default_pad_char, 10, true);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, double double_val, uint decimal_places)
        {
            string_builder.Concat(double_val, decimal_places, 0, ms_default_pad_char, false);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount)
        {
            string_builder.Concat(int_val, pad_amount, ms_default_pad_char, 10, true);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places)
        {
            string_builder.Concat(float_val, decimal_places, 0, ms_default_pad_char, false);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount)
        {
            string_builder.Concat((long) uint_val, pad_amount, ms_default_pad_char, 10, true);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, double double_val, uint decimal_places, uint pad_amount)
        {
            string_builder.Concat(double_val, decimal_places, pad_amount, ms_default_pad_char, false);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char)
        {
            string_builder.Concat(int_val, pad_amount, pad_char, 10, true);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount)
        {
            string_builder.Concat(float_val, decimal_places, pad_amount, ms_default_pad_char, false);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, uint uint_val, uint pad_amount, char pad_char)
        {
            string_builder.Concat((long) uint_val, pad_amount, pad_char, 10, true);
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, double double_val, uint decimal_places, uint pad_amount, char pad_char, bool thousandSeparator)
        {
            if (decimal_places == 0)
            {
                long num = (double_val < 0.0) ? ((long) (double_val - 0.5)) : ((long) (double_val + 0.5));
                string_builder.Concat(num, pad_amount, pad_char, 10, thousandSeparator);
            }
            else
            {
                double num2 = 0.5;
                int num6 = 0;
                while (true)
                {
                    if (num6 >= decimal_places)
                    {
                        double num1 = double_val;
                        double_val = num1 + ((double_val >= 0.0) ? num2 : -num2);
                        long num3 = (long) double_val;
                        if ((num3 == 0) && (double_val < 0.0))
                        {
                            string_builder.Append('-');
                        }
                        string_builder.Concat(num3, pad_amount, pad_char, 10, thousandSeparator);
                        string_builder.Append('.');
                        double num4 = Math.Abs((double) (double_val - num3));
                        uint num5 = decimal_places;
                        while (true)
                        {
                            num4 *= 10.0;
                            num5--;
                            if (num5 <= 0)
                            {
                                string_builder.Concat((long) ((uint) num4), decimal_places, '0', 10, thousandSeparator);
                                break;
                            }
                        }
                        break;
                    }
                    num2 *= 0.10000000149011612;
                    num6++;
                }
            }
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, int int_val, uint pad_amount, char pad_char, uint base_val, bool thousandSeparation)
        {
            if (int_val >= 0)
            {
                string_builder.Concat((long) ((ulong) int_val), pad_amount, pad_char, base_val, thousandSeparation);
            }
            else
            {
                string_builder.Append('-');
                uint num = (uint) ((-1 - int_val) + 1);
                string_builder.Concat((long) num, pad_amount, pad_char, base_val, thousandSeparation);
            }
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, long long_val, uint pad_amount, char pad_char, uint base_val, bool thousandSeparation)
        {
            if (long_val >= 0L)
            {
                string_builder.Concat((ulong) long_val, pad_amount, pad_char, base_val, thousandSeparation);
            }
            else
            {
                string_builder.Append('-');
                ulong num = (ulong) ((-1L - long_val) + 1L);
                string_builder.Concat(num, pad_amount, pad_char, base_val, thousandSeparation);
            }
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, float float_val, uint decimal_places, uint pad_amount, char pad_char, bool thousandSeparator)
        {
            if (decimal_places == 0)
            {
                long num = (float_val < 0f) ? ((long) (float_val - 0.5f)) : ((long) (float_val + 0.5f));
                string_builder.Concat(num, pad_amount, pad_char, 10, thousandSeparator);
            }
            else
            {
                float num2 = 0.5f;
                int num6 = 0;
                while (true)
                {
                    if (num6 >= decimal_places)
                    {
                        float single1 = float_val;
                        float_val = single1 + ((float_val >= 0f) ? num2 : -num2);
                        long num3 = (long) float_val;
                        if ((num3 == 0) && (float_val < 0f))
                        {
                            string_builder.Append('-');
                        }
                        string_builder.Concat(num3, pad_amount, pad_char, 10, thousandSeparator);
                        string_builder.Append('.');
                        float num4 = Math.Abs((float) (float_val - num3));
                        uint num5 = decimal_places;
                        while (true)
                        {
                            num4 *= 10f;
                            num5--;
                            if (num5 <= 0)
                            {
                                string_builder.Concat((long) ((uint) num4), decimal_places, '0', 10, thousandSeparator);
                                break;
                            }
                        }
                        break;
                    }
                    num2 *= 0.1f;
                    num6++;
                }
            }
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, ulong uint_val, uint pad_amount, char pad_char, uint base_val, bool thousandSeparation)
        {
            uint num = 0;
            ulong num2 = uint_val;
            int num3 = 0;
            while (true)
            {
                num3++;
                if (thousandSeparation && ((num3 % 4) == 0))
                {
                    num++;
                }
                else
                {
                    num2 /= (ulong) base_val;
                    num++;
                }
                if (num2 <= 0L)
                {
                    string_builder.Append(pad_char, (int) Math.Max(pad_amount, num));
                    int length = string_builder.Length;
                    num3 = 0;
                    while (num > 0)
                    {
                        length--;
                        num3++;
                        if (thousandSeparation && ((num3 % 4) == 0))
                        {
                            num--;
                            string_builder[length] = NumberFormatInfo.InvariantInfo.NumberGroupSeparator[0];
                            continue;
                        }
                        string_builder[length] = ms_digits[(int) ((IntPtr) (uint_val % ((ulong) base_val)))];
                        uint_val /= (ulong) base_val;
                        num--;
                    }
                    return string_builder;
                }
            }
        }
    }
}

