namespace System.Linq.Expressions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class ExpressionExtension
    {
        public static Func<T> CreateActivator<T>() where T: new() => 
            Expression.Lambda<Func<T>>(Expression.New(typeof(T)), Array.Empty<ParameterExpression>()).Compile();

        public static Func<T> CreateActivator<T>(Type t) => 
            Expression.Lambda<Func<T>>(Expression.New(t), Array.Empty<ParameterExpression>()).Compile();

        public static Func<T, TMember> CreateGetter<T, TMember>(this Expression<Func<T, TMember>> expression)
        {
            MemberExpression body = (MemberExpression) expression.Body;
            if (!(body.Member is PropertyInfo))
            {
                return ((FieldInfo) body.Member).CreateGetter<T, TMember>();
            }
            ParameterExpression expression3 = Expression.Parameter(typeof(T), "value");
            ParameterExpression[] parameters = new ParameterExpression[] { expression3 };
            return Expression.Lambda<Func<T, TMember>>(Expression.Property(expression3, (PropertyInfo) body.Member), parameters).Compile();
        }

        public static Func<T> CreateParametrizedActivator<T, P>(P parameter) => 
            CreateParametrizedActivator<T, P>(typeof(T), parameter);

        public static Func<T> CreateParametrizedActivator<T, P>(Type t, P parameter)
        {
            ConstantExpression expression = Expression.Constant(parameter);
            Type[] types = new Type[] { typeof(P) };
            Expression[] arguments = new Expression[] { expression };
            return Expression.Lambda<Func<T>>(Expression.New(t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, types, null), arguments), Array.Empty<ParameterExpression>()).Compile();
        }

        public static Action<T, TMember> CreateSetter<T, TMember>(this Expression<Func<T, TMember>> expression)
        {
            MemberExpression body = (MemberExpression) expression.Body;
            if (!(body.Member is PropertyInfo))
            {
                return ((FieldInfo) body.Member).CreateSetter<T, TMember>();
            }
            ParameterExpression expression3 = Expression.Parameter(typeof(T));
            ParameterExpression right = Expression.Parameter(typeof(TMember));
            ParameterExpression[] parameters = new ParameterExpression[] { expression3, right };
            return Expression.Lambda<Action<T, TMember>>(Expression.Assign(Expression.Property(expression3, (PropertyInfo) body.Member), right), parameters).Compile();
        }

        public static TDelegate InstanceCall<TDelegate>(this MethodInfo info)
        {
            ParameterInfo[] parameters = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            ParameterInfo[] infoArray2 = info.GetParameters();
            ParameterExpression[] expressionArray = (from s in parameters select Expression.Parameter(s.ParameterType, s.Name)).ToArray<ParameterExpression>();
            Expression[] arguments = new Expression[infoArray2.Length];
            for (int i = 0; i < infoArray2.Length; i++)
            {
                arguments[i] = !(infoArray2[i].ParameterType == parameters[i].ParameterType) ? ((Expression) Expression.Convert(expressionArray[i + 1], infoArray2[i].ParameterType)) : ((Expression) expressionArray[i + 1]);
            }
            return Expression.Lambda<TDelegate>(Expression.Call((parameters[0].ParameterType == info.DeclaringType) ? ((Expression) expressionArray[0]) : ((Expression) Expression.Convert(expressionArray[0], info.DeclaringType)), info, arguments), expressionArray).Compile();
        }

        public static TDelegate StaticCall<TDelegate>(this MethodInfo info)
        {
            ParameterExpression[] parameters = (from s in info.GetParameters() select Expression.Parameter(s.ParameterType, s.Name)).ToArray<ParameterExpression>();
            return Expression.Lambda<TDelegate>(Expression.Call(info, parameters), parameters).Compile();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__0<T> where T: new()
        {
            public static readonly ExpressionExtension.<>c__0<T> <>9;

            static <>c__0()
            {
                ExpressionExtension.<>c__0<T>.<>9 = new ExpressionExtension.<>c__0<T>();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__6<TDelegate>
        {
            public static readonly ExpressionExtension.<>c__6<TDelegate> <>9;
            public static Func<ParameterInfo, ParameterExpression> <>9__6_0;

            static <>c__6()
            {
                ExpressionExtension.<>c__6<TDelegate>.<>9 = new ExpressionExtension.<>c__6<TDelegate>();
            }

            internal ParameterExpression <StaticCall>b__6_0(ParameterInfo s) => 
                Expression.Parameter(s.ParameterType, s.Name);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__7<TDelegate>
        {
            public static readonly ExpressionExtension.<>c__7<TDelegate> <>9;
            public static Func<ParameterInfo, ParameterExpression> <>9__7_0;

            static <>c__7()
            {
                ExpressionExtension.<>c__7<TDelegate>.<>9 = new ExpressionExtension.<>c__7<TDelegate>();
            }

            internal ParameterExpression <InstanceCall>b__7_0(ParameterInfo s) => 
                Expression.Parameter(s.ParameterType, s.Name);
        }
    }
}

