namespace System.Reflection
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public static class PropertyAccess
    {
        public static Func<T, TProperty> CreateGetter<T, TProperty>(this PropertyInfo propertyInfo)
        {
            Type type = typeof(T);
            Type type2 = typeof(TProperty);
            ParameterExpression expression = Expression.Parameter(type, "value");
            Expression expression3 = Expression.Property((propertyInfo.DeclaringType == type) ? ((Expression) expression) : ((Expression) Expression.Convert(expression, propertyInfo.DeclaringType)), propertyInfo);
            if (type2 != propertyInfo.PropertyType)
            {
                expression3 = Expression.Convert(expression3, type2);
            }
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            return Expression.Lambda<Func<T, TProperty>>(expression3, parameters).Compile();
        }

        public static Getter<T, TProperty> CreateGetterRef<T, TProperty>(this PropertyInfo propertyInfo)
        {
            Type type = typeof(T);
            Type type2 = typeof(TProperty);
            ParameterExpression expression = Expression.Parameter(type.MakeByRefType(), "value");
            Expression expression3 = Expression.Property((propertyInfo.DeclaringType == type) ? ((Expression) expression) : ((Expression) Expression.Convert(expression, propertyInfo.DeclaringType)), propertyInfo);
            if (type2 != propertyInfo.PropertyType)
            {
                expression3 = Expression.Convert(expression3, type2);
            }
            ParameterExpression left = Expression.Parameter(type2.MakeByRefType(), "out");
            ParameterExpression[] parameters = new ParameterExpression[] { expression, left };
            return Expression.Lambda<Getter<T, TProperty>>(Expression.Assign(left, expression3), parameters).Compile();
        }

        public static Action<T, TProperty> CreateSetter<T, TProperty>(this PropertyInfo propertyInfo)
        {
            Type type = typeof(T);
            Type type2 = typeof(TProperty);
            ParameterExpression expression = Expression.Parameter(type);
            ParameterExpression expression2 = Expression.Parameter(type2);
            Expression expression3 = (propertyInfo.DeclaringType == type) ? ((Expression) expression) : ((Expression) Expression.Convert(expression, propertyInfo.DeclaringType));
            ParameterExpression[] parameters = new ParameterExpression[] { expression, expression2 };
            return Expression.Lambda<Action<T, TProperty>>(Expression.Assign(Expression.Property(expression3, propertyInfo), (propertyInfo.PropertyType == type2) ? ((Expression) expression2) : ((Expression) Expression.Convert(expression2, propertyInfo.PropertyType))), parameters).Compile();
        }

        public static Setter<T, TProperty> CreateSetterRef<T, TProperty>(this PropertyInfo propertyInfo)
        {
            Type type = typeof(T);
            Type type2 = typeof(TProperty);
            ParameterExpression expression = Expression.Parameter(type.MakeByRefType());
            ParameterExpression expression2 = Expression.Parameter(type2.MakeByRefType());
            Expression expression3 = (propertyInfo.DeclaringType == type) ? ((Expression) expression) : ((Expression) Expression.Convert(expression, propertyInfo.DeclaringType));
            ParameterExpression[] parameters = new ParameterExpression[] { expression, expression2 };
            return Expression.Lambda<Setter<T, TProperty>>(Expression.Assign(Expression.Property(expression3, propertyInfo), (propertyInfo.PropertyType == type2) ? ((Expression) expression2) : ((Expression) Expression.Convert(expression2, propertyInfo.PropertyType))), parameters).Compile();
        }
    }
}

