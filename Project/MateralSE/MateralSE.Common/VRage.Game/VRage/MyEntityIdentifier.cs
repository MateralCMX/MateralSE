namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    public struct MyEntityIdentifier
    {
        private const int DEFAULT_DICTIONARY_SIZE = 0x8000;
        [ThreadStatic]
        private static bool m_inEntityCreationBlock;
        [ThreadStatic]
        private static PerThreadData m_perThreadData;
        private static PerThreadData m_mainData;
        private static bool m_singleThreadClearWarnings;
        private static bool m_isSwapPrepared;
        private static bool m_isSwapped;
        private static PerThreadData m_perThreadData_Swap;
        private static long[] m_lastGeneratedIds;
        public static bool InEntityCreationBlock
        {
            get => 
                m_inEntityCreationBlock;
            set
            {
                if (value)
                {
                    Thread currentThread = Thread.CurrentThread;
                    Thread mainThread = MyUtils.MainThread;
                }
                m_inEntityCreationBlock = value;
            }
        }
        private static Dictionary<long, IMyEntity> EntityList =>
            (m_perThreadData ?? m_mainData).EntityList;
        public static void SetSingleThreadClearWarnings(bool enable)
        {
            m_singleThreadClearWarnings = enable;
        }

        public static bool SwapPerThreadData()
        {
            m_perThreadData = m_perThreadData_Swap;
            m_perThreadData_Swap = m_perThreadData;
            m_isSwapped = !m_isSwapped;
            return m_isSwapped;
        }

        public static void PrepareSwapData()
        {
            m_isSwapPrepared = true;
            m_perThreadData_Swap = new PerThreadData(0x8000);
        }

        public static void ClearSwapDataAndRestore()
        {
            if (!m_isSwapped)
            {
                m_perThreadData_Swap = null;
                m_isSwapPrepared = false;
            }
            else
            {
                m_perThreadData = m_perThreadData_Swap;
                m_perThreadData_Swap = null;
                m_isSwapped = false;
                m_isSwapPrepared = false;
            }
        }

        public static bool AllocationSuspended
        {
            get => 
                (m_perThreadData ?? m_mainData).AllocationSuspended;
            set => 
                ((m_perThreadData ?? m_mainData).AllocationSuspended = value);
        }
        static MyEntityIdentifier()
        {
            m_singleThreadClearWarnings = false;
            m_isSwapPrepared = false;
            m_isSwapped = false;
            m_perThreadData_Swap = null;
            m_lastGeneratedIds = new long[((ID_OBJECT_TYPE) MyEnum<ID_OBJECT_TYPE>.Range.Max) + ID_OBJECT_TYPE.ENTITY];
            m_mainData = new PerThreadData(0x8000);
            m_perThreadData = m_mainData;
        }

        public static void InitPerThreadStorage(int defaultCapacity)
        {
            m_perThreadData = new PerThreadData(defaultCapacity);
        }

        public static void LazyInitPerThreadStorage(int defaultCapacity)
        {
            if ((m_perThreadData == null) || ReferenceEquals(m_perThreadData, m_mainData))
            {
                m_perThreadData = new PerThreadData(defaultCapacity);
            }
        }

        public static void DestroyPerThreadStorage()
        {
            m_perThreadData = null;
        }

        public static void GetPerThreadEntities(List<IMyEntity> result)
        {
            foreach (KeyValuePair<long, IMyEntity> pair in m_perThreadData.EntityList)
            {
                result.Add(pair.Value);
            }
        }

        public static void ClearPerThreadEntities()
        {
            if (m_singleThreadClearWarnings)
            {
                bool isSwapped = m_isSwapped;
            }
            m_perThreadData.EntityList.Clear();
        }

        public static void Reset()
        {
            Array.Clear(m_lastGeneratedIds, 0, m_lastGeneratedIds.Length);
        }

        public static void MarkIdUsed(long id)
        {
            ID_OBJECT_TYPE idObjectType = GetIdObjectType(id);
            MyUtils.InterlockedMax(ref m_lastGeneratedIds[(int) idObjectType], GetIdUniqueNumber(id));
        }

        public static void AddEntityWithId(IMyEntity entity)
        {
            if (EntityList.ContainsKey(entity.EntityId))
            {
                throw new DuplicateIdException(entity, EntityList[entity.EntityId]);
            }
            EntityList.Add(entity.EntityId, entity);
        }

        public static long AllocateId(ID_OBJECT_TYPE objectType = 1, ID_ALLOCATION_METHOD generationMethod = 0)
        {
            long uniqueNumber = (generationMethod != ID_ALLOCATION_METHOD.RANDOM) ? Interlocked.Increment(ref m_lastGeneratedIds[(int) objectType]) : (MyRandom.Instance.NextLong() & 0xffffffffffffffL);
            return ConstructId(objectType, uniqueNumber);
        }

        public static ID_OBJECT_TYPE GetIdObjectType(long id) => 
            ((ID_OBJECT_TYPE) ((byte) (id >> 0x38)));

        public static long GetIdUniqueNumber(long id) => 
            (id & 0xffffffffffffffL);

        public static long ConstructIdFromString(ID_OBJECT_TYPE type, string uniqueString)
        {
            long num = uniqueString.GetHashCode64();
            return (long) (((ulong) ((((num >> 8) + num) + (num << 13)) & 0xffffffffffffffL)) | (((ulong) type) << 0x38));
        }

        public static long ConstructId(ID_OBJECT_TYPE type, long uniqueNumber) => 
            ((long) (((ulong) (uniqueNumber & 0xffffffffffffffL)) | (((ulong) type) << 0x38)));

        public static long FixObsoleteIdentityType(long id)
        {
            if ((GetIdObjectType(id) == ID_OBJECT_TYPE.NPC) || (GetIdObjectType(id) == ID_OBJECT_TYPE.SPAWN_GROUP))
            {
                long num1 = ConstructId(ID_OBJECT_TYPE.IDENTITY, GetIdUniqueNumber(id));
                id = num1;
            }
            return id;
        }

        public static void RemoveEntity(long entityId)
        {
            EntityList.Remove(entityId);
        }

        public static IMyEntity GetEntityById(long entityId, bool allowClosed = false)
        {
            IMyEntity entity;
            if (!EntityList.TryGetValue(entityId, out entity) && (m_perThreadData != null))
            {
                m_mainData.EntityList.TryGetValue(entityId, out entity);
            }
            if (((entity == null) || allowClosed) || !entity.GetTopMostParent(null).Closed)
            {
                return entity;
            }
            return null;
        }

        public static bool TryGetEntity(long entityId, out IMyEntity entity, bool allowClosed = false)
        {
            bool flag = EntityList.TryGetValue(entityId, out entity);
            if (!flag && (m_perThreadData != null))
            {
                flag = m_mainData.EntityList.TryGetValue(entityId, out entity);
            }
            if (((entity != null) && !allowClosed) && entity.GetTopMostParent(null).Closed)
            {
                entity = null;
                flag = false;
            }
            return flag;
        }

        public static bool TryGetEntity<T>(long entityId, out T entity, bool allowClosed = false) where T: class, IMyEntity
        {
            IMyEntity entity2;
            bool flag1 = TryGetEntity(entityId, out entity2, allowClosed);
            entity = entity2 as T;
            return (flag1 && (((T) entity) != null));
        }

        public static bool ExistsById(long entityId) => 
            (EntityList.ContainsKey(entityId) || ((m_perThreadData != null) && m_mainData.EntityList.ContainsKey(entityId)));

        public static void SwapRegisteredEntityId(IMyEntity entity, long oldId, long newId)
        {
            IMyEntity entity2 = EntityList[oldId];
            EntityList.Remove(oldId);
            EntityList[newId] = entity2;
        }

        public static void Clear()
        {
            EntityList.Clear();
        }
        public enum ID_ALLOCATION_METHOD : byte
        {
            RANDOM = 0,
            SERIAL_START_WITH_1 = 1
        }

        public enum ID_OBJECT_TYPE : byte
        {
            UNKNOWN = 0,
            ENTITY = 1,
            IDENTITY = 2,
            FACTION = 3,
            NPC = 4,
            SPAWN_GROUP = 5,
            ASTEROID = 6,
            PLANET = 7,
            VOXEL_PHYSICS = 8,
            PLANET_ENVIRONMENT_SECTOR = 9,
            PLANET_ENVIRONMENT_ITEM = 10,
            PLANET_VOXEL_DETAIL = 11
        }

        private class PerThreadData
        {
            public bool AllocationSuspended;
            public Dictionary<long, IMyEntity> EntityList;

            public PerThreadData(int defaultCapacity)
            {
                this.EntityList = new Dictionary<long, IMyEntity>(defaultCapacity);
            }
        }
    }
}

