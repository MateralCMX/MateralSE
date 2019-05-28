namespace System
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class FloatExtensions
    {
        [Conditional("DEBUG")]
        public static void AssertIsValid(this float f)
        {
        }

        public static bool IsEqual(this float f, float other, float epsilon = 0.0001f) => 
            (f - other).IsZero(epsilon);

        public static bool IsInt(this float f, float epsilon = 0.0001f) => 
            (Math.Abs((float) (f % 1f)) <= epsilon);

        public static bool IsValid(this float f) => 
            (!float.IsNaN(f) && !float.IsInfinity(f));

        public static bool IsZero(this float f, float epsilon = 0.0001f) => 
            (Math.Abs(f) < epsilon);
    }
}

