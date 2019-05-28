namespace VRageMath.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyVector3Grid<T>
    {
        private float m_cellSize;
        private float m_divisor;
        private int m_nextFreeEntry;
        private int m_count;
        private List<Entry<T>> m_storage;
        private Dictionary<Vector3I, int> m_bins;
        private IEqualityComparer<T> m_equalityComparer;

        public MyVector3Grid(float cellSize) : this(cellSize, EqualityComparer<T>.Default)
        {
        }

        public MyVector3Grid(float cellSize, IEqualityComparer<T> comparer)
        {
            this.m_cellSize = cellSize;
            this.m_divisor = 1f / this.m_cellSize;
            this.m_storage = new List<Entry<T>>();
            this.m_bins = new Dictionary<Vector3I, int>();
            this.m_equalityComparer = comparer;
            this.Clear();
        }

        private int AddNewEntry(ref Vector3 point, T data)
        {
            Entry<T> entry;
            this.m_count++;
            if (this.m_nextFreeEntry == this.m_storage.Count)
            {
                entry = new Entry<T> {
                    Point = point,
                    Data = data,
                    NextEntry = this.InvalidIndex
                };
                this.m_storage.Add(entry);
                int num = this.m_nextFreeEntry;
                this.m_nextFreeEntry = num + 1;
                return num;
            }
            if (((ulong) this.m_nextFreeEntry) > this.m_storage.Count)
            {
                return -1;
            }
            int nextFreeEntry = this.m_nextFreeEntry;
            this.m_nextFreeEntry = this.m_storage[this.m_nextFreeEntry].NextEntry;
            entry = new Entry<T> {
                Point = point,
                Data = data,
                NextEntry = this.InvalidIndex
            };
            this.m_storage[nextFreeEntry] = entry;
            return nextFreeEntry;
        }

        public void AddPoint(ref Vector3 point, T data)
        {
            int num;
            Vector3I binIndex = this.GetBinIndex(ref point);
            if (!this.m_bins.TryGetValue(binIndex, out num))
            {
                this.m_bins.Add(binIndex, this.AddNewEntry(ref point, data));
            }
            else
            {
                Entry<T> entry = this.m_storage[num];
                for (int i = entry.NextEntry; i != this.InvalidIndex; i = entry.NextEntry)
                {
                    num = i;
                    entry = this.m_storage[num];
                }
                entry.NextEntry = this.AddNewEntry(ref point, data);
                this.m_storage[num] = entry;
            }
        }

        [Conditional("DEBUG")]
        private void CheckIndexIsValid(int index)
        {
            for (int i = this.m_nextFreeEntry; (i != this.InvalidIndex) && (i != this.m_storage.Count); i = this.m_storage[i].NextEntry)
            {
            }
        }

        public void Clear()
        {
            this.m_nextFreeEntry = 0;
            this.m_count = 0;
            this.m_storage.Clear();
            this.m_bins.Clear();
        }

        public void ClearFast()
        {
            this.m_nextFreeEntry = 0;
            this.m_count = 0;
            this.m_storage.SetSize<Entry<T>>(0);
            this.m_bins.Clear();
        }

        public void CollectEntireStorage(List<T> output)
        {
            output.Clear();
            foreach (KeyValuePair<Vector3I, int> pair in this.m_bins)
            {
                int nextEntry = pair.Value;
                do
                {
                    Entry<T> entry = this.m_storage[nextEntry];
                    output.Add(entry.Data);
                    nextEntry = entry.NextEntry;
                }
                while (nextEntry != this.InvalidIndex);
            }
        }

        public void CollectStorage(int startingIndex, ref List<T> output)
        {
            output.Clear();
            Entry<T> entry = this.m_storage[startingIndex];
            output.Add(entry.Data);
            while (entry.NextEntry != this.InvalidIndex)
            {
                entry = this.m_storage[entry.NextEntry];
                output.Add(entry.Data);
            }
        }

        public Dictionary<Vector3I, int>.Enumerator EnumerateBins() => 
            this.m_bins.GetEnumerator();

        public int FindPointIndex(ref Vector3 point, T data)
        {
            Vector3I binIndex = this.GetBinIndex(ref point);
            int invalidIndex = this.InvalidIndex;
            this.m_bins.TryGetValue(binIndex, out invalidIndex);
            while (invalidIndex != this.InvalidIndex)
            {
                Entry<T> entry = this.m_storage[invalidIndex];
                if ((entry.Point == point) && this.m_equalityComparer.Equals(entry.Data, data))
                {
                    return invalidIndex;
                }
                invalidIndex = entry.NextEntry;
            }
            return invalidIndex;
        }

        private Vector3I GetBinIndex(ref Vector3 point) => 
            Vector3I.Floor(point * this.m_divisor);

        private Vector3I GetBinIndex(Vector3 point) => 
            this.GetBinIndex(ref point);

        public T GetData(int index) => 
            this.m_storage[index].Data;

        public void GetLocalBinBB(ref Vector3I binPosition, out BoundingBoxD output)
        {
            output.Min = (Vector3D) (binPosition * this.m_cellSize);
            output.Max = output.Min + new Vector3(this.m_cellSize);
        }

        public int GetNextBinIndex(int currentIndex) => 
            ((currentIndex != this.InvalidIndex) ? this.m_storage[currentIndex].NextEntry : this.InvalidIndex);

        public Vector3 GetPoint(int index) => 
            this.m_storage[index].Point;

        public void MovePoint(int index, ref Vector3 newPosition)
        {
            Entry<T> entry = this.m_storage[index];
            Vector3I binIndex = this.GetBinIndex(this.m_storage[index].Point);
            if (binIndex == this.GetBinIndex(ref newPosition))
            {
                entry.Point = newPosition;
                this.m_storage[index] = entry;
            }
            else
            {
                int num = this.m_bins[binIndex];
                if (index == num)
                {
                    int num2 = this.RemoveEntry(index);
                    if (num2 == this.InvalidIndex)
                    {
                        this.m_bins.Remove(binIndex);
                    }
                    else
                    {
                        this.m_bins[binIndex] = num2;
                    }
                }
                else
                {
                    int nextEntry;
                    for (int i = num; i != this.InvalidIndex; i = nextEntry)
                    {
                        Entry<T> entry2 = this.m_storage[i];
                        nextEntry = entry2.NextEntry;
                        if (nextEntry == index)
                        {
                            entry2.NextEntry = this.RemoveEntry(index);
                            this.m_storage[i] = entry2;
                            break;
                        }
                    }
                }
                this.AddPoint(ref newPosition, entry.Data);
            }
        }

        public SphereQuery<T> QueryPointsSphere(ref Vector3 point, float dist) => 
            new SphereQuery<T>((MyVector3Grid<T>) this, ref point, dist);

        private int RemoveEntry(int toRemove)
        {
            this.m_count--;
            Entry<T> entry = this.m_storage[toRemove];
            entry.NextEntry = this.m_nextFreeEntry;
            entry.Data = default(T);
            this.m_nextFreeEntry = toRemove;
            this.m_storage[toRemove] = entry;
            return entry.NextEntry;
        }

        public void RemovePoint(ref Vector3 point)
        {
            int nextEntry;
            Vector3I binIndex = this.GetBinIndex(ref point);
            if (this.m_bins.TryGetValue(binIndex, out nextEntry))
            {
                int invalidIndex = this.InvalidIndex;
                int num3 = nextEntry;
                Entry<T> entry = new Entry<T>();
                while (true)
                {
                    if (nextEntry == this.InvalidIndex)
                    {
                        if (num3 != this.InvalidIndex)
                        {
                            this.m_bins[binIndex] = num3;
                            break;
                        }
                        this.m_bins.Remove(binIndex);
                        return;
                    }
                    Entry<T> entry2 = this.m_storage[nextEntry];
                    if (entry2.Point != point)
                    {
                        invalidIndex = nextEntry;
                        entry = entry2;
                        nextEntry = entry2.NextEntry;
                        continue;
                    }
                    int num4 = this.RemoveEntry(nextEntry);
                    if (num3 == nextEntry)
                    {
                        num3 = num4;
                    }
                    else
                    {
                        entry.NextEntry = num4;
                        this.m_storage[invalidIndex] = entry;
                    }
                    nextEntry = num4;
                }
            }
        }

        public void RemoveTwo(ref SphereQuery<T> en0, ref SphereQuery<T> en1)
        {
            if (!(en0.CurrentBin == en1.CurrentBin))
            {
                en0.RemoveCurrent();
                en1.RemoveCurrent();
            }
            else if (en0.StorageIndex == en1.PreviousIndex)
            {
                en1.RemoveCurrent();
                en0.RemoveCurrent();
                en1 = en0;
            }
            else if (en1.StorageIndex == en0.PreviousIndex)
            {
                en0.RemoveCurrent();
                en1.RemoveCurrent();
                en0 = en1;
            }
            else if (en0.StorageIndex == en1.StorageIndex)
            {
                en0.RemoveCurrent();
                en1 = en0;
            }
            else
            {
                en0.RemoveCurrent();
                en1.RemoveCurrent();
            }
        }

        public int Count =>
            this.m_count;

        public int InvalidIndex =>
            -1;

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            public Vector3 Point;
            public T Data;
            public int NextEntry;
            public override string ToString()
            {
                string[] textArray1 = new string[] { this.Point.ToString(), ", -> ", this.NextEntry.ToString(), ", Data: ", this.Data.ToString() };
                return string.Concat(textArray1);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SphereQuery
        {
            private MyVector3Grid<T> m_parent;
            private Vector3 m_point;
            private float m_distSq;
            private int m_previousIndex;
            private int m_storageIndex;
            private Vector3I_RangeIterator m_rangeIterator;
            public SphereQuery(MyVector3Grid<T> parent, ref Vector3 point, float dist)
            {
                this.m_parent = parent;
                this.m_point = point;
                this.m_distSq = dist * dist;
                Vector3 vector = new Vector3(dist);
                Vector3I binIndex = this.m_parent.GetBinIndex(point - vector);
                Vector3I end = this.m_parent.GetBinIndex(point + vector);
                this.m_rangeIterator = new Vector3I_RangeIterator(ref binIndex, ref end);
                this.m_previousIndex = -1;
                this.m_storageIndex = -1;
            }

            public T Current =>
                this.m_parent.m_storage[this.m_storageIndex].Data;
            public Vector3I CurrentBin =>
                this.m_rangeIterator.Current;
            public int PreviousIndex =>
                this.m_previousIndex;
            public int StorageIndex =>
                this.m_storageIndex;
            public bool RemoveCurrent()
            {
                this.m_storageIndex = this.m_parent.RemoveEntry(this.m_storageIndex);
                if (this.m_previousIndex != -1)
                {
                    MyVector3Grid<T>.Entry entry = this.m_parent.m_storage[this.m_previousIndex];
                    entry.NextEntry = this.m_storageIndex;
                    this.m_parent.m_storage[this.m_previousIndex] = entry;
                }
                else if (this.m_storageIndex == -1)
                {
                    this.m_parent.m_bins.Remove(this.m_rangeIterator.Current);
                }
                else
                {
                    this.m_parent.m_bins[this.m_rangeIterator.Current] = this.m_storageIndex;
                }
                return this.FindFirstAcceptableEntry();
            }

            public bool MoveNext()
            {
                if (this.m_storageIndex != -1)
                {
                    this.m_previousIndex = this.m_storageIndex;
                    this.m_storageIndex = this.m_parent.m_storage[this.m_storageIndex].NextEntry;
                }
                else if (!this.FindNextNonemptyBin())
                {
                    return false;
                }
                return this.FindFirstAcceptableEntry();
            }

            private bool FindFirstAcceptableEntry()
            {
                while (true)
                {
                    if (this.m_storageIndex == -1)
                    {
                        this.m_rangeIterator.MoveNext();
                        if (!this.FindNextNonemptyBin())
                        {
                            return false;
                        }
                        continue;
                    }
                    MyVector3Grid<T>.Entry entry = this.m_parent.m_storage[this.m_storageIndex];
                    Vector3 vector = entry.Point - this.m_point;
                    if (vector.LengthSquared() < this.m_distSq)
                    {
                        return true;
                    }
                    this.m_previousIndex = this.m_storageIndex;
                    this.m_storageIndex = entry.NextEntry;
                }
            }

            private bool FindNextNonemptyBin()
            {
                this.m_previousIndex = -1;
                if (!this.m_rangeIterator.IsValid())
                {
                    return false;
                }
                Vector3I current = this.m_rangeIterator.Current;
                while (!this.m_parent.m_bins.TryGetValue(current, out this.m_storageIndex))
                {
                    this.m_rangeIterator.GetNext(out current);
                    if (!this.m_rangeIterator.IsValid())
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}

