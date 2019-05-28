namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyIntervalList
    {
        private List<int> m_list;
        private int m_count;

        public MyIntervalList()
        {
            this.m_list = new List<int>(8);
        }

        private MyIntervalList(int capacity)
        {
            this.m_list = new List<int>(capacity);
        }

        public void Add(int value)
        {
            if (value == -2147483648)
            {
                if (this.m_list.Count == 0)
                {
                    this.InsertInterval(0, value, value);
                }
                else if (this.m_list[0] == -2147483647)
                {
                    this.ExtendIntervalDown(0);
                }
                else if (this.m_list[0] != -2147483648)
                {
                    this.InsertInterval(0, value, value);
                }
            }
            else if (value == 0x7fffffff)
            {
                int i = this.m_list.Count - 2;
                if (i < 0)
                {
                    this.InsertInterval(0, value, value);
                }
                else if (this.m_list[i + 1] == 0x7ffffffe)
                {
                    this.ExtendIntervalUp(i);
                }
                else if (this.m_list[i + 1] != 0x7fffffff)
                {
                    this.InsertInterval(this.m_list.Count, value, value);
                }
            }
            else
            {
                for (int i = 0; i < this.m_list.Count; i += 2)
                {
                    if ((value + 1) < this.m_list[i])
                    {
                        this.InsertInterval(i, value, value);
                        return;
                    }
                    if ((value - 1) <= this.m_list[i + 1])
                    {
                        if ((value + 1) == this.m_list[i])
                        {
                            this.ExtendIntervalDown(i);
                            return;
                        }
                        if ((value - 1) == this.m_list[i + 1])
                        {
                            this.ExtendIntervalUp(i);
                        }
                        return;
                    }
                }
                this.InsertInterval(this.m_list.Count, value, value);
            }
        }

        public void Clear()
        {
            this.m_list.Clear();
            this.m_count = 0;
        }

        public bool Contains(int value)
        {
            for (int i = 0; i < this.m_list.Count; i += 2)
            {
                if (value < this.m_list[i])
                {
                    return false;
                }
                if (value <= this.m_list[i + 1])
                {
                    return true;
                }
            }
            return false;
        }

        private void ExtendIntervalDown(int i)
        {
            int num = i;
            this.m_list[num] -= 1;
            this.m_count++;
            if (i != 0)
            {
                this.TryMergeIntervals(i - 1, i);
            }
        }

        private void ExtendIntervalUp(int i)
        {
            int num = i + 1;
            this.m_list[num] += 1;
            this.m_count++;
            if (i < (this.m_list.Count - 2))
            {
                this.TryMergeIntervals(i + 1, i + 2);
            }
        }

        public MyIntervalList GetCopy()
        {
            MyIntervalList list = new MyIntervalList(this.m_list.Count);
            for (int i = 0; i < this.m_list.Count; i++)
            {
                list.m_list.Add(this.m_list[i]);
            }
            list.m_count = this.m_count;
            return list;
        }

        public Enumerator GetEnumerator() => 
            new Enumerator(this);

        public int IndexOf(int value)
        {
            int num = 0;
            for (int i = 0; i < this.m_list.Count; i += 2)
            {
                if (value < this.m_list[i])
                {
                    return -1;
                }
                if (value <= this.m_list[i + 1])
                {
                    return ((num + value) - this.m_list[i]);
                }
                num += (this.m_list[i + 1] - this.m_list[i]) + 1;
            }
            return -1;
        }

        private void InsertInterval(int listPosition, int min, int max)
        {
            if (listPosition == this.m_list.Count)
            {
                this.m_list.Add(min);
                this.m_list.Add(max);
                this.m_count += (max - min) + 1;
            }
            else
            {
                int num = this.m_list.Count - 2;
                this.m_list.Add(this.m_list[num]);
                this.m_list.Add(this.m_list[num + 1]);
                while (num > listPosition)
                {
                    this.m_list[num] = this.m_list[num - 2];
                    this.m_list[num + 1] = this.m_list[num - 1];
                    num -= 2;
                }
                this.m_list[num] = min;
                this.m_list[num + 1] = max;
                this.m_count += (max - min) + 1;
            }
        }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < this.m_list.Count; i += 2)
            {
                if (i != 0)
                {
                    str = str + "; ";
                }
                object[] objArray1 = new object[] { str, "<", this.m_list[i], ",", this.m_list[i + 1], ">" };
                str = string.Concat(objArray1);
            }
            return str;
        }

        private void TryMergeIntervals(int i1, int i2)
        {
            if ((this.m_list[i1] + 1) == this.m_list[i2])
            {
                for (int i = i1; i < (this.m_list.Count - 2); i++)
                {
                    this.m_list[i] = this.m_list[i + 2];
                }
                this.m_list.RemoveAt(this.m_list.Count - 1);
                this.m_list.RemoveAt(this.m_list.Count - 1);
            }
        }

        public int Count =>
            this.m_count;

        public int IntervalCount =>
            (this.m_list.Count / 2);

        public int this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.m_count))
                {
                    throw new IndexOutOfRangeException("Index " + index + " is out of range in MyIntervalList. Valid indices are in range <0, Count)");
                }
                int num = index;
                for (int i = 0; i < this.m_list.Count; i += 2)
                {
                    int num3 = (this.m_list[i + 1] - this.m_list[i]) + 1;
                    if (num < num3)
                    {
                        return (this.m_list[i] + num);
                    }
                    num -= num3;
                }
                return 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            private int m_interval;
            private int m_dist;
            private int m_lowerBound;
            private int m_upperBound;
            private MyIntervalList m_parent;
            public Enumerator(MyIntervalList parent)
            {
                this.m_interval = -1;
                this.m_dist = 0;
                this.m_lowerBound = 0;
                this.m_upperBound = 0;
                this.m_parent = parent;
            }

            public int Current =>
                (this.m_lowerBound + this.m_dist);
            public bool MoveNext()
            {
                if (this.m_interval == -1)
                {
                    return this.MoveNextInterval();
                }
                if ((this.m_lowerBound + this.m_dist) >= this.m_upperBound)
                {
                    return this.MoveNextInterval();
                }
                this.m_dist++;
                return true;
            }

            private bool MoveNextInterval()
            {
                this.m_interval++;
                if (this.m_interval >= this.m_parent.IntervalCount)
                {
                    return false;
                }
                this.m_dist = 0;
                this.m_lowerBound = this.m_parent.m_list[this.m_interval * 2];
                this.m_upperBound = this.m_parent.m_list[(this.m_interval * 2) + 1];
                return true;
            }
        }
    }
}

