namespace VRage.Library.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class GenericCallCache
    {
        private Dictionary<Key, object> m_cache = new Dictionary<Key, object>(new KeyComparer());

        public TDelegate Get<TDelegate>(MethodInfo methodInfo, Type[] arguments) where TDelegate: class
        {
            object obj2;
            Key key = new Key(methodInfo, arguments);
            if (!this.m_cache.TryGetValue(key, out obj2))
            {
                obj2 = methodInfo.MakeGenericMethod(arguments).CreateDelegate<TDelegate>();
                this.m_cache[key] = obj2;
            }
            return (TDelegate) obj2;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Key
        {
            public MethodInfo Method;
            public Type[] Arguments;
            public Key(MethodInfo method, Type[] typeArgs)
            {
                this.Method = method;
                this.Arguments = typeArgs;
            }
        }

        private class KeyComparer : IEqualityComparer<GenericCallCache.Key>
        {
            public bool Equals(GenericCallCache.Key x, GenericCallCache.Key y)
            {
                if ((x.Method != y.Method) || (x.Arguments.Length != y.Arguments.Length))
                {
                    return false;
                }
                for (int i = 0; i < x.Arguments.Length; i++)
                {
                    if (x.Arguments[i] != y.Arguments[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(GenericCallCache.Key obj)
            {
                int hashCode = obj.Method.GetHashCode();
                for (int i = 0; i < obj.Arguments.Length; i++)
                {
                    hashCode = (hashCode * 0x18d) ^ obj.Arguments[i].GetHashCode();
                }
                return hashCode;
            }
        }
    }
}

