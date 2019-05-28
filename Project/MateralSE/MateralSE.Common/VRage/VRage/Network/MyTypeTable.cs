namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRage.Utils;

    public class MyTypeTable
    {
        private List<MySynchronizedTypeInfo> m_idToType = new List<MySynchronizedTypeInfo>();
        private Dictionary<Type, MySynchronizedTypeInfo> m_typeLookup = new Dictionary<Type, MySynchronizedTypeInfo>();
        private Dictionary<int, MySynchronizedTypeInfo> m_hashLookup = new Dictionary<int, MySynchronizedTypeInfo>();
        private MyEventTable m_staticEventTable = new MyEventTable(null);

        private static bool CanHaveEvents(Type type) => 
            (Attribute.IsDefined(type, typeof(StaticEventOwnerAttribute)) || typeof(IMyEventOwner).IsAssignableFrom(type));

        public bool Contains(Type type) => 
            this.m_typeLookup.ContainsKey(type);

        private MySynchronizedTypeInfo CreateBaseType(Type type)
        {
            while ((type.BaseType != null) && (type.BaseType != typeof(object)))
            {
                if (ShouldRegister(type.BaseType))
                {
                    return this.Register(type.BaseType);
                }
                type = type.BaseType;
            }
            return null;
        }

        public MySynchronizedTypeInfo Get(Type type) => 
            this.m_typeLookup[type];

        public MySynchronizedTypeInfo Get(TypeId id)
        {
            if (id.Value >= this.m_idToType.Count)
            {
                MyLog.Default.WriteLine("Invalid replication type ID: " + id.Value);
            }
            return this.m_idToType[(int) id.Value];
        }

        private static bool HasEvents(Type type) => 
            type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any<MemberInfo>(s => s.HasAttribute<EventAttribute>());

        private static bool IsReplicated(Type type) => 
            (!type.IsAbstract && (typeof(IMyReplicable).IsAssignableFrom(type) && !type.HasAttribute<NotReplicableAttribute>()));

        private static bool IsSerializableClass(Type type) => 
            type.HasAttribute<SerializableAttribute>();

        public MySynchronizedTypeInfo Register(Type type)
        {
            MySynchronizedTypeInfo info;
            if (!this.m_typeLookup.TryGetValue(type, out info))
            {
                MySynchronizedTypeInfo baseType = this.CreateBaseType(type);
                bool isReplicated = IsReplicated(type);
                if (isReplicated || HasEvents(type))
                {
                    info = new MySynchronizedTypeInfo(type, new TypeId((uint) this.m_idToType.Count), baseType, isReplicated);
                    this.m_idToType.Add(info);
                    this.m_hashLookup.Add(info.TypeHash, info);
                    this.m_typeLookup.Add(type, info);
                    this.m_staticEventTable.AddStaticEvents(type);
                }
                else if (IsSerializableClass(type))
                {
                    info = new MySynchronizedTypeInfo(type, new TypeId((uint) this.m_idToType.Count), baseType, isReplicated);
                    this.m_idToType.Add(info);
                    this.m_hashLookup.Add(info.TypeHash, info);
                    this.m_typeLookup.Add(type, info);
                }
                else if (baseType == null)
                {
                    info = null;
                }
                else
                {
                    info = baseType;
                    this.m_typeLookup.Add(type, info);
                }
            }
            return info;
        }

        public void Serialize(BitStream stream)
        {
            if (stream.Writing)
            {
                stream.WriteVariant((uint) this.m_idToType.Count);
                for (int i = 0; i < this.m_idToType.Count; i++)
                {
                    stream.WriteInt32(this.m_idToType[i].TypeHash, 0x20);
                }
            }
            else
            {
                int num2 = (int) stream.ReadUInt32Variant();
                if (this.m_idToType.Count != num2)
                {
                    MyLog.Default.WriteLine($"Bad number of types from server. Recieved {num2}, have {this.m_idToType.Count}");
                }
                this.m_staticEventTable = new MyEventTable(null);
                for (int i = 0; i < num2; i++)
                {
                    int key = stream.ReadInt32(0x20);
                    if (!this.m_hashLookup.ContainsKey(key))
                    {
                        MyLog.Default.WriteLine("Type hash not found! Value: " + key);
                    }
                    MySynchronizedTypeInfo info = this.m_hashLookup[key];
                    this.m_idToType[i] = info;
                    this.m_staticEventTable.AddStaticEvents(info.Type);
                }
            }
        }

        public static bool ShouldRegister(Type type) => 
            (IsReplicated(type) || (CanHaveEvents(type) || IsSerializableClass(type)));

        public bool TryGet(Type type, out MySynchronizedTypeInfo typeInfo) => 
            this.m_typeLookup.TryGetValue(type, out typeInfo);

        public MyEventTable StaticEventTable =>
            this.m_staticEventTable;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTypeTable.<>c <>9 = new MyTypeTable.<>c();
            public static Func<MemberInfo, bool> <>9__15_0;

            internal bool <HasEvents>b__15_0(MemberInfo s) => 
                s.HasAttribute<EventAttribute>();
        }
    }
}

