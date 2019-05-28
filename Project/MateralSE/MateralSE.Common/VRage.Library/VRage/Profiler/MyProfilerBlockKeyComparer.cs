namespace VRage.Profiler
{
    using System;
    using System.Collections.Generic;

    public class MyProfilerBlockKeyComparer : IEqualityComparer<MyProfilerBlockKey>
    {
        public bool Equals(MyProfilerBlockKey x, MyProfilerBlockKey y) => 
            ((x.ParentId == y.ParentId) && ((x.Name == y.Name) && ((x.Member == y.Member) && ((x.File == y.File) && (x.Line == y.Line)))));

        public int GetHashCode(MyProfilerBlockKey obj) => 
            obj.HashCode;
    }
}

