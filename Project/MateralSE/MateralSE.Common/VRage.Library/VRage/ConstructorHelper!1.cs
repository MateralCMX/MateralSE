namespace VRage
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    public static class ConstructorHelper<T>
    {
        public static Delegate CreateInPlaceConstructor(Type constructorType)
        {
            Type[] emptyTypes;
            Type type = typeof(T);
            Type[] sourceArray = Array.ConvertAll<ParameterInfo, Type>(constructorType.GetMethod("Invoke").GetParameters(), p => p.ParameterType);
            if (sourceArray.Length <= 1)
            {
                emptyTypes = Type.EmptyTypes;
            }
            else
            {
                emptyTypes = new Type[sourceArray.Length - 1];
                Array.ConstrainedCopy(sourceArray, 1, emptyTypes, 0, sourceArray.Length - 1);
            }
            ConstructorInfo constructor = type.GetConstructor(emptyTypes);
            if (constructor == null)
            {
                throw new InvalidOperationException($"No matching constructor for object {type.Name} was found!");
            }
            DynamicMethod method = new DynamicMethod($"Pool<T>__{Guid.NewGuid().ToString().Replace("-", "")}", typeof(void), sourceArray, typeof(ConstructorHelper<T>), false);
            ILGenerator iLGenerator = method.GetILGenerator();
            for (int i = 0; i < sourceArray.Length; i++)
            {
                if (i >= 4)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_S, i);
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            iLGenerator.Emit(OpCodes.Ldarg_0);
                            break;

                        case 1:
                            iLGenerator.Emit(OpCodes.Ldarg_1);
                            break;

                        case 2:
                            iLGenerator.Emit(OpCodes.Ldarg_2);
                            break;

                        case 3:
                            iLGenerator.Emit(OpCodes.Ldarg_3);
                            break;

                        default:
                            break;
                    }
                }
            }
            iLGenerator.Emit(OpCodes.Callvirt, constructor);
            iLGenerator.Emit(OpCodes.Ret);
            return method.CreateDelegate(constructorType);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ConstructorHelper<T>.<>c <>9;
            public static Converter<ParameterInfo, Type> <>9__0_0;

            static <>c()
            {
                ConstructorHelper<T>.<>c.<>9 = new ConstructorHelper<T>.<>c();
            }

            internal Type <CreateInPlaceConstructor>b__0_0(ParameterInfo p) => 
                p.ParameterType;
        }
    }
}

