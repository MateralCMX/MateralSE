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
    public struct MyStringHash : IEquatable<MyStringHash>, IXmlSerializable
    {
        public static readonly MyStringHash NullOrEmpty;
        [ProtoMember(0x1b)]
        private int m_hash;
        public static readonly HashComparerType Comparer;
        private static readonly FastResourceLock m_lock;
        private static Dictionary<string, MyStringHash> m_stringToHash;
        private static Dictionary<MyStringHash, string> m_hashToString;
        private MyStringHash(int hash)
        {
            this.m_hash = hash;
        }

        public string String
        {
            get
            {
                using (m_lock.AcquireSharedUsing())
                {
                    return m_hashToString[this];
                }
            }
        }
        public override string ToString() => 
            this.String;

        public override int GetHashCode() => 
            this.m_hash;

        public override bool Equals(object obj) => 
            ((obj is MyStringHash) && this.Equals((MyStringHash) obj));

        public bool Equals(MyStringHash id) => 
            (this.m_hash == id.m_hash);

        public static bool operator ==(MyStringHash lhs, MyStringHash rhs) => 
            (lhs.m_hash == rhs.m_hash);

        public static bool operator !=(MyStringHash lhs, MyStringHash rhs) => 
            (lhs.m_hash != rhs.m_hash);

        public static explicit operator int(MyStringHash id) => 
            id.m_hash;

        static MyStringHash()
        {
            Comparer = new HashComparerType();
            m_lock = new FastResourceLock();
            m_stringToHash = new Dictionary<string, MyStringHash>(50);
            m_hashToString = new Dictionary<MyStringHash, string>(50, Comparer);
            NullOrEmpty = GetOrCompute("");
        }

        public static MyStringHash GetOrCompute(string str)
        {
            MyStringHash hash;
            if (str == null)
            {
                return NullOrEmpty;
            }
            using (m_lock.AcquireSharedUsing())
            {
                if (m_stringToHash.TryGetValue(str, out hash))
                {
                    return hash;
                }
            }
            using (m_lock.AcquireExclusiveUsing())
            {
                if (!m_stringToHash.TryGetValue(str, out hash))
                {
                    hash = new MyStringHash(MyUtils.GetHash(str, 0));
                    m_hashToString.Add(hash, str);
                    m_stringToHash.Add(str, hash);
                }
                return hash;
            }
        }

        public static MyStringHash Get(string str)
        {
            using (m_lock.AcquireSharedUsing())
            {
                return m_stringToHash[str];
            }
        }

        public static bool TryGet(string str, out MyStringHash id)
        {
            using (m_lock.AcquireSharedUsing())
            {
                return m_stringToHash.TryGetValue(str, out id);
            }
        }

        public static MyStringHash TryGet(string str)
        {
            using (m_lock.AcquireSharedUsing())
            {
                MyStringHash hash;
                m_stringToHash.TryGetValue(str, out hash);
                return hash;
            }
        }

        public static MyStringHash TryGet(int id)
        {
            using (m_lock.AcquireSharedUsing())
            {
                MyStringHash key = new MyStringHash(id);
                return (!m_hashToString.ContainsKey(key) ? NullOrEmpty : key);
            }
        }

        public static bool IsKnown(MyStringHash id)
        {
            using (m_lock.AcquireSharedUsing())
            {
                return m_hashToString.ContainsKey(id);
            }
        }

        public XmlSchema GetSchema() => 
            null;

        public void ReadXml(XmlReader reader)
        {
            this.m_hash = GetOrCompute(reader.ReadInnerXml()).m_hash;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(this.String);
        }
        public class HashComparerType : IComparer<MyStringHash>, IEqualityComparer<MyStringHash>
        {
            public int Compare(MyStringHash x, MyStringHash y) => 
                (x.m_hash - y.m_hash);

            public bool Equals(MyStringHash x, MyStringHash y) => 
                (x.m_hash == y.m_hash);

            public int GetHashCode(MyStringHash obj) => 
                obj.m_hash;
        }
    }
}

