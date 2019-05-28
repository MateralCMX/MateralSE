namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class StringBuilderExtensions_Format
    {
        public static StringBuilder AppendStringBuilder(this StringBuilder stringBuilder, StringBuilder otherStringBuilder)
        {
            stringBuilder.EnsureCapacity(stringBuilder.Length + otherStringBuilder.Length);
            for (int i = 0; i < otherStringBuilder.Length; i++)
            {
                stringBuilder.Append(otherStringBuilder[i]);
            }
            return stringBuilder;
        }

        public static StringBuilder AppendSubstring(this StringBuilder stringBuilder, StringBuilder append, int start, int count)
        {
            stringBuilder.EnsureCapacity(stringBuilder.Length + count);
            for (int i = 0; i < count; i++)
            {
                stringBuilder.Append(append[start + i]);
            }
            return stringBuilder;
        }

        public static StringBuilder ConcatFormat<A>(this StringBuilder string_builder, string format_string, A arg1, NumberFormatInfo numberFormat = null) where A: IConvertible => 
            string_builder.ConcatFormat<A, int, int, int, int>(format_string, arg1, 0, 0, 0, 0, numberFormat);

        public static StringBuilder ConcatFormat<A, B>(this StringBuilder string_builder, string format_string, A arg1, B arg2, NumberFormatInfo numberFormat = null) where A: IConvertible where B: IConvertible => 
            string_builder.ConcatFormat<A, B, int, int, int>(format_string, arg1, arg2, 0, 0, 0, numberFormat);

        public static StringBuilder ConcatFormat<A, B, C>(this StringBuilder string_builder, string format_string, A arg1, B arg2, C arg3, NumberFormatInfo numberFormat = null) where A: IConvertible where B: IConvertible where C: IConvertible => 
            string_builder.ConcatFormat<A, B, C, int, int>(format_string, arg1, arg2, arg3, 0, 0, numberFormat);

        public static StringBuilder ConcatFormat<A, B, C, D>(this StringBuilder string_builder, string format_string, A arg1, B arg2, C arg3, D arg4, NumberFormatInfo numberFormat = null) where A: IConvertible where B: IConvertible where C: IConvertible where D: IConvertible => 
            string_builder.ConcatFormat<A, B, C, D, int>(format_string, arg1, arg2, arg3, arg4, 0, numberFormat);

        public static StringBuilder ConcatFormat<A, B, C, D, E>(this StringBuilder string_builder, string format_string, A arg1, B arg2, C arg3, D arg4, E arg5, NumberFormatInfo numberFormat = null) where A: IConvertible where B: IConvertible where C: IConvertible where D: IConvertible where E: IConvertible
        {
            int startIndex = 0;
            numberFormat = numberFormat ?? CultureInfo.InvariantCulture.NumberFormat;
            for (int i = 0; i < format_string.Length; i++)
            {
                if (format_string[i] == '{')
                {
                    if (startIndex < i)
                    {
                        string_builder.Append(format_string, startIndex, i - startIndex);
                    }
                    uint num3 = 10;
                    uint padding = 0;
                    uint numberDecimalDigits = (uint) numberFormat.NumberDecimalDigits;
                    bool thousandSeparation = !string.IsNullOrEmpty(numberFormat.NumberGroupSeparator);
                    i++;
                    char ch = format_string[i];
                    if (ch == '{')
                    {
                        string_builder.Append('{');
                        i++;
                    }
                    else
                    {
                        i++;
                        if (format_string[i] == ':')
                        {
                            i++;
                            while (true)
                            {
                                if (format_string[i] != '0')
                                {
                                    if (format_string[i] == 'X')
                                    {
                                        i++;
                                        num3 = 0x10;
                                        if ((format_string[i] >= '0') && (format_string[i] <= '9'))
                                        {
                                            padding = (uint) (format_string[i] - '0');
                                            i++;
                                        }
                                    }
                                    else if (format_string[i] == '.')
                                    {
                                        i++;
                                        numberDecimalDigits = 0;
                                        while (format_string[i] == '0')
                                        {
                                            i++;
                                            numberDecimalDigits++;
                                        }
                                    }
                                    break;
                                }
                                i++;
                                padding++;
                            }
                        }
                        while (true)
                        {
                            if (format_string[i] == '}')
                            {
                                switch (ch)
                                {
                                    case '0':
                                        string_builder.ConcatFormatValue<A>(arg1, padding, num3, numberDecimalDigits, thousandSeparation);
                                        break;

                                    case '1':
                                        string_builder.ConcatFormatValue<B>(arg2, padding, num3, numberDecimalDigits, thousandSeparation);
                                        break;

                                    case '2':
                                        string_builder.ConcatFormatValue<C>(arg3, padding, num3, numberDecimalDigits, thousandSeparation);
                                        break;

                                    case '3':
                                        string_builder.ConcatFormatValue<D>(arg4, padding, num3, numberDecimalDigits, thousandSeparation);
                                        break;

                                    case '4':
                                        string_builder.ConcatFormatValue<E>(arg5, padding, num3, numberDecimalDigits, thousandSeparation);
                                        break;

                                    default:
                                        break;
                                }
                                break;
                            }
                            i++;
                        }
                    }
                    startIndex = i + 1;
                }
            }
            if (startIndex < format_string.Length)
            {
                string_builder.Append(format_string, startIndex, format_string.Length - startIndex);
            }
            return string_builder;
        }

        private static void ConcatFormatValue<T>(this StringBuilder string_builder, T arg, uint padding, uint base_value, uint decimal_places, bool thousandSeparation) where T: IConvertible
        {
            switch (arg.GetTypeCode())
            {
                case TypeCode.Boolean:
                    if (arg.ToBoolean(CultureInfo.InvariantCulture))
                    {
                        string_builder.Append("true");
                        return;
                    }
                    string_builder.Append("false");
                    return;

                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.DateTime:
                case (TypeCode.DateTime | TypeCode.Object):
                    break;

                case TypeCode.Int32:
                    string_builder.Concat(arg.ToInt32(NumberFormatInfo.InvariantInfo), padding, '0', base_value, thousandSeparation);
                    return;

                case TypeCode.UInt32:
                    string_builder.Concat((long) arg.ToUInt32(NumberFormatInfo.InvariantInfo), padding, '0', base_value, thousandSeparation);
                    return;

                case TypeCode.Int64:
                    string_builder.Concat(arg.ToInt64(NumberFormatInfo.InvariantInfo), padding, '0', base_value, thousandSeparation);
                    return;

                case TypeCode.UInt64:
                    string_builder.Concat(arg.ToInt32(NumberFormatInfo.InvariantInfo), padding, '0', base_value, thousandSeparation);
                    return;

                case TypeCode.Single:
                    string_builder.Concat(arg.ToSingle(NumberFormatInfo.InvariantInfo), decimal_places, padding, '0', false);
                    return;

                case TypeCode.Double:
                    string_builder.Concat(arg.ToDouble(NumberFormatInfo.InvariantInfo), decimal_places, padding, '0', false);
                    return;

                case TypeCode.Decimal:
                    string_builder.Concat(arg.ToSingle(NumberFormatInfo.InvariantInfo), decimal_places, padding, '0', false);
                    return;

                case TypeCode.String:
                    string_builder.Append(arg.ToString());
                    break;

                default:
                    return;
            }
        }

        public static StringBuilder FirstLetterUpperCase(this StringBuilder self)
        {
            if (self.Length > 0)
            {
                self[0] = char.ToUpper(self[0]);
            }
            return self;
        }

        public static StringBuilder ToLower(this StringBuilder self)
        {
            for (int i = 0; i < self.Length; i++)
            {
                self[i] = char.ToLower(self[i]);
            }
            return self;
        }

        public static StringBuilder ToUpper(this StringBuilder self)
        {
            for (int i = 0; i < self.Length; i++)
            {
                self[i] = char.ToUpper(self[i]);
            }
            return self;
        }
    }
}

