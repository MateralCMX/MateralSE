namespace VRage.Library.Utils
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class TypeEmiter
    {
        private static System.Reflection.Emit.ModuleBuilder m_moduleBuilder;

        public static void EmitForMethod(MethodInfo method, Type baseType)
        {
            TypeBuilder typeBuilder = GetTypeBuilder(method.Name, baseType);
            ParameterInfo[] parameters = method.GetParameters();
            ILGenerator iLGenerator = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public, CallingConventions.Standard, method.ReturnType, (from x in parameters select x.ParameterType).ToArray<Type>()).GetILGenerator();
            iLGenerator.Emit(OpCodes.Call, method);
            iLGenerator.Emit(OpCodes.Ret);
            Type type = typeBuilder.CreateType();
            object firstArgument = Activator.CreateInstance(type);
            ((Action) Delegate.CreateDelegate(typeof(Action), firstArgument, type.GetMethod("Invoke")))();
        }

        public static TypeBuilder GetTypeBuilder(string typeName, Type baseType = null) => 
            ModuleBuilder.DefineType(typeName, TypeAttributes.AutoClass | TypeAttributes.Public, baseType);

        public static System.Reflection.Emit.ModuleBuilder ModuleBuilder
        {
            get
            {
                if (m_moduleBuilder == null)
                {
                    AssemblyName name = new AssemblyName(typeof(TypeEmiter).Name);
                    m_moduleBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule("MainModule");
                }
                return m_moduleBuilder;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly TypeEmiter.<>c <>9 = new TypeEmiter.<>c();
            public static Func<ParameterInfo, Type> <>9__4_0;

            internal Type <EmitForMethod>b__4_0(ParameterInfo x) => 
                x.ParameterType;
        }
    }
}

