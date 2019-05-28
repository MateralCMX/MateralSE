namespace System
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class MethodInfoExtensions
    {
        private static void CheckParameterCountsAreEqual(IEnumerable<ParameterExpression> delegateParameters, IEnumerable<ParameterInfo> methodParameters)
        {
            if (delegateParameters.Count<ParameterExpression>() != methodParameters.Count<ParameterInfo>())
            {
                throw new InvalidOperationException("The number of parameters of the requested delegate does not match the number parameters of the specified method.");
            }
        }

        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method) where TDelegate: class => 
            CreateDelegate<TDelegate>((MethodBase) method, (Func<Type[], ParameterExpression[], MethodCallExpression>) ((typeArguments, parameterExpressions) => Expression.Call(method, ProvideStrongArgumentsFor(method, parameterExpressions))));

        private static TDelegate CreateDelegate<TDelegate>(MethodBase method, Func<Type[], ParameterExpression[], MethodCallExpression> getCallExpression)
        {
            ParameterExpression[] delegateParameters = ExtractParameterExpressionsFrom<TDelegate>();
            CheckParameterCountsAreEqual(delegateParameters, method.GetParameters());
            return Expression.Lambda<TDelegate>(getCallExpression(GetTypeArgumentsFor(method), delegateParameters), delegateParameters).Compile();
        }

        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method, object instance) where TDelegate: class
        {
            <>c__DisplayClass0_0<TDelegate> class_;
            return CreateDelegate<TDelegate>((MethodBase) method, (Func<Type[], ParameterExpression[], MethodCallExpression>) ((typeArguments, parameterExpressions) => Expression.Call(Expression.Convert(Expression.Lambda<Func<object>>(Expression.Field(Expression.Constant(class_, typeof(<>c__DisplayClass0_0<TDelegate>)), fieldof(<>c__DisplayClass0_0<TDelegate>.instance, <>c__DisplayClass0_0<TDelegate>)), Array.Empty<ParameterExpression>()).Body, instance.GetType()), method.Name, typeArguments, ProvideStrongArgumentsFor(method, parameterExpressions))));
        }

        public static ParameterExpression[] ExtractParameterExpressionsFrom<TDelegate>() => 
            (from s in typeof(TDelegate).GetMethod("Invoke").GetParameters() select Expression.Parameter(s.ParameterType)).ToArray<ParameterExpression>();

        private static Type[] GetTypeArgumentsFor(MethodBase method) => 
            null;

        private static Expression[] ProvideStrongArgumentsFor(MethodInfo method, ParameterExpression[] parameterExpressions) => 
            method.GetParameters().Select<ParameterInfo, UnaryExpression>((parameter, index) => Expression.Convert(parameterExpressions[index], parameter.ParameterType)).ToArray<UnaryExpression>();

        [Serializable, CompilerGenerated]
        private sealed class <>c__3<TDelegate>
        {
            public static readonly MethodInfoExtensions.<>c__3<TDelegate> <>9;
            public static Func<ParameterInfo, ParameterExpression> <>9__3_0;

            static <>c__3()
            {
                MethodInfoExtensions.<>c__3<TDelegate>.<>9 = new MethodInfoExtensions.<>c__3<TDelegate>();
            }

            internal ParameterExpression <ExtractParameterExpressionsFrom>b__3_0(ParameterInfo s) => 
                Expression.Parameter(s.ParameterType);
        }
    }
}

