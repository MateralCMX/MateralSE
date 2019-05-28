namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;

    public class MyAchievement_PersonalityCrisis : MySteamAchievementBase
    {
        private const int NUMBER_OF_CHANGES_REQUIRED = 20;
        private const int MAXIMUM_TIMER = 60;
        private uint[] m_startS;
        private int m_timerIndex;

        private void MyGuiScreenWardrobeOnLookChanged()
        {
            this.m_timerIndex++;
            this.m_timerIndex = this.m_timerIndex % 20;
            if (this.m_startS[this.m_timerIndex] == uint.MaxValue)
            {
                this.m_startS[this.m_timerIndex] = (uint) MySession.Static.ElapsedPlayTime.TotalSeconds;
            }
            else
            {
                MyGuiScreenLoadInventory.LookChanged -= new MyLookChangeDelegate(this.MyGuiScreenWardrobeOnLookChanged);
                this.m_startS[this.m_timerIndex] = uint.MaxValue;
                base.NotifyAchieved();
            }
        }

        public override void SessionLoad()
        {
            if (!base.IsAchieved)
            {
                this.m_startS = new uint[20];
                for (int i = 0; i < this.m_startS.Length; i++)
                {
                    this.m_startS[i] = uint.MaxValue;
                }
                MyGuiScreenLoadInventory.LookChanged += new MyLookChangeDelegate(this.MyGuiScreenWardrobeOnLookChanged);
            }
        }

        public override void SessionUpdate()
        {
            if (!base.IsAchieved)
            {
                uint totalSeconds = (uint) MySession.Static.ElapsedPlayTime.TotalSeconds;
                for (int i = 0; i < this.m_startS.Length; i++)
                {
                    if ((this.m_startS[i] != uint.MaxValue) && ((totalSeconds - this.m_startS[i]) > 60))
                    {
                        this.m_startS[i] = uint.MaxValue;
                    }
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_PersonalityCrisis";

        public override bool NeedsUpdate =>
            (this.m_startS[this.m_timerIndex] != uint.MaxValue);
    }
}

