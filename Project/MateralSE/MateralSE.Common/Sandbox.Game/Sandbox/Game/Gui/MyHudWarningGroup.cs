namespace Sandbox.Game.Gui
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class MyHudWarningGroup
    {
        private List<MyHudWarning> m_hudWarnings;
        private bool m_canBeTurnedOff;
        private int m_msSinceLastCuePlayed;
        private int m_highestWarnedPriority = 0x7fffffff;

        public MyHudWarningGroup(List<MyHudWarning> hudWarnings, bool canBeTurnedOff)
        {
            this.m_hudWarnings = new List<MyHudWarning>(hudWarnings);
            this.SortByPriority();
            this.m_canBeTurnedOff = canBeTurnedOff;
            this.InitLastCuePlayed();
            using (List<MyHudWarning>.Enumerator enumerator = hudWarnings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyHudWarning warning;
                    warning.CanPlay = () => (this.m_highestWarnedPriority > warning.WarningPriority) || ((this.m_msSinceLastCuePlayed > warning.RepeatInterval) && (this.m_highestWarnedPriority == warning.WarningPriority));
                    warning.Played = delegate {
                        this.m_msSinceLastCuePlayed = 0;
                        this.m_highestWarnedPriority = warning.WarningPriority;
                    };
                }
            }
        }

        public void Add(MyHudWarning hudWarning)
        {
            this.m_hudWarnings.Add(hudWarning);
            this.SortByPriority();
            this.InitLastCuePlayed();
            hudWarning.CanPlay = () => (this.m_highestWarnedPriority > hudWarning.WarningPriority) || ((this.m_msSinceLastCuePlayed > hudWarning.RepeatInterval) && (this.m_highestWarnedPriority == hudWarning.WarningPriority));
            hudWarning.Played = delegate {
                this.m_msSinceLastCuePlayed = 0;
                this.m_highestWarnedPriority = hudWarning.WarningPriority;
            };
        }

        public void Clear()
        {
            this.m_hudWarnings.Clear();
        }

        private void InitLastCuePlayed()
        {
            foreach (MyHudWarning warning in this.m_hudWarnings)
            {
                if (warning.RepeatInterval > this.m_msSinceLastCuePlayed)
                {
                    this.m_msSinceLastCuePlayed = warning.RepeatInterval;
                }
            }
        }

        public void Remove(MyHudWarning hudWarning)
        {
            this.m_hudWarnings.Remove(hudWarning);
        }

        private void SortByPriority()
        {
            this.m_hudWarnings.Sort((x, y) => x.WarningPriority.CompareTo(y.WarningPriority));
        }

        public void Update()
        {
            if (MySandboxGame.IsGameReady)
            {
                this.m_msSinceLastCuePlayed += 0x10 * MyHudWarnings.FRAMES_BETWEEN_UPDATE;
                bool isWarnedHigherPriority = false;
                using (List<MyHudWarning>.Enumerator enumerator = this.m_hudWarnings.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.Update(isWarnedHigherPriority))
                        {
                            continue;
                        }
                        isWarnedHigherPriority = true;
                    }
                }
                if (!isWarnedHigherPriority)
                {
                    this.m_highestWarnedPriority = 0x7fffffff;
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyHudWarningGroup.<>c <>9 = new MyHudWarningGroup.<>c();
            public static Comparison<MyHudWarning> <>9__10_0;

            internal int <SortByPriority>b__10_0(MyHudWarning x, MyHudWarning y) => 
                x.WarningPriority.CompareTo(y.WarningPriority);
        }
    }
}

