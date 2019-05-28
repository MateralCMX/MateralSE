namespace System.Reflection
{
    using System;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    public static class FieldAccess
    {
        public static Func<TType, TMember> CreateGetter<TType, TMember>(this FieldInfo field)
        {
            Type[] parameterTypes = new Type[] { typeof(TType) };
            DynamicMethod method = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, typeof(TMember), parameterTypes, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (field.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType != typeof(TType))
                {
                    iLGenerator.Emit(OpCodes.Castclass, field.DeclaringType);
                }
                iLGenerator.Emit(OpCodes.Ldfld, field);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<TType, TMember>) method.CreateDelegate(typeof(Func<TType, TMember>));
        }

        public static Getter<TType, TMember> CreateGetterRef<TType, TMember>(this FieldInfo field)
        {
            Type[] parameterTypes = new Type[] { typeof(TType).MakeByRefType(), typeof(TMember).MakeByRefType() };
            DynamicMethod method = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, null, parameterTypes, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (field.IsStatic)
            {
                throw new NotImplementedException();
            }
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            if (!typeof(TType).IsValueType)
            {
                iLGenerator.Emit(OpCodes.Ldind_Ref);
            }
            if (field.DeclaringType != typeof(TType))
            {
                iLGenerator.Emit(OpCodes.Castclass, field.DeclaringType);
            }
            iLGenerator.Emit(OpCodes.Ldfld, field);
            if (field.FieldType != typeof(TMember))
            {
                iLGenerator.Emit(OpCodes.Castclass, typeof(TMember));
            }
            if (!typeof(TMember).IsValueType)
            {
                iLGenerator.Emit(OpCodes.Stind_Ref);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Stobj, typeof(TMember));
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (Getter<TType, TMember>) method.CreateDelegate(typeof(Getter<TType, TMember>));
        }

        public static Action<TMember> CreateSetter<TMember>(this FieldInfo field)
        {
            if (!field.IsStatic)
            {
                throw new InvalidOperationException("Field must be static");
            }
            Type[] parameterTypes = new Type[] { typeof(TMember) };
            DynamicMethod method = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, parameterTypes, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Stsfld, field);
            iLGenerator.Emit(OpCodes.Ret);
            return (Action<TMember>) method.CreateDelegate(typeof(Action<TMember>));
        }

        public static Action<TType, TMember> CreateSetter<TType, TMember>(this FieldInfo field)
        {
            Type[] parameterTypes = new Type[] { typeof(TType), typeof(TMember) };
            DynamicMethod method = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, parameterTypes, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (field.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                if (typeof(TType).IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Ldarga, 0);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                }
                if (field.DeclaringType != typeof(TType))
                {
                    iLGenerator.Emit(OpCodes.Castclass, field.DeclaringType);
                }
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Stfld, field);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (Action<TType, TMember>) method.CreateDelegate(typeof(Action<TType, TMember>));
        }

        public static Setter<TType, TMember> CreateSetterRef<TType, TMember>(this FieldInfo field)
        {
            Type[] parameterTypes = new Type[] { typeof(TType).MakeByRefType(), typeof(TMember).MakeByRefType() };
            DynamicMethod method = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, parameterTypes, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            if (field.IsStatic)
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
                if (!typeof(TType).IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Ldind_Ref);
                }
                if (field.DeclaringType != typeof(TType))
                {
                    iLGenerator.Emit(OpCodes.Castclass, field.DeclaringType);
                }
                iLGenerator.Emit(OpCodes.Ldarg_1);
                if (!typeof(TMember).IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Ldind_Ref);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldobj, typeof(TMember));
                }
                if (field.FieldType != typeof(TMember))
                {
                    iLGenerator.Emit(OpCodes.Castclass, field.FieldType);
                }
                iLGenerator.Emit(OpCodes.Stfld, field);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (Setter<TType, TMember>) method.CreateDelegate(typeof(Setter<TType, TMember>));
        }
    }
}

