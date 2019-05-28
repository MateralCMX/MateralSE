namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;

    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        public bool FireEvents;

        public ObservableCollection()
        {
            this.FireEvents = true;
        }

        protected override void ClearItems()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this);
            if (this.FireEvents)
            {
                this.OnCollectionChanged(e);
            }
            base.ClearItems();
        }

        public int FindIndex(Predicate<T> match)
        {
            int num = -1;
            int num2 = 0;
            while (true)
            {
                if (num2 < base.Items.Count)
                {
                    if (!match(base.Items[num2]))
                    {
                        num2++;
                        continue;
                    }
                    num = num2;
                }
                return num;
            }
        }

        public Enumerator<T> GetEnumerator() => 
            new Enumerator<T>((VRage.Collections.ObservableCollection<T>) this);

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private VRage.Collections.ObservableCollection<T> m_collection;
            private int m_index;
            public Enumerator(VRage.Collections.ObservableCollection<T> collection)
            {
                this.m_index = -1;
                this.m_collection = collection;
            }

            public T Current =>
                this.m_collection[this.m_index];
            public void Dispose()
            {
            }

            object IEnumerator.Current =>
                this.Current;
            public bool MoveNext()
            {
                this.m_index++;
                return (this.m_index < this.m_collection.Count);
            }

            public void Reset()
            {
                this.m_index = -1;
            }
        }
    }
}

