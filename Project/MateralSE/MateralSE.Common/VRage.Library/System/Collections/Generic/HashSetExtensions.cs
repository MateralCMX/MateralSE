namespace System.Collections.Generic
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class HashSetExtensions
    {
        public static T FirstElement<T>(this HashSet<T> hashset)
        {
            using (HashSet<T>.Enumerator enumerator = hashset.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException();
                }
                return enumerator.Current;
            }
        }

        public static bool TryGetValue<T>(this HashSet<T> hashset, T equalValue, out T actualValue)
        {
            if (hashset.Count > 0)
            {
                int num = HashSetInternalAccessor<T>.InternalIndexOfGetter(hashset, equalValue);
                if (num >= 0)
                {
                    actualValue = HashSetInternalAccessor<T>.SlotValueGetter(hashset, num);
                    return true;
                }
            }
            actualValue = default(T);
            return false;
        }

        public static class HashSetInternalAccessor<T>
        {
            public static readonly Func<HashSet<T>, int> VersionGetter;
            public static readonly Func<HashSet<T>, int, T> SlotValueGetter;
            public static readonly Func<HashSet<T>, T, int> InternalIndexOfGetter;

            static HashSetInternalAccessor()
            {
                Type type = typeof(HashSet<T>);
                Type cls = type.GetNestedType("Slot", BindingFlags.NonPublic).MakeGenericType(type.GenericTypeArguments);
                FieldInfo field = type.GetField("m_slots", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo info2 = cls.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo info3 = type.GetField("m_version", BindingFlags.NonPublic | BindingFlags.Instance);
                Type[] parameterTypes = new Type[] { type };
                DynamicMethod method = new DynamicMethod("VersionGetter", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), parameterTypes, typeof(HashSetExtensions.HashSetInternalAccessor<T>), true);
                ILGenerator iLGenerator = method.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, info3);
                iLGenerator.Emit(OpCodes.Ret);
                Type[] typeArray2 = new Type[] { type, typeof(int) };
                DynamicMethod method2 = new DynamicMethod("SlotValueGetter", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T), typeArray2, typeof(HashSetExtensions.HashSetInternalAccessor<T>), true);
                ILGenerator generator2 = method2.GetILGenerator();
                generator2.Emit(OpCodes.Ldarg_0);
                generator2.Emit(OpCodes.Ldfld, field);
                generator2.Emit(OpCodes.Ldarg_1);
                generator2.Emit(OpCodes.Ldelema, cls);
                generator2.Emit(OpCodes.Ldfld, info2);
                generator2.Emit(OpCodes.Ret);
                HashSetExtensions.HashSetInternalAccessor<T>.VersionGetter = (Func<HashSet<T>, int>) method.CreateDelegate(typeof(Func<HashSet<T>, int>));
                HashSetExtensions.HashSetInternalAccessor<T>.SlotValueGetter = (Func<HashSet<T>, int, T>) method2.CreateDelegate(typeof(Func<HashSet<T>, int, T>));
                HashSetExtensions.HashSetInternalAccessor<T>.InternalIndexOfGetter = (Func<HashSet<T>, T, int>) Delegate.CreateDelegate(typeof(Func<HashSet<T>, T, int>), typeof(HashSet<T>).GetMethod("InternalIndexOf", BindingFlags.NonPublic | BindingFlags.Instance));
            }
        }
    }
}

