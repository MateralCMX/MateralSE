namespace Sandbox.Game
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class MyGuiScreenProgress : MyGuiScreenProgressBase
    {
        [CompilerGenerated]
        private Action Tick;

        public event Action Tick
        {
            [CompilerGenerated] add
            {
                Action tick = this.Tick;
                while (true)
                {
                    Action a = tick;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    tick = Interlocked.CompareExchange<Action>(ref this.Tick, action3, a);
                    if (ReferenceEquals(tick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action tick = this.Tick;
                while (true)
                {
                    Action source = tick;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    tick = Interlocked.CompareExchange<Action>(ref this.Tick, action3, source);
                    if (ReferenceEquals(tick, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenProgress(StringBuilder text, MyStringId? cancelText = new MyStringId?(), bool isTopMostScreen = true, bool enableBackgroundFade = true) : base(MySpaceTexts.Blank, cancelText, isTopMostScreen, enableBackgroundFade)
        {
            this.Text = new StringBuilder(text.Length);
            this.Text.AppendStringBuilder(text);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenProgress";

        protected override void ProgressStart()
        {
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_rotatingWheel.MultipleSpinningWheels = MyPerGameSettings.GUI.MultipleSpinningWheels;
        }

        public override bool Update(bool hasFocus)
        {
            Action tick = this.Tick;
            if ((tick != null) && !base.Cancelled)
            {
                tick();
            }
            return base.Update(hasFocus);
        }

        public StringBuilder Text
        {
            get => 
                base.m_progressTextLabel.TextToDraw;
            set => 
                (base.m_progressTextLabel.TextToDraw = value);
        }
    }
}

