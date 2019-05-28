namespace VRage.Utils
{
    using System;
    using System.Diagnostics;
    using VRageMath;

    public class MyDebug
    {
        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void AssertDebug(bool condition)
        {
        }

        [Conditional("DEBUG"), DebuggerStepThrough]
        public static void AssertDebug(bool condition, string assertMessage)
        {
        }

        public static void AssertIsValid(Vector3? vec)
        {
        }

        public static void AssertIsValid(Vector3D? vec)
        {
        }

        public static void AssertIsValid(float f)
        {
        }

        public static void AssertIsValid(Matrix matrix)
        {
        }

        public static void AssertIsValid(Quaternion q)
        {
        }

        public static void AssertIsValid(Vector2 vec)
        {
        }

        public static void AssertIsValid(Vector3 vec)
        {
        }

        public static void AssertIsValid(Vector3D vec)
        {
        }

        public static void AssertRelease(bool condition)
        {
            AssertRelease(condition, "Assertion failed");
        }

        public static void AssertRelease(bool condition, string assertMessage)
        {
            if (!condition)
            {
                MyLog.Default.WriteLine("Assert: " + assertMessage);
                Trace.Fail(assertMessage);
            }
        }

        public static void FailRelease(string message)
        {
            MyLog.Default.WriteLine("Assert Fail: " + message);
            Trace.Fail(message);
        }

        public static void FailRelease(string format, params object[] args)
        {
            string message = string.Format(format, args);
            MyLog.Default.WriteLine("Assert Fail: " + message);
            Trace.Fail(message);
        }

        public static bool IsValid(double d) => 
            (!double.IsNaN(d) && !double.IsInfinity(d));

        public static bool IsValid(Vector3? vec) => 
            ((vec == null) || (IsValid(vec.Value.X) && (IsValid(vec.Value.Y) && IsValid(vec.Value.Z))));

        public static bool IsValid(Vector3D? vec) => 
            ((vec == null) || (IsValid(vec.Value.X) && (IsValid(vec.Value.Y) && IsValid(vec.Value.Z))));

        public static bool IsValid(float f) => 
            (!float.IsNaN(f) && !float.IsInfinity(f));

        public static bool IsValid(Matrix matrix) => 
            (IsValid(matrix.Up) && (IsValid(matrix.Left) && (IsValid(matrix.Forward) && (IsValid(matrix.Translation) && (matrix != Matrix.Zero)))));

        public static bool IsValid(Quaternion q) => 
            (IsValid(q.X) && (IsValid(q.Y) && (IsValid(q.Z) && (IsValid(q.W) && !MyUtils.IsZero(q, 1E-05f)))));

        public static bool IsValid(Vector2 vec) => 
            (IsValid(vec.X) && IsValid(vec.Y));

        public static bool IsValid(Vector3 vec) => 
            (IsValid(vec.X) && (IsValid(vec.Y) && IsValid(vec.Z)));

        public static bool IsValid(Vector3D vec) => 
            (IsValid(vec.X) && (IsValid(vec.Y) && IsValid(vec.Z)));

        public static bool IsValidNormal(Vector3 vec)
        {
            float num = vec.LengthSquared();
            return (IsValid(vec) && ((num > 0.999f) && (num < 1.001f)));
        }
    }
}

