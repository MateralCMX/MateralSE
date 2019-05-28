namespace System.Linq.Expressions
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class FieldExtensions
    {
        public static Func<T, TMember> CreateGetter<T, TMember>(this FieldInfo info)
        {
            if (!typeof(T).IsAssignableFrom(info.DeclaringType))
            {
                throw new ArgumentException("T must be assignable from field declaring type: " + info.DeclaringType);
            }
            ParameterExpression expression = Expression.Parameter(typeof(T), "obj");
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            return Expression.Lambda<Func<T, TMember>>(Expression.Field(Expression.Convert(expression, info.DeclaringType), info.Name), parameters).Compile();
        }

        public static Action<T, TMember> CreateSetter<T, TMember>(this FieldInfo info)
        {
            if (!typeof(T).IsAssignableFrom(info.DeclaringType))
            {
                throw new ArgumentException("T must be assignable from field declaring type: " + info.DeclaringType);
            }
            ParameterExpression expression = Expression.Parameter(typeof(T), "obj");
            ParameterExpression right = Expression.Parameter(info.FieldType, "value");
            ParameterExpression[] parameters = new ParameterExpression[] { expression, right };
            return Expression.Lambda<Action<T, TMember>>(Expression.Assign(Expression.Field(Expression.Convert(expression, info.DeclaringType), info.Name), right), parameters).Compile();
        }
    }
}

