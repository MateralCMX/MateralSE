namespace VRage.Game.ModAPI.Ingame.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyCommandLine
    {
        private readonly List<StringSegment> m_items = new List<StringSegment>();
        private readonly Dictionary<StringSegment, int> m_switchIndexes = new Dictionary<StringSegment, int>(StringSegmentIgnoreCaseComparer.DEFAULT);
        private readonly List<int> m_argumentIndexes = new List<int>();

        public MyCommandLine()
        {
            this.Items = new ItemCollection(this.m_items);
            this.Switches = new SwitchCollection(this.m_switchIndexes);
        }

        public string Argument(int index)
        {
            if ((index < 0) || (index >= this.m_argumentIndexes.Count))
            {
                return null;
            }
            return this.Items[this.m_argumentIndexes[index]];
        }

        public void Clear()
        {
            this.m_items.Clear();
            this.m_switchIndexes.Clear();
            this.m_argumentIndexes.Clear();
        }

        private void ParseParameter(ref TextPtr ptr)
        {
            StringSegment item = this.ParseQuoted(ref ptr);
            int count = this.Items.Count;
            this.m_items.Add(item);
            this.m_argumentIndexes.Add(count);
        }

        private StringSegment ParseQuoted(ref TextPtr ptr)
        {
            TextPtr ptr3;
            TextPtr ptr4;
            TextPtr ptr2 = ptr;
            bool flag = ptr2.Char == '"';
            if (flag)
            {
                ptr2 += 1;
            }
            for (ptr3 = ptr2; !ptr3.IsOutOfBounds(); ptr3 += 1)
            {
                if (ptr3.Char == '"')
                {
                    flag = !flag;
                }
                if (!flag && char.IsWhiteSpace(ptr3.Char))
                {
                    ptr = ptr3;
                    ptr4 = ptr3 - 1;
                    if (ptr4.Char == '"')
                    {
                        ptr3 = ptr4;
                    }
                    return new StringSegment(ptr2.Content, ptr2.Index, ptr3.Index - ptr2.Index);
                }
            }
            ptr3 = new TextPtr(ptr.Content, ptr.Content.Length);
            ptr = ptr3;
            ptr4 = ptr3 - 1;
            if (ptr4.Char == '"')
            {
                ptr3 = ptr4;
            }
            return new StringSegment(ptr2.Content, ptr2.Index, ptr3.Index - ptr2.Index);
        }

        public bool Switch(string name) => 
            this.m_switchIndexes.ContainsKey(new StringSegment(name));

        public string Switch(string name, int relativeArgument)
        {
            StringSegment segment;
            int num;
            if ((name.Length <= 0) || (name[0] != '-'))
            {
                segment = new StringSegment(name);
            }
            else
            {
                segment = new StringSegment(name, 1, name.Length - 1);
            }
            if (!this.m_switchIndexes.TryGetValue(segment, out num))
            {
                return null;
            }
            relativeArgument += 1 + num;
            for (int i = relativeArgument; i > num; i--)
            {
                if (!this.m_argumentIndexes.Contains(i))
                {
                    return null;
                }
            }
            return this.m_items[relativeArgument].ToString();
        }

        public bool TryParse(string argument)
        {
            this.Clear();
            if (string.IsNullOrEmpty(argument))
            {
                return false;
            }
            TextPtr ptr = new TextPtr(argument);
            while (true)
            {
                ptr = ptr.SkipWhitespace(false);
                if (ptr.IsOutOfBounds())
                {
                    return (this.Items.Count > 0);
                }
                if (!this.TryParseSwitch(ref ptr))
                {
                    this.ParseParameter(ref ptr);
                }
            }
        }

        private bool TryParseSwitch(ref TextPtr ptr)
        {
            if (ptr.Char != '-')
            {
                return false;
            }
            StringSegment item = this.ParseQuoted(ref ptr);
            int count = this.Items.Count;
            this.m_items.Add(item);
            this.m_switchIndexes.Add(new StringSegment(item.Text, item.Start + 1, item.Length - 1), count);
            return true;
        }

        public ItemCollection Items { get; private set; }

        public SwitchCollection Switches { get; private set; }

        public int ArgumentCount =>
            this.m_argumentIndexes.Count;

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<string>, IDisposable, IEnumerator
        {
            private List<StringSegment>.Enumerator m_enumerator;
            internal Enumerator(List<StringSegment>.Enumerator enumerator)
            {
                this.m_enumerator = enumerator;
            }

            public void Dispose()
            {
                this.m_enumerator.Dispose();
            }

            public bool MoveNext() => 
                this.m_enumerator.MoveNext();

            public string Current =>
                this.m_enumerator.Current.ToString();
            object IEnumerator.Current =>
                this.Current;
            void IEnumerator.Reset()
            {
                this.m_enumerator.Reset();
            }
        }

        public class ItemCollection : IReadOnlyList<string>, IReadOnlyCollection<string>, IEnumerable<string>, IEnumerable
        {
            private readonly List<StringSegment> m_items;

            internal ItemCollection(List<StringSegment> items)
            {
                this.m_items = items;
            }

            public MyCommandLine.Enumerator GetEnumerator() => 
                new MyCommandLine.Enumerator(this.m_items.GetEnumerator());

            IEnumerator<string> IEnumerable<string>.GetEnumerator() => 
                new MyCommandLine.Enumerator(this.m_items.GetEnumerator());

            IEnumerator IEnumerable.GetEnumerator() => 
                new MyCommandLine.Enumerator(this.m_items.GetEnumerator());

            public int Count =>
                this.m_items.Count;

            public string this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.m_items.Count))
                    {
                        return null;
                    }
                    return this.m_items[index].ToString();
                }
            }
        }

        public class SwitchCollection : IReadOnlyCollection<string>, IEnumerable<string>, IEnumerable
        {
            private readonly Dictionary<StringSegment, int> m_switches;

            internal SwitchCollection(Dictionary<StringSegment, int> switches)
            {
                this.m_switches = switches;
            }

            public MyCommandLine.SwitchEnumerator GetEnumerator() => 
                new MyCommandLine.SwitchEnumerator(this.m_switches.Keys.GetEnumerator());

            IEnumerator<string> IEnumerable<string>.GetEnumerator() => 
                new MyCommandLine.SwitchEnumerator(this.m_switches.Keys.GetEnumerator());

            IEnumerator IEnumerable.GetEnumerator() => 
                new MyCommandLine.SwitchEnumerator(this.m_switches.Keys.GetEnumerator());

            public int Count =>
                this.m_switches.Count;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct SwitchEnumerator : IEnumerator<string>, IDisposable, IEnumerator
        {
            private Dictionary<StringSegment, int>.KeyCollection.Enumerator m_enumerator;
            internal SwitchEnumerator(Dictionary<StringSegment, int>.KeyCollection.Enumerator enumerator)
            {
                this.m_enumerator = enumerator;
            }

            public void Dispose()
            {
                this.m_enumerator.Dispose();
            }

            public bool MoveNext() => 
                this.m_enumerator.MoveNext();

            public string Current =>
                this.m_enumerator.Current.ToString();
            object IEnumerator.Current =>
                this.Current;
            void IEnumerator.Reset()
            {
                this.m_enumerator.Reset();
            }
        }
    }
}

