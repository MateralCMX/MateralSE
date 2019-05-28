namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Localization;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Utils;

    internal class MyHudWarning : MyHudNotification
    {
        public int RepeatInterval;
        public Func<bool> CanPlay;
        public Action Played;
        private MyWarningDetectionMethod m_warningDetectionMethod;
        private WarningState m_warningState;
        private bool m_warningDetected;
        private int m_msSinceLastStateChange;
        private int m_soundDelay;

        public MyHudWarning(MyWarningDetectionMethod detectionMethod, int priority, int repeatInterval = 0, int soundDelay = 0, int disappearTime = 0) : base(id, disappearTime, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important)
        {
            MyStringId id = new MyStringId();
            this.m_warningDetectionMethod = detectionMethod;
            this.RepeatInterval = repeatInterval;
            this.m_soundDelay = soundDelay;
            this.WarningPriority = priority;
            this.m_warningDetected = false;
        }

        public bool Update(bool isWarnedHigherPriority)
        {
            MyGuiSounds none = MyGuiSounds.None;
            MyStringId blank = MySpaceTexts.Blank;
            this.m_warningDetected = false;
            if (!isWarnedHigherPriority)
            {
                this.m_warningDetected = this.m_warningDetectionMethod(out none, out blank) && MyHudWarnings.EnableWarnings;
            }
            this.m_msSinceLastStateChange += 0x10 * MyHudWarnings.FRAMES_BETWEEN_UPDATE;
            if (!this.m_warningDetected)
            {
                MyHud.Notifications.Remove(this);
                MyHudWarnings.RemoveSound(none);
                this.m_warningState = WarningState.NOT_STARTED;
            }
            else
            {
                switch (this.m_warningState)
                {
                    case WarningState.NOT_STARTED:
                        base.Text = blank;
                        MyHud.Notifications.Add(this);
                        this.m_msSinceLastStateChange = 0;
                        this.m_warningState = WarningState.STARTED;
                        break;

                    case WarningState.STARTED:
                        if ((this.m_msSinceLastStateChange >= this.m_soundDelay) && this.CanPlay())
                        {
                            MyHudWarnings.EnqueueSound(none);
                            this.m_warningState = WarningState.PLAYED;
                            this.Played();
                        }
                        break;

                    case WarningState.PLAYED:
                        if ((this.RepeatInterval > 0) && this.CanPlay())
                        {
                            MyHud.Notifications.Remove(this);
                            MyHud.Notifications.Add(this);
                            MyHudWarnings.EnqueueSound(none);
                            this.Played();
                        }
                        break;

                    default:
                        break;
                }
            }
            return this.m_warningDetected;
        }

        public int WarningPriority { get; private set; }

        private enum WarningState
        {
            NOT_STARTED,
            STARTED,
            PLAYED
        }
    }
}

