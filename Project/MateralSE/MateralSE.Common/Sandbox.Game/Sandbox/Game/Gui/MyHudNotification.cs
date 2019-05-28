namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.ModAPI;
    using VRage.Utils;

    public class MyHudNotification : MyHudNotificationBase, IMyHudNotification
    {
        private MyStringId m_originalText;

        public MyHudNotification(MyStringId text = new MyStringId(), int disappearTimeMs = 0x9c4, string font = "Blue", MyGuiDrawAlignEnum textAlign = 4, int priority = 0, MyNotificationLevel level = 0) : base(disappearTimeMs, font, textAlign, priority, level)
        {
            this.m_originalText = text;
        }

        protected override string GetOriginalText() => 
            MyTexts.Get(this.m_originalText).ToString();

        void IMyHudNotification.Hide()
        {
            MyHud.Notifications.Remove(this);
        }

        void IMyHudNotification.ResetAliveTime()
        {
            base.ResetAliveTime();
        }

        void IMyHudNotification.Show()
        {
            MyHud.Notifications.Add(this);
        }

        string IMyHudNotification.Text
        {
            get => 
                base.GetText();
            set
            {
                object[] arguments = new object[] { value };
                base.SetTextFormatArguments(arguments);
            }
        }

        int IMyHudNotification.AliveTime
        {
            get => 
                base.m_lifespanMs;
            set
            {
                base.m_lifespanMs = value;
                base.ResetAliveTime();
            }
        }

        string IMyHudNotification.Font
        {
            get => 
                base.Font;
            set => 
                (base.Font = value);
        }

        public MyStringId Text
        {
            get => 
                this.m_originalText;
            set
            {
                if (this.m_originalText != value)
                {
                    this.m_originalText = value;
                    base.SetTextDirty();
                }
            }
        }
    }
}

