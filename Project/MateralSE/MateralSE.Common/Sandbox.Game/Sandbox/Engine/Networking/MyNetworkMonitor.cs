namespace Sandbox.Engine.Networking
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class MyNetworkMonitor
    {
        [CompilerGenerated]
        private static Action OnTick;
        private static bool m_disposed = true;
        private static Thread m_workerThread;

        public static  event Action OnTick
        {
            [CompilerGenerated] add
            {
                Action onTick = OnTick;
                while (true)
                {
                    Action a = onTick;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onTick = Interlocked.CompareExchange<Action>(ref OnTick, action3, a);
                    if (ReferenceEquals(onTick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onTick = OnTick;
                while (true)
                {
                    Action source = onTick;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onTick = Interlocked.CompareExchange<Action>(ref OnTick, action3, source);
                    if (ReferenceEquals(onTick, source))
                    {
                        return;
                    }
                }
            }
        }

        public static void Dispose()
        {
            m_disposed = true;
            m_workerThread.Join();
        }

        public static void Init()
        {
            if (m_disposed)
            {
                m_disposed = false;
                Thread thread1 = new Thread(new ThreadStart(MyNetworkMonitor.Worker));
                thread1.CurrentCulture = CultureInfo.InvariantCulture;
                thread1.CurrentUICulture = CultureInfo.InvariantCulture;
                thread1.Name = "Network Monitor";
                m_workerThread = thread1;
                m_workerThread.Start();
            }
        }

        private static void Worker()
        {
            while (!m_disposed)
            {
                Thread.Sleep(0x10);
                MyNetworkWriter.SendAll();
                MyGameService.ServerUpdate();
                MyNetworkReader.ReceiveAll();
                Action onTick = OnTick;
                if (onTick != null)
                {
                    onTick();
                }
            }
        }
    }
}

