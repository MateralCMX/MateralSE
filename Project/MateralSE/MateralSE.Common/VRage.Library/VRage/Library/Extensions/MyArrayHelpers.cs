namespace VRage.Library.Extensions
{
    using SharpDX;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library;

    public static class MyArrayHelpers
    {
        public static unsafe NativeArray CloneUnsafe(NativeArray cloneFrom)
        {
            NativeArray array = new NativeArray(cloneFrom.Size);
            Buffer.MemoryCopy(cloneFrom.Ptr.ToPointer(), array.Ptr.ToPointer(), (long) array.Size, (long) cloneFrom.Size);
            return array;
        }

        public static TElement[] CloneUnsafe<TElement>(TElement[] cloneFrom) where TElement: struct
        {
            int size = Utilities.SizeOf<TElement>(cloneFrom);
            TElement[] localArray = new TElement[cloneFrom.Length];
            Utilities.Pin<TElement>(localArray, delegate (IntPtr targPtr) {
                Utilities.Pin<TElement>(cloneFrom, (Action<IntPtr>) (srcPtr => Utilities.CopyMemory(targPtr, srcPtr, size)));
            });
            return localArray;
        }

        public static void InitOrReserve<T>(ref T[] array, int size, int threshold = 0x400, float allocScale = 1.5f)
        {
            if (array == null)
            {
                array = new T[size];
            }
            else
            {
                Reserve<T>(ref array, size, threshold, allocScale);
            }
        }

        public static void InitOrReserveNoCopy<T>(ref T[] array, int size, int threshold = 0x400, float allocScale = 1.5f)
        {
            if (array == null)
            {
                array = new T[size];
            }
            else
            {
                ReserveNoCopy<T>(ref array, size, threshold, allocScale);
            }
        }

        public static void Reserve<T>(ref T[] array, int size, int threshold = 0x400, float allocScale = 1.5f)
        {
            if (array.Length < size)
            {
                int num = (size == 0) ? 1 : size;
                Array.Resize<T>(ref array, (num < threshold) ? (num * 2) : ((int) (num * allocScale)));
            }
        }

        public static void ReserveNoCopy<T>(ref T[] array, int size, int threshold = 0x400, float allocScale = 1.5f)
        {
            if (array.Length < size)
            {
                int num = (size == 0) ? 1 : size;
                array = new T[(num < threshold) ? (num * 2) : ((int) (num * allocScale))];
            }
        }

        public static void ResizeNoCopy<T>(ref T[] array, int newSize)
        {
            if ((array == null) || (array.Length != newSize))
            {
                array = new T[newSize];
            }
        }
    }
}

