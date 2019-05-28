namespace VRage.FileSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using VRage.Compression;

    public class MyZipFileProvider : IFileProvider
    {
        public readonly char[] Separators = new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar };

        public bool DirectoryExists(string path) => 
            this.TryDoZipAction<bool>(path, new Func<string, string, bool>(this.DirectoryExistsInZip), false);

        private bool DirectoryExistsInZip(string zipFile, string subpath)
        {
            using (MyZipArchive archive = MyZipArchive.OpenOnFile(zipFile, FileMode.Open, FileAccess.Read, FileShare.Read, false))
            {
                return ((subpath == string.Empty) || archive.DirectoryExists(subpath + "/"));
            }
        }

        public bool FileExists(string path) => 
            this.TryDoZipAction<bool>(path, new Func<string, string, bool>(this.FileExistsInZip), false);

        private bool FileExistsInZip(string zipFile, string subpath)
        {
            using (MyZipArchive archive = MyZipArchive.OpenOnFile(zipFile, FileMode.Open, FileAccess.Read, FileShare.Read, false))
            {
                return archive.FileExists(subpath);
            }
        }

        [IteratorStateMachine(typeof(<GetFiles>d__8))]
        public IEnumerable<string> GetFiles(string path, string filter, MySearchOption searchOption)
        {
            <GetFiles>d__8 d__1 = new <GetFiles>d__8(-2);
            d__1.<>4__this = this;
            d__1.<>3__path = path;
            d__1.<>3__filter = filter;
            d__1.<>3__searchOption = searchOption;
            return d__1;
        }

        public static bool IsZipFile(string path) => 
            !Directory.Exists(path);

        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if ((mode != FileMode.Open) || (access != FileAccess.Read))
            {
                return null;
            }
            return this.TryDoZipAction<Stream>(path, new Func<string, string, Stream>(this.TryOpen), null);
        }

        private T TryDoZipAction<T>(string path, Func<string, string, T> action, T defaultValue)
        {
            for (int i = path.Length; i >= 0; i = path.LastIndexOfAny(this.Separators, i - 1))
            {
                string str = path.Substring(0, i);
                if (File.Exists(str))
                {
                    return action(str, path.Substring(Math.Min(path.Length, i + 1)));
                }
            }
            return defaultValue;
        }

        private string TryGetSubpath(string zipFile, string subpath) => 
            subpath;

        private MyZipArchive TryGetZipArchive(string zipFile, string subpath)
        {
            MyZipArchive archive = MyZipArchive.OpenOnFile(zipFile, FileMode.Open, FileAccess.Read, FileShare.Read, false);
            try
            {
                return archive;
            }
            catch
            {
                archive.Dispose();
                return null;
            }
        }

        private Stream TryOpen(string zipFile, string subpath)
        {
            MyZipArchive objectToClose = MyZipArchive.OpenOnFile(zipFile, FileMode.Open, FileAccess.Read, FileShare.Read, false);
            try
            {
                MyStreamWrapper wrapper1;
                if (!objectToClose.FileExists(subpath))
                {
                    wrapper1 = null;
                }
                else
                {
                    wrapper1 = new MyStreamWrapper(objectToClose.GetFile(subpath).GetStream(FileMode.Open, FileAccess.Read), objectToClose);
                }
                return wrapper1;
            }
            catch
            {
                objectToClose.Dispose();
                return null;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyZipFileProvider.<>c <>9 = new MyZipFileProvider.<>c();
            public static Func<char, bool> <>9__8_0;
            public static Func<char, bool> <>9__8_1;

            internal bool <GetFiles>b__8_0(char x) => 
                (x == '\\');

            internal bool <GetFiles>b__8_1(char x) => 
                (x == '\\');
        }

        [CompilerGenerated]
        private sealed class <GetFiles>d__8 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            public MyZipFileProvider <>4__this;
            private string path;
            public string <>3__path;
            private MySearchOption searchOption;
            public MySearchOption <>3__searchOption;
            private string filter;
            public string <>3__filter;
            private MyZipArchive <zipFile>5__2;
            private string <subpath>5__3;
            private string <pattern>5__4;
            private IEnumerator<string> <>7__wrap4;

            [DebuggerHidden]
            public <GetFiles>d__8(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap4 != null)
                {
                    this.<>7__wrap4.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    MyZipFileProvider provider = this.<>4__this;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<zipFile>5__2 = provider.TryDoZipAction<MyZipArchive>(this.path, new Func<string, string, MyZipArchive>(provider.TryGetZipArchive), null);
                        this.<subpath>5__3 = "";
                        if (this.searchOption == MySearchOption.TopDirectoryOnly)
                        {
                            this.<subpath>5__3 = provider.TryDoZipAction<string>(this.path, new Func<string, string, string>(provider.TryGetSubpath), null);
                        }
                        if (this.<zipFile>5__2 == null)
                        {
                            goto TR_0003;
                        }
                        else
                        {
                            this.<pattern>5__4 = Regex.Escape(this.filter).Replace(@"\*", ".*").Replace(@"\?", ".");
                            this.<pattern>5__4 = this.<pattern>5__4 + "$";
                            this.<>7__wrap4 = this.<zipFile>5__2.FileNames.GetEnumerator();
                            this.<>1__state = -3;
                        }
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    while (true)
                    {
                        if (this.<>7__wrap4.MoveNext())
                        {
                            string current = this.<>7__wrap4.Current;
                            if ((this.searchOption == MySearchOption.TopDirectoryOnly) && (current.Count<char>((MyZipFileProvider.<>c.<>9__8_0 ?? (MyZipFileProvider.<>c.<>9__8_0 = new Func<char, bool>(this.<GetFiles>b__8_0)))) != (this.<subpath>5__3.Count<char>((MyZipFileProvider.<>c.<>9__8_1 ?? (MyZipFileProvider.<>c.<>9__8_1 = new Func<char, bool>(this.<GetFiles>b__8_1)))) + 1)))
                            {
                                continue;
                            }
                            if (!Regex.IsMatch(current, this.<pattern>5__4, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                            {
                                continue;
                            }
                            this.<>2__current = Path.Combine(this.<zipFile>5__2.ZipPath, current);
                            this.<>1__state = 1;
                            return true;
                        }
                        else
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap4 = null;
                            this.<zipFile>5__2.Dispose();
                            this.<pattern>5__4 = null;
                        }
                        break;
                    }
                TR_0003:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                MyZipFileProvider.<GetFiles>d__8 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new MyZipFileProvider.<GetFiles>d__8(0) {
                        <>4__this = this.<>4__this
                    };
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.path = this.<>3__path;
                d__.filter = this.<>3__filter;
                d__.searchOption = this.<>3__searchOption;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            string IEnumerator<string>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

