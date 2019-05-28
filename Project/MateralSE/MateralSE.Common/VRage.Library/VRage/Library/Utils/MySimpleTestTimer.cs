namespace VRage.Library.Utils
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MySimpleTestTimer : IDisposable
    {
        private string m_name;
        private Stopwatch m_watch;
        public MySimpleTestTimer(string name)
        {
            this.m_name = name;
            this.m_watch = new Stopwatch();
            this.m_watch.Start();
        }

        public void Dispose()
        {
            File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "perf.log"), $"{this.m_name}: {this.m_watch.ElapsedMilliseconds:N}ms
");
        }
    }
}

