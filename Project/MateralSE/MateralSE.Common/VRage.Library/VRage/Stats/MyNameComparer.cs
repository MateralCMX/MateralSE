namespace VRage.Stats
{
    using System;
    using System.Collections.Generic;

    internal class MyNameComparer : Comparer<KeyValuePair<string, MyStat>>
    {
        public override int Compare(KeyValuePair<string, MyStat> x, KeyValuePair<string, MyStat> y) => 
            x.Key.CompareTo(y.Key);
    }
}

