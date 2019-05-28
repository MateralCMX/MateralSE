namespace VRage.Utils
{
    using System;
    using System.Reflection;
    using System.Threading;

    public class MySingleProgramInstance
    {
        private Mutex m_mutex;
        private bool m_weOwn;

        public MySingleProgramInstance()
        {
            this.m_mutex = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name, out this.m_weOwn);
        }

        public MySingleProgramInstance(string identifier)
        {
            this.m_mutex = new Mutex(true, identifier, out this.m_weOwn);
        }

        public void Close()
        {
            if (this.m_weOwn)
            {
                this.m_mutex.ReleaseMutex();
                this.m_mutex.Close();
                this.m_weOwn = false;
            }
        }

        public bool IsSingleInstance =>
            this.m_weOwn;
    }
}

