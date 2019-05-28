namespace VRage.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class CsvParser
    {
        [IteratorStateMachine(typeof(<EnumerateTail>d__1))]
        private static IEnumerable<T> EnumerateTail<T>(IEnumerator<T> en)
        {
            <EnumerateTail>d__1<T> d__1 = new <EnumerateTail>d__1<T>(-2);
            d__1.<>3__en = en;
            return d__1;
        }

        private static Tuple<T, IEnumerable<T>> HeadAndTail<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            IEnumerator<T> en = source.GetEnumerator();
            en.MoveNext();
            return Tuple.Create<T, IEnumerable<T>>(en.Current, EnumerateTail<T>(en));
        }

        [IteratorStateMachine(typeof(<Parse>d__4))]
        public static IEnumerable<IList<string>> Parse(TextReader reader, char delimiter, char qualifier)
        {
            bool <inQuote>5__2 = false;
            List<string> <record>5__3 = new List<string>();
            StringBuilder <sb>5__4 = new StringBuilder();
            while (true)
            {
                while (true)
                {
                    if (reader.Peek() != -1)
                    {
                        char c = (char) reader.Read();
                        if ((c != '\n') && ((c != '\r') || (((ushort) reader.Peek()) != 10)))
                        {
                            if ((<sb>5__4.Length == 0) && !<inQuote>5__2)
                            {
                                if (c == qualifier)
                                {
                                    <inQuote>5__2 = true;
                                }
                                else if (c == delimiter)
                                {
                                    <record>5__3.Add(<sb>5__4.ToString());
                                    <sb>5__4.Clear();
                                }
                                else if (!char.IsWhiteSpace(c))
                                {
                                    <sb>5__4.Append(c);
                                }
                            }
                            else if (c == delimiter)
                            {
                                if (<inQuote>5__2)
                                {
                                    <sb>5__4.Append(delimiter);
                                }
                                else
                                {
                                    <record>5__3.Add(<sb>5__4.ToString());
                                    <sb>5__4.Clear();
                                }
                            }
                            else if (c != qualifier)
                            {
                                <sb>5__4.Append(c);
                            }
                            else if (!<inQuote>5__2)
                            {
                                <sb>5__4.Append(c);
                            }
                            else if (((char) reader.Peek()) != qualifier)
                            {
                                <inQuote>5__2 = false;
                            }
                            else
                            {
                                reader.Read();
                                <sb>5__4.Append(qualifier);
                            }
                            continue;
                        }
                        if (c == '\r')
                        {
                            reader.Read();
                        }
                        if (<inQuote>5__2)
                        {
                            if (c == '\r')
                            {
                                <sb>5__4.Append('\r');
                            }
                            <sb>5__4.Append('\n');
                            continue;
                        }
                        if ((<record>5__3.Count > 0) || (<sb>5__4.Length > 0))
                        {
                            <record>5__3.Add(<sb>5__4.ToString());
                            <sb>5__4.Clear();
                        }
                        if (<record>5__3.Count > 0)
                        {
                            yield return <record>5__3;
                            break;
                        }
                    }
                    else
                    {
                        if ((<record>5__3.Count > 0) || (<sb>5__4.Length > 0))
                        {
                            <record>5__3.Add(<sb>5__4.ToString());
                        }
                        if (<record>5__3.Count > 0)
                        {
                            yield return <record>5__3;
                        }
                        break;
                    }
                    break;
                }
                <record>5__3 = new List<string>(<record>5__3.Count);
            }
        }

        public static IEnumerable<IList<string>> Parse(string content, char delimiter, char qualifier)
        {
            using (StringReader reader = new StringReader(content))
            {
                return Parse(reader, delimiter, qualifier);
            }
        }

        public static Tuple<IList<string>, IEnumerable<IList<string>>> ParseHeadAndTail(TextReader reader, char delimiter, char qualifier) => 
            Parse(reader, delimiter, qualifier).HeadAndTail<IList<string>>();

        [CompilerGenerated]
        private sealed class <EnumerateTail>d__1<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private T <>2__current;
            private int <>l__initialThreadId;
            private IEnumerator<T> en;
            public IEnumerator<T> <>3__en;

            [DebuggerHidden]
            public <EnumerateTail>d__1(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                }
                if (!this.en.MoveNext())
                {
                    return false;
                }
                this.<>2__current = this.en.Current;
                this.<>1__state = 1;
                return true;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                CsvParser.<EnumerateTail>d__1<T> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new CsvParser.<EnumerateTail>d__1<T>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (CsvParser.<EnumerateTail>d__1<T>) this;
                }
                d__.en = this.<>3__en;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

    }
}

