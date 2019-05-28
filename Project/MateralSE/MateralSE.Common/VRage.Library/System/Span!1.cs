namespace System
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Span<T>
    {
        private readonly int m_count;
        private readonly int m_offset;
        private readonly IList<T> m_collection;
        public Span(IList<T> collection, int offset, int? count = new int?())
        {
            this.m_offset = offset;
            this.m_collection = collection;
            this.m_count = count.GetValueOrDefault(collection.Count - offset);
        }

        public static implicit operator Span<T>(T[] array) => 
            new Span<T>(array, 0, new int?(array.Length));

        public static implicit operator Span<T>(List<T> list) => 
            new Span<T>(list, 0, new int?(list.Count));

        public int Count =>
            this.m_count;
        public T this[int index]
        {
            get => 
                this.m_collection[this.m_offset + index];
            set => 
                (this.m_collection[this.m_offset + index] = value);
        }
    }
}

