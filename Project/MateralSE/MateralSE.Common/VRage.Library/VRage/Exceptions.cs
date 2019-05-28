namespace VRage
{
    using System;
    using System.Diagnostics;

    public static class Exceptions
    {
        [DebuggerStepThrough]
        public static void ThrowAll<TException>(bool[] conditions, params object[] args) where TException: Exception
        {
            for (uint i = 0; i < conditions.Length; i++)
            {
                if (!conditions[i])
                {
                    return;
                }
            }
            throw ((Exception) Activator.CreateInstance(typeof(TException), args));
        }

        [DebuggerStepThrough]
        public static void ThrowAny<TException>(bool[] conditions, params object[] args) where TException: Exception
        {
            for (uint i = 0; i < conditions.Length; i++)
            {
                if (conditions[i])
                {
                    throw ((Exception) Activator.CreateInstance(typeof(TException), args));
                }
            }
        }

        [DebuggerStepThrough]
        public static void ThrowIf<TException>(bool condition) where TException: Exception
        {
            if (condition)
            {
                throw ((Exception) Activator.CreateInstance(typeof(TException)));
            }
        }

        [DebuggerStepThrough]
        public static void ThrowIf<TException>(bool condition, string arg1) where TException: Exception
        {
            if (condition)
            {
                object[] args = new object[] { arg1 };
                throw ((Exception) Activator.CreateInstance(typeof(TException), args));
            }
        }

        [DebuggerStepThrough]
        public static void ThrowIf<TException>(bool condition, params object[] args) where TException: Exception
        {
            if (condition)
            {
                throw ((Exception) Activator.CreateInstance(typeof(TException), args));
            }
        }

        [DebuggerStepThrough]
        public static void ThrowIf<TException>(bool condition, string arg1, string arg2) where TException: Exception
        {
            if (condition)
            {
                object[] args = new object[] { arg1, arg2 };
                throw ((Exception) Activator.CreateInstance(typeof(TException), args));
            }
        }
    }
}

