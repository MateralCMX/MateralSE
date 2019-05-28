namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public sealed class EnumComparer<TEnum> : IEqualityComparer<TEnum>, IComparer<TEnum> where TEnum: struct, IComparable, IConvertible, IFormattable
    {
        private static readonly Func<TEnum, TEnum, bool> equalsFunct;
        private static readonly Func<TEnum, int> getHashCodeFunct;
        private static readonly Func<TEnum, TEnum, int> compareToFunct;
        private static readonly EnumComparer<TEnum> instance;

        static EnumComparer()
        {
            EnumComparer<TEnum>.getHashCodeFunct = EnumComparer<TEnum>.GenerateGetHashCodeFunct();
            EnumComparer<TEnum>.equalsFunct = EnumComparer<TEnum>.GenerateEqualsFunct();
            EnumComparer<TEnum>.compareToFunct = EnumComparer<TEnum>.GenerateCompareToFunct();
            EnumComparer<TEnum>.instance = new EnumComparer<TEnum>();
        }

        private EnumComparer()
        {
            EnumComparer<TEnum>.AssertTypeIsEnum();
            EnumComparer<TEnum>.AssertUnderlyingTypeIsSupported();
        }

        private static void AssertTypeIsEnum()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new NotSupportedException($"The type parameter {typeof(TEnum)} is not an Enum. LcgEnumComparer supports Enums only.");
            }
        }

        private static void AssertUnderlyingTypeIsSupported()
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            Type[] typeArray1 = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
            if (!typeArray1.Contains(underlyingType))
            {
                throw new NotSupportedException($"The underlying type of the type parameter {typeof(TEnum)} is {underlyingType}. LcgEnumComparer only supports Enums with underlying type of byte, sbyte, short, ushort, int, uint, long, or ulong.");
            }
        }

        public int Compare(TEnum x, TEnum y) => 
            EnumComparer<TEnum>.compareToFunct(x, y);

        public bool Equals(TEnum x, TEnum y) => 
            EnumComparer<TEnum>.equalsFunct(x, y);

        private static Func<TEnum, TEnum, int> GenerateCompareToFunct()
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            ParameterExpression expression = Expression.Parameter(typeof(TEnum), "x");
            ParameterExpression expression2 = Expression.Parameter(typeof(TEnum), "y");
            UnaryExpression expression4 = Expression.Convert(expression2, underlyingType);
            Type[] types = new Type[] { underlyingType };
            Expression[] arguments = new Expression[] { expression4 };
            ParameterExpression[] parameters = new ParameterExpression[] { expression, expression2 };
            return Expression.Lambda<Func<TEnum, TEnum, int>>(Expression.Call(Expression.Convert(expression, underlyingType), underlyingType.GetMethod("CompareTo", types), arguments), parameters).Compile();
        }

        private static Func<TEnum, TEnum, bool> GenerateEqualsFunct()
        {
            ParameterExpression left = Expression.Parameter(typeof(TEnum), "x");
            ParameterExpression right = Expression.Parameter(typeof(TEnum), "y");
            ParameterExpression[] parameters = new ParameterExpression[] { left, right };
            return Expression.Lambda<Func<TEnum, TEnum, bool>>(Expression.Equal(left, right), parameters).Compile();
        }

        private static Func<TEnum, int> GenerateGetHashCodeFunct()
        {
            ParameterExpression expression = Expression.Parameter(typeof(TEnum), "obj");
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            return Expression.Lambda<Func<TEnum, int>>(Expression.Call(Expression.Convert(expression, underlyingType), underlyingType.GetMethod("GetHashCode")), parameters).Compile();
        }

        public int GetHashCode(TEnum obj) => 
            EnumComparer<TEnum>.getHashCodeFunct(obj);

        public static EnumComparer<TEnum> Instance =>
            EnumComparer<TEnum>.instance;
    }
}

