namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class MyHudVoiceChat
    {
        [CompilerGenerated]
        private Action<bool> VisibilityChanged;

        public event Action<bool> VisibilityChanged
        {
            [CompilerGenerated] add
            {
                Action<bool> visibilityChanged = this.VisibilityChanged;
                while (true)
                {
                    Action<bool> a = visibilityChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    visibilityChanged = Interlocked.CompareExchange<Action<bool>>(ref this.VisibilityChanged, action3, a);
                    if (ReferenceEquals(visibilityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> visibilityChanged = this.VisibilityChanged;
                while (true)
                {
                    Action<bool> source = visibilityChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    visibilityChanged = Interlocked.CompareExchange<Action<bool>>(ref this.VisibilityChanged, action3, source);
                    if (ReferenceEquals(visibilityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public void Hide()
        {
            this.Visible = false;
            if (this.VisibilityChanged != null)
            {
                this.VisibilityChanged(false);
            }
        }

        public void Show()
        {
            this.Visible = true;
            if (this.VisibilityChanged != null)
            {
                this.VisibilityChanged(true);
            }
        }

        public bool Visible { get; private set; }
    }
}

