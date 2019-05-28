namespace System.Collections.Generic
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class ListInternalAccessor<T>
    {
        public static Func<List<T>, T[]> GetArray;
        public static Action<List<T>, int> SetSize;

        static ListInternalAccessor()
        {
            Type[] parameterTypes = new Type[] { typeof(List<T>) };
            DynamicMethod method = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), parameterTypes, typeof(ListInternalAccessor<T>), true);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
            iLGenerator.Emit(OpCodes.Ret);
            ListInternalAccessor<T>.GetArray = (Func<List<T>, T[]>) method.CreateDelegate(typeof(Func<List<T>, T[]>));
            Type[] typeArray2 = new Type[] { typeof(List<T>), typeof(int) };
            DynamicMethod method2 = new DynamicMethod("set", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, null, typeArray2, typeof(ListInternalAccessor<T>), true);
            ILGenerator generator2 = method2.GetILGenerator();
            generator2.Emit(OpCodes.Ldarg_0);
            generator2.Emit(OpCodes.Ldarg_1);
            generator2.Emit(OpCodes.Stfld, typeof(List<T>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance));
            FieldInfo field = typeof(List<T>).GetField("_version", BindingFlags.NonPublic | BindingFlags.Instance);
            generator2.Emit(OpCodes.Ldarg_0);
            generator2.Emit(OpCodes.Dup);
            generator2.Emit(OpCodes.Ldfld, field);
            generator2.Emit(OpCodes.Ldc_I4_1);
            generator2.Emit(OpCodes.Add);
            generator2.Emit(OpCodes.Stfld, field);
            generator2.Emit(OpCodes.Ret);
            ListInternalAccessor<T>.SetSize = (Action<List<T>, int>) method2.CreateDelegate(typeof(Action<List<T>, int>));
        }
    }
}

