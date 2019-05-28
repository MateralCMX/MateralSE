namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public abstract class MyHudNotificationBase
    {
        public static readonly int INFINITE;
        private int m_formatArgsCount;
        private object[] m_textFormatArguments = new object[20];
        private MyGuiDrawAlignEnum m_actualTextAlign;
        private int m_aliveTime;
        private string m_notificationText;
        private bool m_isTextDirty;
        public int m_lifespanMs;
        public MyNotificationLevel Level;
        public readonly int Priority;
        public string Font;
        public bool HasFog;

        public MyHudNotificationBase(int disapearTimeMs, string font = "Blue", MyGuiDrawAlignEnum textAlign = 4, int priority = 0, MyNotificationLevel level = 0)
        {
            this.Font = font;
            this.Priority = priority;
            this.HasFog = false;
            this.m_isTextDirty = true;
            this.m_actualTextAlign = textAlign;
            this.AssignFormatArgs(null);
            this.Level = level;
            this.m_lifespanMs = disapearTimeMs;
            this.m_aliveTime = 0;
            this.RefreshAlive();
        }

        public void AddAliveTime(int timeStep)
        {
            this.m_aliveTime += timeStep;
            this.RefreshAlive();
        }

        private void AssignFormatArgs(object[] args)
        {
            int index = 0;
            this.m_formatArgsCount = 0;
            if (args != null)
            {
                if (this.m_textFormatArguments.Length < args.Length)
                {
                    this.m_textFormatArguments = new object[args.Length];
                }
                while (true)
                {
                    if (index >= args.Length)
                    {
                        this.m_formatArgsCount = args.Length;
                        break;
                    }
                    this.m_textFormatArguments[index] = args[index];
                    index++;
                }
            }
            while (index < this.m_textFormatArguments.Length)
            {
                this.m_textFormatArguments[index] = "<missing>";
                index++;
            }
        }

        public virtual void BeforeAdd()
        {
        }

        public virtual void BeforeRemove()
        {
        }

        protected abstract string GetOriginalText();
        public string GetText()
        {
            if (string.IsNullOrEmpty(this.m_notificationText) || this.m_isTextDirty)
            {
                this.m_notificationText = (this.m_formatArgsCount <= 0) ? this.GetOriginalText() : string.Format(this.GetOriginalText(), this.m_textFormatArguments);
                this.m_isTextDirty = false;
            }
            return this.m_notificationText;
        }

        public object[] GetTextFormatArguments() => 
            this.m_textFormatArguments;

        private void RefreshAlive()
        {
            this.Alive = (this.m_lifespanMs == INFINITE) || (this.m_aliveTime < this.m_lifespanMs);
        }

        public void ResetAliveTime()
        {
            this.m_aliveTime = 0;
            this.RefreshAlive();
        }

        public void SetTextDirty()
        {
            this.m_isTextDirty = true;
        }

        public void SetTextFormatArguments(params object[] arguments)
        {
            this.AssignFormatArgs(arguments);
            this.m_notificationText = null;
            this.GetText();
        }

        public bool IsControlsHint =>
            (this.Level == MyNotificationLevel.Control);

        public bool Alive { get; private set; }
    }
}

