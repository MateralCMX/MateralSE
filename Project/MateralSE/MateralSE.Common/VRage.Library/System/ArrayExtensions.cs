namespace System
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Extensions;
    using VRage.Library.Collections;

    public static class ArrayExtensions
    {
        public static int BinaryIntervalSearch<T>(this T[] self, T value) where T: IComparable<T>
        {
            if (self.Length == 0)
            {
                return 0;
            }
            if (self.Length == 1)
            {
                return ((value.CompareTo(self[0]) >= 0) ? 1 : 0);
            }
            int index = 0;
            int length = self.Length;
            while ((length - index) > 1)
            {
                int num = (index + length) / 2;
                if (value.CompareTo(self[num]) >= 0)
                {
                    index = num;
                    continue;
                }
                length = num;
            }
            int num4 = index;
            if (value.CompareTo(self[index]) >= 0)
            {
                num4 = length;
            }
            return num4;
        }

        public static int BinaryIntervalSearch<T>(this T[] self, Func<T, int> selector)
        {
            if (self.Length == 0)
            {
                return 0;
            }
            if (self.Length == 1)
            {
                return ((selector(self[0]) >= 0) ? 1 : 0);
            }
            int index = 0;
            int length = self.Length;
            while ((length - index) > 1)
            {
                int num = (index + length) / 2;
                if (selector(self[num]) >= 0)
                {
                    index = num;
                    continue;
                }
                length = num;
            }
            int num4 = index;
            if (selector(self[index]) >= 0)
            {
                num4 = length;
            }
            return num4;
        }

        public static unsafe bool Compare(this byte[] a1, byte[] a2)
        {
            byte* numPtr;
            byte* numPtr2;
            byte[] pinned buffer;
            byte[] pinned buffer2;
            if (((a1 == null) || (a2 == null)) || (a1.Length != a2.Length))
            {
                return false;
            }
            if (((buffer = a1) == null) || (buffer.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer;
            }
            if (((buffer2 = a2) == null) || (buffer2.Length == 0))
            {
                numPtr2 = null;
            }
            else
            {
                numPtr2 = buffer2;
            }
            byte* numPtr3 = numPtr;
            byte* numPtr4 = numPtr2;
            int length = a1.Length;
            int num2 = 0;
            while (num2 < (length / 8))
            {
                if (*(((long*) numPtr3)) != *(((long*) numPtr4)))
                {
                    return false;
                }
                num2++;
                numPtr3 += 8;
                numPtr4 += 8;
            }
            if ((length & 4) != 0)
            {
                if (*(((int*) numPtr3)) != *(((int*) numPtr4)))
                {
                    return false;
                }
                numPtr3 += 4;
                numPtr4 += 4;
            }
            if ((length & 2) != 0)
            {
                if (*(((short*) numPtr3)) != *(((short*) numPtr4)))
                {
                    return false;
                }
                numPtr3 += 2;
                numPtr4 += 2;
            }
            return (((length & 1) == 0) || (numPtr3[0] == numPtr4[0]));
        }

        public static bool Contains<T>(this T[] array, T element)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < array.Length; i++)
            {
                if (comparer.Equals(array[i], element))
                {
                    return true;
                }
            }
            return false;
        }

        public static T[] CreateSubarray<T>(this T[] inputArray, int theFirstElement, int elementsCount)
        {
            int num = theFirstElement + elementsCount;
            if (inputArray.Length < num)
            {
                throw new ArgumentOutOfRangeException("The requested interval for the subarray is out of the boundaries");
            }
            T[] localArray = new T[elementsCount];
            for (int i = 0; i < elementsCount; i++)
            {
                localArray[i] = inputArray[theFirstElement + i];
            }
            return localArray;
        }

        public static void EnsureCapacity<T>(ref T[] array, int size)
        {
            if ((array == null) || (array.Length < size))
            {
                Array.Resize<T>(ref array, size);
            }
        }

        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength != 0)
            {
                ArrayTraverse traverse = new ArrayTraverse(array);
                while (true)
                {
                    action(array, traverse.Position);
                    if (!traverse.Step())
                    {
                        return;
                    }
                }
            }
        }

        public static bool IsNullOrEmpty<T>(this T[] self) => 
            ((self == null) || (self.Length == 0));

        public static bool IsValidIndex<T>(this T[] self, int index) => 
            (index < self.Length);

        public static ArrayOfTypeEnumerator<TBase, ArrayEnumerator<TBase>, T> OfTypeFast<TBase, T>(this TBase[] array) where T: TBase => 
            new ArrayOfTypeEnumerator<TBase, ArrayEnumerator<TBase>, T>(new ArrayEnumerator<TBase>(array));

        public static MyRangeIterator<T>.Enumerable Range<T>(this T[] array, int start, int end) => 
            MyRangeIterator<T>.ForRange(array, start, end);

        public static T[] RemoveIndices<T>(this T[] self, IList<int> indices)
        {
            if (indices.Count >= self.Length)
            {
                return new T[0];
            }
            if (indices.Count == 0)
            {
                return self;
            }
            T[] localArray = new T[self.Length - indices.Count];
            int num = 0;
            int index = 0;
            while (index < (self.Length - indices.Count))
            {
                while (true)
                {
                    if ((num >= indices.Count) || (index != (indices[num] - num)))
                    {
                        localArray[index] = self[index + num];
                        index++;
                        break;
                    }
                    num++;
                }
            }
            return localArray;
        }

        public static Span<T> Span<T>(this T[] array, int offset, int? count = new int?()) => 
            new Span<T>(array, offset, count);

        public static bool TryGetValue<T>(this T[] self, int index, out T value)
        {
            if (index < self.Length)
            {
                value = self[index];
                return true;
            }
            value = default(T);
            return false;
        }

        public static T[] Without<T>(this T[] self, IList<int> indices) => 
            self.RemoveIndices<T>(indices);

        public static T[] Without<T>(this T[] self, int position)
        {
            T[] localArray = new T[self.Length - 1];
            for (int i = position; i < localArray.Length; i++)
            {
                localArray[i] = self[i + 1];
            }
            return localArray;
        }
    }
}

