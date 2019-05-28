namespace VRage
{
    using System;
    using System.Collections.Generic;

    public class MyTupleComparer<T1, T2, T3> : IEqualityComparer<MyTuple<T1, T2, T3>> where T1: IEquatable<T1> where T2: IEquatable<T2> where T3: IEquatable<T3>
    {
        public bool Equals(MyTuple<T1, T2, T3> x, MyTuple<T1, T2, T3> y) => 
            (x.Item1.Equals(y.Item1) && (x.Item2.Equals(y.Item2) && x.Item3.Equals(y.Item3)));

        public int GetHashCode(MyTuple<T1, T2, T3> obj) => 
            (((obj.Item1.GetHashCode() * 0x60000005) + (obj.Item2.GetHashCode() * 0x60000005)) + obj.Item3.GetHashCode());
    }
}

