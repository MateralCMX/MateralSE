namespace VRage.Library.Utils
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class Disposable : IDisposable
    {
        public Disposable(bool collectStack = false)
        {
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~Disposable()
        {
            Trace.Fail("Dispose not called!", $"Dispose was not called for '{base.GetType().FullName}'");
        }
    }
}

