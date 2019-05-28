namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Localization;
    using System;
    using System.Text;
    using VRage;

    public class MyHudScenarioInfo
    {
        private int m_livesLeft = -1;
        private int m_timeLeftMin = -1;
        private int m_timeLeftSec = -1;
        private bool m_needsRefresh = true;
        private MyHudNameValueData m_data = new MyHudNameValueData(typeof(LineEnum).GetEnumValues().Length, "Blue", "White", 0.025f, false);
        private bool m_visible;

        public MyHudScenarioInfo()
        {
            this.Reload();
        }

        public void Hide()
        {
            this.Visible = false;
        }

        public void Refresh()
        {
            this.m_needsRefresh = false;
            if (this.LivesLeft < 0)
            {
                this.Data[1].Visible = false;
            }
            else
            {
                this.Data[1].Value.Clear().AppendInt32(this.LivesLeft);
                this.Data[1].Visible = true;
            }
            if ((this.TimeLeftMin <= 0) && (this.TimeLeftSec < 0))
            {
                this.Data[0].Visible = false;
            }
            else
            {
                this.Data[0].Value.Clear().AppendInt32(this.TimeLeftMin).Append(":").AppendFormat("{0:D2}", this.TimeLeftSec);
                this.Data[0].Visible = true;
            }
            if (this.Data.GetVisibleCount() == 0)
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
            }
        }

        public void Reload()
        {
            this.m_data[1].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudScenarioInfoLivesLeft));
            this.m_data[0].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudScenarioInfoTimeLeft));
            this.m_livesLeft = -1;
            this.m_timeLeftMin = -1;
            this.m_timeLeftSec = -1;
            this.m_needsRefresh = true;
        }

        public void Show(Action<MyHudScenarioInfo> propertiesInit)
        {
            this.Refresh();
            if (propertiesInit != null)
            {
                propertiesInit(this);
            }
        }

        public int LivesLeft
        {
            get => 
                this.m_livesLeft;
            set
            {
                if (this.m_livesLeft != value)
                {
                    this.m_livesLeft = value;
                    this.m_needsRefresh = true;
                    this.Visible = true;
                }
            }
        }

        public int TimeLeftMin
        {
            get => 
                this.m_timeLeftMin;
            set
            {
                if (this.m_timeLeftMin != value)
                {
                    this.m_timeLeftMin = value;
                    this.m_needsRefresh = true;
                    this.Visible = true;
                }
            }
        }

        public int TimeLeftSec
        {
            get => 
                this.m_timeLeftSec;
            set
            {
                if (this.m_timeLeftSec != value)
                {
                    this.m_timeLeftSec = value;
                    this.m_needsRefresh = true;
                    this.Visible = true;
                }
            }
        }

        public MyHudNameValueData Data
        {
            get
            {
                if (this.m_needsRefresh)
                {
                    this.Refresh();
                }
                return this.m_data;
            }
        }

        public bool Visible
        {
            get => 
                this.m_visible;
            set => 
                (this.m_visible = value);
        }

        private enum LineEnum
        {
            TimeLeft,
            LivesLeft
        }
    }
}

