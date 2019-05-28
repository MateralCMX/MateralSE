namespace VRage.Stats
{
    using System;
    using System.Collections.Generic;

    internal class MyPriorityComparer : Comparer<KeyValuePair<string, MyStat>>
    {
        public override int Compare(KeyValuePair<string, MyStat> x, KeyValuePair<string, MyStat> y) => 
            -x.Value.Priority.CompareTo(y.Value.Priority);
    }
}

