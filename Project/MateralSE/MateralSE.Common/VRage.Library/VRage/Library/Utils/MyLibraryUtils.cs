namespace VRage.Library.Utils
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class MyLibraryUtils
    {
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void AssertBlittable<T>()
        {
            try
            {
                T local = default(T);
                if (local != null)
                {
                    local = default(T);
                    GCHandle.Alloc(local, GCHandleType.Pinned).Free();
                }
            }
            catch
            {
            }
        }

        public static float DenormalizeFloat(uint value, float min, float max, int bits)
        {
            int num = (1 << (bits & 0x1f)) - 1;
            float num2 = ((float) value) / ((float) num);
            return (min + (num2 * (max - min)));
        }

        public static float DenormalizeFloatCenter(uint value, float min, float max, int bits)
        {
            int num = (1 << (bits & 0x1f)) - 2;
            float num2 = ((float) value) / ((float) num);
            return (min + (num2 * (max - min)));
        }

        public static int GetDivisionCeil(int num, int div) => 
            (((num - 1) / div) + 1);

        public static uint NormalizeFloat(float value, float min, float max, int bits)
        {
            int num = (1 << (bits & 0x1f)) - 1;
            value = (value - min) / (max - min);
            return (uint) ((value * num) + 0.5f);
        }

        public static uint NormalizeFloatCenter(float value, float min, float max, int bits)
        {
            int num = (1 << (bits & 0x1f)) - 2;
            value = (value - min) / (max - min);
            return (uint) ((value * num) + 0.5f);
        }

        public static void ThrowNonBlittable<T>()
        {
            try
            {
                T local = default(T);
                if (local == null)
                {
                    throw new InvalidOperationException("Class is never blittable");
                }
                local = default(T);
                GCHandle.Alloc(local, GCHandleType.Pinned).Free();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Type '" + typeof(T) + "' is not blittable", exception);
            }
        }
    }
}

