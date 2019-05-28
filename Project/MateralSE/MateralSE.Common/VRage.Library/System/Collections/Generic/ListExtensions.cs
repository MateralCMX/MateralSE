namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public static class ListExtensions
    {
        public static void AddArray<T>(this List<T> list, T[] itemsToAdd)
        {
            list.AddArray<T>(itemsToAdd, itemsToAdd.Length);
        }

        public static void AddArray<T>(this List<T> list, T[] itemsToAdd, int itemCount)
        {
            if (list.Capacity < (list.Count + itemCount))
            {
                list.Capacity = list.Count + itemCount;
            }
            Array.Copy(itemsToAdd, 0, list.GetInternalArray<T>(), list.Count, itemCount);
            ListInternalAccessor<T>.SetSize(list, list.Count + itemCount);
        }

        public static void AddHashset<T>(this List<T> list, HashSet<T> hashset)
        {
            foreach (T local in hashset)
            {
                list.Add(local);
            }
        }

        public static void AddHashsetCasting<T1, T2>(this List<T1> list, HashSet<T2> hashset)
        {
            foreach (T2 local in hashset)
            {
                list.Add((T1) local);
            }
        }

        public static void AddList<T>(this List<T> list, List<T> itemsToAdd)
        {
            int count = itemsToAdd.Count;
            if (count != 0)
            {
                list.AddArray<T>(itemsToAdd.GetInternalArray<T>(), count);
            }
        }

        public static void AddOrInsert<T>(this List<T> list, T item, int index)
        {
            if ((index < 0) || (index > list.Count))
            {
                list.Add(item);
            }
            else
            {
                list.Insert(index, item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static void AssertEmpty<T>(this List<T> list)
        {
            if (list.Count != 0)
            {
                list.Clear();
            }
        }

        public static T AtMod<T>(this List<T> list, int index) => 
            list[index % list.Count];

        public static T AtMod<T>(this ListReader<T> list, int index) => 
            list[index % list.Count];

        public static int BinaryIntervalSearch<T>(this IList<T> self, Func<T, bool> less)
        {
            if (less == null)
            {
                throw new ArgumentNullException("less");
            }
            if (self.Count == 0)
            {
                return 0;
            }
            if (self.Count == 1)
            {
                return (less(self[0]) ? 1 : 0);
            }
            int num2 = 0;
            int count = self.Count;
            while ((count - num2) > 1)
            {
                int num = (num2 + count) / 2;
                if (less(self[num]))
                {
                    num2 = num;
                    continue;
                }
                count = num;
            }
            int num4 = num2;
            if (less(self[num2]))
            {
                num4 = count;
            }
            return num4;
        }

        public static int BinaryIntervalSearch<T>(this IList<T> self, T value, IComparer<T> comparer = null)
        {
            if (self.Count == 0)
            {
                return 0;
            }
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
            }
            if (self.Count == 1)
            {
                return ((comparer.Compare(value, self[0]) >= 0) ? 1 : 0);
            }
            int num2 = 0;
            int count = self.Count;
            while ((count - num2) > 1)
            {
                int num = (num2 + count) / 2;
                if (comparer.Compare(value, self[num]) >= 0)
                {
                    num2 = num;
                    continue;
                }
                count = num;
            }
            int num4 = num2;
            if (comparer.Compare(value, self[num2]) >= 0)
            {
                num4 = count;
            }
            return num4;
        }

        public static int BinaryIntervalSearch<T>(this IList<T> self, T value, Comparison<T> comparison)
        {
            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }
            return ((self.Count != 0) ? self.BinaryIntervalSearch<T>(value, FunctorComparer<T>.Get(comparison)) : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static void EnsureCapacity<T>(this List<T> list, int capacity)
        {
            if (list.Capacity < capacity)
            {
                list.Capacity = capacity;
            }
        }

        public static ClearToken<T> GetClearToken<T>(this List<T> list) => 
            new ClearToken<T> { List = list };

        public static T[] GetInternalArray<T>(this List<T> list) => 
            ListInternalAccessor<T>.GetArray(list);

        public static void InsertInOrder<T>(this List<T> self, T value) where T: IComparable<T>
        {
            self.InsertInOrder<T>(value, Comparer<T>.Default);
        }

        public static void InsertInOrder<T>(this List<T> self, T value, IComparer<T> comparer)
        {
            int index = self.BinarySearch(value, comparer);
            if (index < 0)
            {
                index = ~index;
            }
            self.Insert(index, value);
        }

        public static bool IsSorted<T>(this List<T> self, IComparer<T> comparer)
        {
            for (int i = 1; i < self.Count; i++)
            {
                if (comparer.Compare(self[i - 1], self[i]) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidIndex<T>(this List<T> list, int index) => 
            ((0 <= index) && (index < list.Count));

        public static T MaxBy<T>(this IEnumerable<T> source, Func<T, float> selector)
        {
            T arg = default(T);
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new Exception("No elements");
                }
                arg = enumerator.Current;
                float num = selector(arg);
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    float num2 = selector(current);
                    if (num2 > num)
                    {
                        num = num2;
                        arg = current;
                    }
                }
            }
            return arg;
        }

        public static T MinBy<T>(this IEnumerable<T> source, Func<T, float> selector) => 
            source.MaxBy<T>(x => -selector(x));

        public static void Move<T>(this List<T> list, int originalIndex, int targetIndex)
        {
            int num = Math.Sign((int) (targetIndex - originalIndex));
            if (num != 0)
            {
                T local = list[originalIndex];
                for (int i = originalIndex; i != targetIndex; i += num)
                {
                    list[i] = list[i + num];
                }
                list[targetIndex] = local;
            }
        }

        public static TValue Pop<TValue>(this List<TValue> self)
        {
            TValue local = self[self.Count - 1];
            self.RemoveAt(self.Count - 1);
            return local;
        }

        public static void RemoveAtFast<T>(this IList<T> list, int index)
        {
            int num = list.Count - 1;
            list[index] = list[num];
            list.RemoveAt(num);
        }

        public static void RemoveAtFast<T>(this List<T> list, int index)
        {
            int num = list.Count - 1;
            list[index] = list[num];
            list.RemoveAt(num);
        }

        public static void RemoveIndices<T>(this List<T> list, List<int> indices)
        {
            if (indices.Count != 0)
            {
                int num = 0;
                int num2 = indices[num];
                while (num2 < (list.Count - indices.Count))
                {
                    while (true)
                    {
                        if ((num >= indices.Count) || (num2 != (indices[num] - num)))
                        {
                            list[num2] = list[num2 + num];
                            num2++;
                            break;
                        }
                        num++;
                    }
                }
                list.RemoveRange(list.Count - indices.Count, indices.Count);
            }
        }

        public static void SetSize<T>(this List<T> list, int newSize)
        {
            ListInternalAccessor<T>.SetSize(list, newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
        public static void SortNoAlloc<T>(this List<T> list, Comparison<T> comparator)
        {
            list.Sort(FunctorComparer<T>.Get(comparator));
        }

        public static void Swap<T>(this List<T> list, int a, int b)
        {
            T local = list[a];
            list[a] = list[b];
            list[b] = local;
        }

        public static O[] ToArray<I, O>(this IList<I> collection, Func<I, O> selector)
        {
            int count = collection.Count;
            O[] localArray = new O[collection.Count];
            for (int i = 0; i < count; i++)
            {
                localArray[i] = selector(collection[i]);
            }
            return localArray;
        }

        private sealed class FunctorComparer<T> : IComparer<T>
        {
            private Comparison<T> m_comparison;
            [ThreadStatic]
            private static ListExtensions.FunctorComparer<T> m_Instance;

            public int Compare(T x, T y) => 
                this.m_comparison(x, y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ListExtensions.FunctorComparer<T> Get(Comparison<T> comparison)
            {
                ListExtensions.FunctorComparer<T> instance = ListExtensions.FunctorComparer<T>.m_Instance;
                if (instance == null)
                {
                    ListExtensions.FunctorComparer<T>.m_Instance = instance = new ListExtensions.FunctorComparer<T>();
                }
                instance.m_comparison = comparison;
                return instance;
            }
        }
    }
}

