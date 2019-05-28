namespace VRage.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage;
    using VRage.Collections;
    using VRage.Network;

    public static class MyFactory
    {
        private static ThreadSafeStore<Type, MySerializer> m_serializers = new ThreadSafeStore<Type, MySerializer>(new Func<Type, MySerializer>(MyFactory.CreateSerializerInternal));
        private static Dictionary<Type, Type> m_serializerTypes = new Dictionary<Type, Type>();

        static MyFactory()
        {
            RegisterFromAssembly(Assembly.GetExecutingAssembly());
        }

        public static MySerializeInfo CreateInfo(MemberInfo member) => 
            MySerializeInfo.Create(member);

        public static MyMemberSerializer<TOwner> CreateMemberSerializer<TOwner>(MemberInfo member) => 
            ((MyMemberSerializer<TOwner>) CreateMemberSerializer(member, typeof(TOwner)));

        public static MyMemberSerializer CreateMemberSerializer(MemberInfo member, Type ownerType)
        {
            Type[] typeArguments = new Type[] { ownerType, member.GetMemberType() };
            MyMemberSerializer serializer1 = (MyMemberSerializer) Activator.CreateInstance(typeof(MyMemberSerializer<,>).MakeGenericType(typeArguments));
            serializer1.Init(member, CreateInfo(member));
            return serializer1;
        }

        private static MySerializer CreateSerializerInternal(Type t)
        {
            Type type;
            Dictionary<Type, Type> serializerTypes = m_serializerTypes;
            lock (serializerTypes)
            {
                m_serializerTypes.TryGetValue(t, out type);
            }
            if (type != null)
            {
                return (MySerializer) Activator.CreateInstance(type);
            }
            if (t.IsEnum)
            {
                Type[] typeArray1 = new Type[] { t };
                return (MySerializer) Activator.CreateInstance(typeof(MySerializerEnum<>).MakeGenericType(typeArray1));
            }
            if (t.IsArray)
            {
                Type[] typeArray2 = new Type[] { t.GetElementType() };
                return (MySerializer) Activator.CreateInstance(typeof(MySerializerArray<>).MakeGenericType(typeArray2));
            }
            if (typeof(IMyNetObject).IsAssignableFrom(t))
            {
                Type[] typeArray3 = new Type[] { t };
                return (MySerializer) Activator.CreateInstance(typeof(MySerializerNetObject<>).MakeGenericType(typeArray3));
            }
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                Type[] typeArray4 = new Type[] { t.GetGenericArguments()[0] };
                return (MySerializer) Activator.CreateInstance(typeof(MySerializerNullable<>).MakeGenericType(typeArray4));
            }
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>)))
            {
                Type[] typeArray5 = new Type[] { t.GetGenericArguments()[0] };
                return (MySerializer) Activator.CreateInstance(typeof(MySerializerList<>).MakeGenericType(typeArray5));
            }
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(HashSet<>)))
            {
                Type[] typeArray6 = new Type[] { t.GetGenericArguments()[0] };
                return (MySerializer) Activator.CreateInstance(typeof(MySerializerHashSet<>).MakeGenericType(typeArray6));
            }
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                Type[] genericArguments = t.GetGenericArguments();
                Type[] typeArray7 = new Type[] { genericArguments[0], genericArguments[1] };
                return (MySerializer) Activator.CreateInstance(typeof(MySerializerDictionary<,>).MakeGenericType(typeArray7));
            }
            if (!t.IsClass && !t.IsStruct())
            {
                throw new InvalidOperationException("No serializer found for type: " + t.Name);
            }
            Type[] typeArguments = new Type[] { t };
            return (MySerializer) Activator.CreateInstance(typeof(MySerializerObject<>).MakeGenericType(typeArguments));
        }

        public static MySerializer<T> GetSerializer<T>() => 
            ((MySerializer<T>) GetSerializer(typeof(T)));

        public static MySerializer GetSerializer(Type t) => 
            m_serializers.Get(t);

        public static void Register(Type serializedType, Type serializer)
        {
            Dictionary<Type, Type> serializerTypes = m_serializerTypes;
            lock (serializerTypes)
            {
                m_serializerTypes.Add(serializedType, serializer);
            }
        }

        public static void RegisterFromAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if ((!type.IsGenericType && ((type.BaseType != null) && type.BaseType.IsGenericType)) && (type.BaseType.GetGenericTypeDefinition() == typeof(MySerializer<>)))
                {
                    Register(type.BaseType.GetGenericArguments()[0], type);
                }
            }
        }
    }
}

