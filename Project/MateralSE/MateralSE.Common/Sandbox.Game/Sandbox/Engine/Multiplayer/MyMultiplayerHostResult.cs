namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Engine.Networking;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class MyMultiplayerHostResult
    {
        [CompilerGenerated]
        private Action<bool, string, MyMultiplayerBase> Done;
        private bool m_done;

        public event Action<bool, string, MyMultiplayerBase> Done
        {
            [CompilerGenerated] add
            {
                Action<bool, string, MyMultiplayerBase> done = this.Done;
                while (true)
                {
                    Action<bool, string, MyMultiplayerBase> a = done;
                    Action<bool, string, MyMultiplayerBase> action3 = (Action<bool, string, MyMultiplayerBase>) Delegate.Combine(a, value);
                    done = Interlocked.CompareExchange<Action<bool, string, MyMultiplayerBase>>(ref this.Done, action3, a);
                    if (ReferenceEquals(done, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool, string, MyMultiplayerBase> done = this.Done;
                while (true)
                {
                    Action<bool, string, MyMultiplayerBase> source = done;
                    Action<bool, string, MyMultiplayerBase> action3 = (Action<bool, string, MyMultiplayerBase>) Delegate.Remove(source, value);
                    done = Interlocked.CompareExchange<Action<bool, string, MyMultiplayerBase>>(ref this.Done, action3, source);
                    if (ReferenceEquals(done, source))
                    {
                        return;
                    }
                }
            }
        }

        public void Cancel()
        {
            this.Cancelled = true;
        }

        public void RaiseDone(bool success, string msg, MyMultiplayerBase multiplayer)
        {
            Action<bool, string, MyMultiplayerBase> done = this.Done;
            if (done != null)
            {
                done(success, msg, multiplayer);
            }
            this.m_done = true;
        }

        public void Wait(bool runCallbacks = true)
        {
            while (!this.Cancelled && !this.m_done)
            {
                if (runCallbacks)
                {
                    MyGameService.Update();
                }
                Thread.Sleep(10);
            }
        }

        public bool Cancelled { get; private set; }
    }
}

