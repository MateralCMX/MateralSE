namespace VRage.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Serialization;

    public static class SyncHelpers
    {
        private static Dictionary<Type, List<Tuple<Composer, MySerializeInfo>>> m_composers = new Dictionary<Type, List<Tuple<Composer, MySerializeInfo>>>();
        private static FastResourceLock m_composersLock = new FastResourceLock();

        public static SyncType Compose(object obj, int firstId = 0)
        {
            List<SyncBase> resultList = new List<SyncBase>();
            Compose(obj, firstId, resultList);
            return new SyncType(resultList);
        }

        public static void Compose(object obj, int startingId, List<SyncBase> resultList)
        {
            List<Tuple<Composer, MySerializeInfo>> list;
            Type key = obj.GetType();
            using (m_composersLock.AcquireExclusiveUsing())
            {
                if (!m_composers.TryGetValue(key, out list))
                {
                    list = CreateComposer(key);
                    m_composers.Add(key, list);
                }
            }
            foreach (Tuple<Composer, MySerializeInfo> tuple in list)
            {
                startingId++;
                SyncBase item = tuple.Item1(obj, startingId, tuple.Item2);
                item.DebugName = tuple.Item1.Method.Name;
                resultList.Add(item);
            }
        }

        private static List<Tuple<Composer, MySerializeInfo>> CreateComposer(Type type)
        {
            List<Tuple<Composer, MySerializeInfo>> list = new List<Tuple<Composer, MySerializeInfo>>();
            foreach (FieldInfo info in type.GetDataMembers(true, false, true, true, false, true, true, true).OfType<FieldInfo>())
            {
                if (typeof(SyncBase).IsAssignableFrom(info.FieldType))
                {
                    list.Add(new Tuple<Composer, MySerializeInfo>(CreateFieldComposer(info), MyFactory.CreateInfo(info)));
                }
            }
            return list;
        }

        private static Composer CreateFieldComposer(FieldInfo field)
        {
            Type[] types = new Type[] { typeof(int), typeof(MySerializeInfo) };
            ConstructorInfo constructor = field.FieldType.GetConstructor(types);
            Module m = Assembly.GetEntryAssembly().GetModules()[0];
            Type[] parameterTypes = new Type[] { typeof(object), typeof(int), typeof(MySerializeInfo) };
            DynamicMethod method1 = new DynamicMethod("set" + field.Name, typeof(SyncBase), parameterTypes, m, true);
            ILGenerator iLGenerator = method1.GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(typeof(SyncBase));
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Castclass, field.DeclaringType);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Ldarg_2);
            iLGenerator.Emit(OpCodes.Newobj, constructor);
            iLGenerator.Emit(OpCodes.Dup);
            iLGenerator.Emit(OpCodes.Stloc, local);
            iLGenerator.Emit(OpCodes.Stfld, field);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Ret);
            return (Composer) method1.CreateDelegate(typeof(Composer));
        }

        internal delegate SyncBase Composer(object instance, int id, MySerializeInfo serializeInfo);
    }
}

