namespace VRage.Native
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class NativeCallHelper<TDelegate> where TDelegate: class
    {
        public static readonly TDelegate Invoke;

        static NativeCallHelper()
        {
            NativeCallHelper<TDelegate>.Invoke = NativeCallHelper<TDelegate>.Create();
        }

        private static unsafe TDelegate Create()
        {
            MethodInfo info = typeof(TDelegate).GetMethod("Invoke");
            Type[] source = (from s in info.GetParameters() select s.ParameterType).ToArray<Type>();
            if ((source.Length == 0) || (source[0] != typeof(IntPtr)))
            {
                throw new InvalidOperationException("First parameter must be function pointer");
            }
            Type[] parameterTypes = (from s in source.Skip<Type>(1) select (s == typeof(IntPtr)) ? ((IEnumerable<Type>) typeof(void*)) : ((IEnumerable<Type>) s)).ToArray<Type>();
            DynamicMethod method = new DynamicMethod(string.Empty, info.ReturnType, source, Assembly.GetExecutingAssembly().ManifestModule);
            ILGenerator iLGenerator = method.GetILGenerator();
            for (int i = 1; i < source.Length; i++)
            {
                iLGenerator.Emit(OpCodes.Ldarg, i);
            }
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldind_I);
            iLGenerator.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, info.ReturnType, parameterTypes);
            iLGenerator.Emit(OpCodes.Ret);
            return method.CreateDelegate<TDelegate>();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly NativeCallHelper<TDelegate>.<>c <>9;
            public static Func<ParameterInfo, Type> <>9__1_0;
            public static Func<Type, Type> <>9__1_1;

            static <>c()
            {
                NativeCallHelper<TDelegate>.<>c.<>9 = new NativeCallHelper<TDelegate>.<>c();
            }

            internal Type <Create>b__1_0(ParameterInfo s) => 
                s.ParameterType;

            internal unsafe Type <Create>b__1_1(Type s) => 
                ((s == typeof(IntPtr)) ? typeof(void*) : s);
        }
    }
}

