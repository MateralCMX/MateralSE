namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class MySwapList<T>
    {
        private List<T> m_foreground;
        private List<T> m_background;

        public MySwapList()
        {
            this.m_foreground = new List<T>();
            this.m_background = new List<T>();
        }

        public void Add(T element)
        {
            this.m_foreground.Add(element);
        }

        public void Clear()
        {
            this.m_foreground.Clear();
        }

        public List<T>.Enumerator GetEnumerator() => 
            this.m_foreground.GetEnumerator();

        public void Remove(T element)
        {
            this.m_background.Add(element);
        }

        public void Swap()
        {
            List<T> foreground = this.m_foreground;
            this.m_foreground = this.m_background;
            this.m_background = foreground;
        }

        public List<T> BackgroundList =>
            this.m_background;

        public T this[int index]
        {
            get => 
                this.m_foreground[index];
            set => 
                (this.m_foreground[index] = value);
        }
    }
}

