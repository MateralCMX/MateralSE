namespace VRageMath.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyVector3DGrid<T>
    {
        private double m_cellSize;
        private double m_divisor;
        private int m_nextFreeEntry;
        private int m_count;
        private List<Entry<T>> m_storage;
        private Dictionary<Vector3I, int> m_bins;

        public MyVector3DGrid(double cellSize)
        {
            this.m_cellSize = cellSize;
            this.m_divisor = 1.0 / this.m_cellSize;
            this.m_storage = new List<Entry<T>>();
            this.m_bins = new Dictionary<Vector3I, int>();
            this.Clear();
        }

        private int AddNewEntry(ref Vector3D point, T data)
        {
            Entry<T> entry;
            this.m_count++;
            if (this.m_nextFreeEntry == this.m_storage.Count)
            {
                entry = new Entry<T> {
                    Point = point,
                    Data = data,
                    NextEntry = -1
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
                NextEntry = -1
            };
            this.m_storage[nextFreeEntry] = entry;
            return nextFreeEntry;
        }

        public void AddPoint(ref Vector3D point, T data)
        {
            int num;
            Vector3I key = Vector3I.Floor(point * this.m_divisor);
            if (!this.m_bins.TryGetValue(key, out num))
            {
                this.m_bins.Add(key, this.AddNewEntry(ref point, data));
            }
            else
            {
                Entry<T> entry = this.m_storage[num];
                for (int i = entry.NextEntry; i != -1; i = entry.NextEntry)
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
            for (int i = this.m_nextFreeEntry; (i != -1) && (i != this.m_storage.Count); i = this.m_storage[i].NextEntry)
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

        public void CollectStorage(int startingIndex, ref List<T> output)
        {
            output.Clear();
            Entry<T> entry = this.m_storage[startingIndex];
            output.Add(entry.Data);
            while (entry.NextEntry != -1)
            {
                entry = this.m_storage[entry.NextEntry];
                output.Add(entry.Data);
            }
        }

        public Dictionary<Vector3I, int>.Enumerator EnumerateBins() => 
            this.m_bins.GetEnumerator();

        public void GetLocalBinBB(ref Vector3I binPosition, out BoundingBoxD output)
        {
            output.Min = (Vector3D) (binPosition * this.m_cellSize);
            output.Max = output.Min + new Vector3D(this.m_cellSize);
        }

        public Enumerator<T> GetPointsCloserThan(ref Vector3D point, double dist) => 
            new Enumerator<T>((MyVector3DGrid<T>) this, ref point, dist);

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

        public void RemovePoint(ref Vector3D point)
        {
            int nextEntry;
            Vector3I key = Vector3I.Floor(point * this.m_divisor);
            if (this.m_bins.TryGetValue(key, out nextEntry))
            {
                int num2 = -1;
                int num3 = nextEntry;
                Entry<T> entry = new Entry<T>();
                while (true)
                {
                    if (nextEntry == -1)
                    {
                        if (num3 != -1)
                        {
                            this.m_bins[key] = num3;
                            break;
                        }
                        this.m_bins.Remove(key);
                        return;
                    }
                    Entry<T> entry2 = this.m_storage[nextEntry];
                    if (entry2.Point != point)
                    {
                        num2 = nextEntry;
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
                        this.m_storage[num2] = entry;
                    }
                    nextEntry = num4;
                }
            }
        }

        public void RemoveTwo(ref Enumerator<T> en0, ref Enumerator<T> en1)
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

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            public Vector3D Point;
            public T Data;
            public int NextEntry;
            public override string ToString()
            {
                string[] textArray1 = new string[] { this.Point.ToString(), ", -> ", this.NextEntry.ToString(), ", Data: ", this.Data.ToString() };
                return string.Concat(textArray1);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            private MyVector3DGrid<T> m_parent;
            private Vector3D m_point;
            private double m_distSq;
            private int m_previousIndex;
            private int m_storageIndex;
            private Vector3I_RangeIterator m_rangeIterator;
            public Enumerator(MyVector3DGrid<T> parent, ref Vector3D point, double dist)
            {
                this.m_parent = parent;
                this.m_point = point;
                this.m_distSq = dist * dist;
                Vector3D vectord = new Vector3D(dist);
                Vector3I start = Vector3I.Floor((point - vectord) * parent.m_divisor);
                Vector3I end = Vector3I.Floor((point + vectord) * parent.m_divisor);
                this.m_rangeIterator = new Vector3I_RangeIterator(ref start, ref end);
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
                    MyVector3DGrid<T>.Entry entry = this.m_parent.m_storage[this.m_previousIndex];
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
                    MyVector3DGrid<T>.Entry entry = this.m_parent.m_storage[this.m_storageIndex];
                    Vector3D vectord = entry.Point - this.m_point;
                    if (vectord.LengthSquared() < this.m_distSq)
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

