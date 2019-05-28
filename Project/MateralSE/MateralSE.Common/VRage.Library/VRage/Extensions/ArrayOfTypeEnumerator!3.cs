namespace VRage.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ArrayOfTypeEnumerator<T, TInner, TOfType> : IEnumerator<TOfType>, IDisposable, IEnumerator where TInner: struct, IEnumerator<T> where TOfType: T
    {
        private TInner m_inner;
        public ArrayOfTypeEnumerator(TInner enumerator)
        {
            this.m_inner = enumerator;
        }

        public unsafe ArrayOfTypeEnumerator<T, TInner, TOfType> GetEnumerator() => 
            *(((ArrayOfTypeEnumerator<T, TInner, TOfType>*) this));

        public TOfType Current =>
            ((TOfType) this.m_inner.Current);
        public void Dispose()
        {
            this.m_inner.Dispose();
        }

        object IEnumerator.Current =>
            this.m_inner.Current;
        public bool MoveNext()
        {
            while (this.m_inner.MoveNext())
            {
                if (this.m_inner.Current is TOfType)
                {
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            this.m_inner.Reset();
        }
    }
}

