namespace VRage.Sync
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public static class SyncExtensions
    {
        public static void AlwaysReject<T, TSyncDirection>(this VRage.Sync.Sync<T, TSyncDirection> sync) where TSyncDirection: SyncDirection
        {
            sync.Validate = value => false;
        }

        public static void ValidateRange<TSyncDirection>(this VRage.Sync.Sync<float, TSyncDirection> sync, Func<MyBounds> bounds) where TSyncDirection: SyncDirection
        {
            sync.Validate = delegate (float value) {
                MyBounds bounds = bounds();
                return (value >= bounds.Min) && (value <= bounds.Max);
            };
        }

        public static void ValidateRange<TSyncDirection>(this VRage.Sync.Sync<float, TSyncDirection> sync, Func<float> inclusiveMin, Func<float> inclusiveMax) where TSyncDirection: SyncDirection
        {
            sync.Validate = value => (value >= inclusiveMin()) && (value <= inclusiveMax());
        }

        public static void ValidateRange<TSyncDirection>(this VRage.Sync.Sync<float, TSyncDirection> sync, float inclusiveMin, float inclusiveMax) where TSyncDirection: SyncDirection
        {
            sync.Validate = value => (value >= inclusiveMin) && (value <= inclusiveMax);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__0<T, TSyncDirection> where TSyncDirection: SyncDirection
        {
            public static readonly SyncExtensions.<>c__0<T, TSyncDirection> <>9;
            public static SyncValidate<T> <>9__0_0;

            static <>c__0()
            {
                SyncExtensions.<>c__0<T, TSyncDirection>.<>9 = new SyncExtensions.<>c__0<T, TSyncDirection>();
            }

            internal bool <AlwaysReject>b__0_0(T value) => 
                false;
        }
    }
}

