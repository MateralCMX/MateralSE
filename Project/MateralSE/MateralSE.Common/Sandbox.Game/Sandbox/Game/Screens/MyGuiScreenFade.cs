namespace Sandbox.Game.Screens
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRageMath;

    public class MyGuiScreenFade : MyGuiScreenBase
    {
        private uint m_fadeInTimeMs;
        private uint m_fadeOutTimeMs;
        [CompilerGenerated]
        private Action<MyGuiScreenFade> Shown;

        public event Action<MyGuiScreenFade> Shown
        {
            [CompilerGenerated] add
            {
                Action<MyGuiScreenFade> shown = this.Shown;
                while (true)
                {
                    Action<MyGuiScreenFade> a = shown;
                    Action<MyGuiScreenFade> action3 = (Action<MyGuiScreenFade>) Delegate.Combine(a, value);
                    shown = Interlocked.CompareExchange<Action<MyGuiScreenFade>>(ref this.Shown, action3, a);
                    if (ReferenceEquals(shown, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiScreenFade> shown = this.Shown;
                while (true)
                {
                    Action<MyGuiScreenFade> source = shown;
                    Action<MyGuiScreenFade> action3 = (Action<MyGuiScreenFade>) Delegate.Remove(source, value);
                    shown = Interlocked.CompareExchange<Action<MyGuiScreenFade>>(ref this.Shown, action3, source);
                    if (ReferenceEquals(shown, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenFade(Color fadeColor, uint fadeInTimeMs = 0x1388, uint fadeOutTimeMs = 0x1388) : base(new Vector2?(Vector2.Zero), new Vector4?((Vector4) fadeColor), new Vector2?(Vector2.One * 10f), true, null, 0f, 0f)
        {
            this.m_fadeInTimeMs = fadeInTimeMs;
            this.m_fadeOutTimeMs = fadeOutTimeMs;
            base.m_backgroundFadeColor = fadeColor;
            base.EnabledBackgroundFade = true;
        }

        public override bool CloseScreen() => 
            base.CloseScreen();

        public override string GetFriendlyName() => 
            "Fade Screen";

        public override int GetTransitionClosingTime() => 
            ((int) this.m_fadeOutTimeMs);

        public override int GetTransitionOpeningTime() => 
            ((int) this.m_fadeInTimeMs);

        protected override void OnShow()
        {
            if (this.Shown != null)
            {
                this.Shown(this);
            }
        }
    }
}

