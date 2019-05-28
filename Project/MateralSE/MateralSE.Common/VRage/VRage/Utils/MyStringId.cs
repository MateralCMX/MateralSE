namespace VRage.Utils
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using VRage;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyStringId : IXmlSerializable
    {
        public static readonly MyStringId NullOrEmpty;
        [ProtoMember(0x18)]
        private int m_id;
        public static readonly IdComparerType Comparer;
        private static readonly FastResourceLock m_lock;
        private static Dictionary<string, MyStringId> m_stringToId;
        private static Dictionary<MyStringId, string> m_idToString;
        private MyStringId(int id)
        {
            this.m_id = id;
        }

        public int Id =>
            this.m_id;
        public string String
        {
            get
            {
                using (m_lock.AcquireSharedUsing())
                {
                    return m_idToString[this];
                }
            }
        }
        public override string ToString() => 
            this.String;

        public override int GetHashCode() => 
            this.m_id;

        public override bool Equals(object obj) => 
            ((obj is MyStringId) && this.Equals((MyStringId) obj));

        public bool Equals(MyStringId id) => 
            (this.m_id == id.m_id);

        public static bool operator ==(MyStringId lhs, MyStringId rhs) => 
            (lhs.m_id == rhs.m_id);

        public static bool operator !=(MyStringId lhs, MyStringId rhs) => 
            (lhs.m_id != rhs.m_id);

        public static explicit operator int(MyStringId id) => 
            id.m_id;

        static MyStringId()
        {
            Comparer = new IdComparerType();
            m_lock = new FastResourceLock();
            m_stringToId = new Dictionary<string, MyStringId>(50);
            m_idToString = new Dictionary<MyStringId, string>(50, Comparer);
            NullOrEmpty = GetOrCompute("");
        }

        public static MyStringId GetOrCompute(string str)
        {
            MyStringId id;
            if (str == null)
            {
                return NullOrEmpty;
            }
            using (m_lock.AcquireSharedUsing())
            {
                if (m_stringToId.TryGetValue(str, out id))
                {
                    return id;
                }
            }
            using (m_lock.AcquireExclusiveUsing())
            {
                if (!m_stringToId.TryGetValue(str, out id))
                {
                    id = new MyStringId(m_stringToId.Count);
                    m_idToString.Add(id, str);
                    m_stringToId.Add(str, id);
                }
                return id;
            }
        }

        public static MyStringId Get(string str)
        {
            using (m_lock.AcquireSharedUsing())
            {
                return m_stringToId[str];
            }
        }

        public static bool TryGet(string str, out MyStringId id)
        {
            using (m_lock.AcquireSharedUsing())
            {
                return m_stringToId.TryGetValue(str, out id);
            }
        }

        public static MyStringId TryGet(string str)
        {
            using (m_lock.AcquireSharedUsing())
            {
                MyStringId id;
                m_stringToId.TryGetValue(str, out id);
                return id;
            }
        }

        public static bool IsKnown(MyStringId id)
        {
            using (m_lock.AcquireSharedUsing())
            {
                return m_idToString.ContainsKey(id);
            }
        }

        public XmlSchema GetSchema() => 
            null;

        public void ReadXml(XmlReader reader)
        {
            this.m_id = GetOrCompute(reader.ReadInnerXml()).Id;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(this.String);
        }
        public class IdComparerType : IComparer<MyStringId>, IEqualityComparer<MyStringId>
        {
            public int Compare(MyStringId x, MyStringId y) => 
                (x.m_id - y.m_id);

            public bool Equals(MyStringId x, MyStringId y) => 
                (x.m_id == y.m_id);

            public int GetHashCode(MyStringId obj) => 
                obj.m_id;
        }
    }
}

