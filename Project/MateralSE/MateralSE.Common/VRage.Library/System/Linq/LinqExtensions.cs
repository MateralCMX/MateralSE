namespace System.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T local in source)
            {
                action(local);
            }
        }
    }
}

