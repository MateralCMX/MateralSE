namespace VRage
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class MemberHelper
    {
        public static MemberInfo GetMember<TValue>(Expression<Func<TValue>> selector)
        {
            Exceptions.ThrowIf<ArgumentNullException>(ReferenceEquals(selector, null), "selector");
            MemberExpression body = selector.Body as MemberExpression;
            Exceptions.ThrowIf<ArgumentNullException>(ReferenceEquals(body, null), "Selector must be a member access expression", "selector");
            return body.Member;
        }
    }
}

