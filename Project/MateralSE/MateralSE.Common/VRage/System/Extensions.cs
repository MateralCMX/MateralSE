namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Library.Collections;
    using VRage.Network;

    public static class Extensions
    {
        [ThreadStatic]
        private static List<IMyStateGroup> m_tmpStateGroupsPerThread;

        public static T FindStateGroup<T>(this IMyReplicable obj) where T: class, IMyStateGroup
        {
            T local;
            try
            {
                if (obj != null)
                {
                    obj.GetStateGroups(m_tmpStateGroups);
                    using (List<IMyStateGroup>.Enumerator enumerator = m_tmpStateGroups.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            T current = enumerator.Current as T;
                            if (current != null)
                            {
                                return current;
                            }
                        }
                    }
                    local = default(T);
                }
                else
                {
                    local = default(T);
                    local = local;
                }
            }
            finally
            {
                m_tmpStateGroups.Clear();
            }
            return local;
        }

        public static NetworkId ReadNetworkId(this BitStream stream) => 
            new NetworkId(stream.ReadUInt32Variant());

        public static TypeId ReadTypeId(this BitStream stream) => 
            new TypeId(stream.ReadUInt32Variant());

        public static void WriteNetworkId(this BitStream stream, NetworkId networkId)
        {
            stream.WriteVariant(networkId.Value);
        }

        public static void WriteTypeId(this BitStream stream, TypeId typeId)
        {
            stream.WriteVariant(typeId.Value);
        }

        private static List<IMyStateGroup> m_tmpStateGroups
        {
            get
            {
                if (m_tmpStateGroupsPerThread == null)
                {
                    m_tmpStateGroupsPerThread = new List<IMyStateGroup>();
                }
                return m_tmpStateGroupsPerThread;
            }
        }
    }
}

