namespace Sandbox.Engine.Utils
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public static class MyDebugWorkTrackerExtensions
    {
        public static int Average(this MyDebugWorkTracker<int> self)
        {
            long num = 0L;
            int count = self.History.Count;
            for (int i = 0; i < count; i++)
            {
                num += (long) self.History[i];
            }
            return (int) (num / ((long) count));
        }

        [Conditional("__RANDOM_UNDEFINED_PROFILING_SYMBOL__")]
        public static void Hit(this MyDebugWorkTracker<int> self)
        {
            self.Current += 1;
        }

        public static int Max(this MyDebugWorkTracker<int> self)
        {
            int num = -2147483648;
            int count = self.History.Count;
            for (int i = 0; i < count; i++)
            {
                int num4 = self.History[i];
                if (num < num4)
                {
                    num = num4;
                }
            }
            return num;
        }

        public static int Min(this MyDebugWorkTracker<int> self)
        {
            int num = 0x7fffffff;
            int count = self.History.Count;
            for (int i = 0; i < count; i++)
            {
                int num4 = self.History[i];
                if (num > num4)
                {
                    num = num4;
                }
            }
            return num;
        }

        public static Vector4I Stats(this MyDebugWorkTracker<int> self)
        {
            Vector4I vectori;
            if (self.History.Count == 0)
            {
                return new Vector4I(0, 0, 0, 0);
            }
            long num = 0L;
            int num2 = 0x7fffffff;
            int num3 = -2147483648;
            int count = self.History.Count;
            for (int i = 0; i < count; i++)
            {
                int num6 = self.History[i];
                if (num3 < num6)
                {
                    num3 = num6;
                }
                if (num2 > num6)
                {
                    num2 = num6;
                }
                num += num6;
            }
            vectori.X = self.History[count - 1];
            vectori.Y = num2;
            vectori.Z = (int) (num / ((long) count));
            vectori.W = num3;
            return vectori;
        }
    }
}

