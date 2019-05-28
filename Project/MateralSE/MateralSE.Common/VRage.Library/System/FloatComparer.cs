namespace System
{
    using System.Collections.Generic;

    public class FloatComparer : IComparer<float>
    {
        public static FloatComparer Instance = new FloatComparer();

        public int Compare(float x, float y) => 
            x.CompareTo(y);
    }
}

