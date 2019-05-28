namespace System
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public static class DoubleExtensions
    {
        [Conditional("DEBUG")]
        public static void AssertIsValid(this double f)
        {
        }

        public static bool IsValid(this double f) => 
            (!double.IsNaN(f) && !double.IsInfinity(f));
    }
}

