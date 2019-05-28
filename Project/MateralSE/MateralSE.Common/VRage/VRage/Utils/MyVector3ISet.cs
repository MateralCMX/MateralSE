namespace VRage.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyVector3ISet : IEnumerable<Vector3I>, IEnumerable
    {
        private Dictionary<Vector3I, ulong> m_chunks = new Dictionary<Vector3I, ulong>();

        public MyVector3ISet()
        {
            this.Timestamp = 0;
        }

        public void Add(ref Vector3I position)
        {
            Vector3I key = new Vector3I(position.X >> 2, position.Y >> 2, position.Z >> 2);
            ulong num = 0UL;
            this.m_chunks.TryGetValue(key, out num);
            this.m_chunks[key] = num | GetMask(ref position);
            int timestamp = this.Timestamp;
            this.Timestamp = timestamp + 1;
        }

        public void Add(Vector3I position)
        {
            this.Add(ref position);
        }

        public void Clear()
        {
            this.m_chunks.Clear();
            int timestamp = this.Timestamp;
            this.Timestamp = timestamp + 1;
        }

        public bool Contains(ref Vector3I position)
        {
            ulong num = 0UL;
            Vector3I key = new Vector3I(position.X >> 2, position.Y >> 2, position.Z >> 2);
            return (this.m_chunks.TryGetValue(key, out num) && ((num & GetMask(ref position)) != 0L));
        }

        public bool Contains(Vector3I position) => 
            this.Contains(ref position);

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        private static ulong GetMask(ref Vector3I position) => 
            ((ulong) (1L << (((((position.Z & 3) << 4) + ((position.Y & 3) << 2)) + (position.X & 3)) & 0x3f)));

        public void Remove(ref Vector3I position)
        {
            Vector3I key = new Vector3I(position.X >> 2, position.Y >> 2, position.Z >> 2);
            ulong num = 0UL;
            this.m_chunks.TryGetValue(key, out num);
            num &= ~GetMask(ref position);
            if (num == 0)
            {
                this.m_chunks.Remove(key);
            }
            else
            {
                this.m_chunks[key] = num;
            }
            int timestamp = this.Timestamp;
            this.Timestamp = timestamp + 1;
        }

        public void Remove(Vector3I position)
        {
            this.Remove(ref position);
        }

        IEnumerator<Vector3I> IEnumerable<Vector3I>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public void Union(MyVector3ISet otherSet)
        {
            foreach (KeyValuePair<Vector3I, ulong> pair in otherSet.m_chunks)
            {
                ulong num = 0UL;
                this.m_chunks.TryGetValue(pair.Key, out num);
                num |= pair.Value;
                this.m_chunks[pair.Key] = num;
            }
            int timestamp = this.Timestamp;
            this.Timestamp = timestamp + 1;
        }

        private int Timestamp { get; set; }

        public bool Empty =>
            (this.m_chunks.Count == 0);

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<Vector3I>, IDisposable, IEnumerator
        {
            private Dictionary<Vector3I, ulong>.Enumerator m_dictEnum;
            private int m_shift;
            private ulong m_currentData;
            private MyVector3ISet m_parent;
            private int m_timestamp;
            public Enumerator(MyVector3ISet set)
            {
                this.m_parent = set;
                this.m_dictEnum = new Dictionary<Vector3I, ulong>.Enumerator();
                this.m_shift = 0;
                this.m_currentData = 0UL;
                this.m_timestamp = 0;
                this.Init();
            }

            private void Init()
            {
                this.m_dictEnum = this.m_parent.m_chunks.GetEnumerator();
                this.m_shift = 0x3f;
                this.m_currentData = 0UL;
                this.m_timestamp = this.m_parent.Timestamp;
            }

            public Vector3I Current
            {
                get
                {
                    int x = this.m_shift & 3;
                    return ((this.m_dictEnum.Current.Key * 4) + new Vector3I(x, (this.m_shift >> 2) & 3, this.m_shift >> 4));
                }
            }
            public bool MoveNext()
            {
                while (this.MoveNextInternal())
                {
                    if ((this.m_currentData & ((ulong) (1L << (this.m_shift & 0x3f)))) != 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool MoveNextInternal()
            {
                if (this.m_shift != 0x3f)
                {
                    this.m_shift++;
                    return true;
                }
                this.m_shift = 0;
                if (!this.m_dictEnum.MoveNext())
                {
                    return false;
                }
                this.m_currentData = this.m_dictEnum.Current.Value;
                return true;
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current =>
                this.Current;
            public void Reset()
            {
                this.Init();
            }

            [Conditional("DEBUG")]
            private void CheckTimestamp()
            {
                if (this.m_timestamp != this.m_parent.Timestamp)
                {
                    throw new InvalidOperationException("A Vector3I set collection was modified during iteration using an enumerator!");
                }
            }
        }
    }
}

