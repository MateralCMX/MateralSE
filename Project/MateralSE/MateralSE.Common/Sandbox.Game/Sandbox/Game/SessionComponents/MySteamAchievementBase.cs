namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Networking;
    using Sandbox.ModAPI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public abstract class MySteamAchievementBase
    {
        [CompilerGenerated]
        private Action<MySteamAchievementBase> Achieved;

        public event Action<MySteamAchievementBase> Achieved
        {
            [CompilerGenerated] add
            {
                Action<MySteamAchievementBase> achieved = this.Achieved;
                while (true)
                {
                    Action<MySteamAchievementBase> a = achieved;
                    Action<MySteamAchievementBase> action3 = (Action<MySteamAchievementBase>) Delegate.Combine(a, value);
                    achieved = Interlocked.CompareExchange<Action<MySteamAchievementBase>>(ref this.Achieved, action3, a);
                    if (ReferenceEquals(achieved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MySteamAchievementBase> achieved = this.Achieved;
                while (true)
                {
                    Action<MySteamAchievementBase> source = achieved;
                    Action<MySteamAchievementBase> action3 = (Action<MySteamAchievementBase>) Delegate.Remove(source, value);
                    achieved = Interlocked.CompareExchange<Action<MySteamAchievementBase>>(ref this.Achieved, action3, source);
                    if (ReferenceEquals(achieved, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MySteamAchievementBase()
        {
        }

        public virtual void Init()
        {
            this.IsAchieved = MyGameService.IsAchieved(this.AchievementTag);
        }

        protected void NotifyAchieved()
        {
            MyGameService.SetAchievement(this.AchievementTag);
            if (MySteamAchievements.OFFLINE_ACHIEVEMENT_INFO)
            {
                MyAPIGateway.Utilities.ShowNotification("Achievement Unlocked: " + this.AchievementTag, 0x2710, "Red");
            }
            this.IsAchieved = true;
            if (this.Achieved != null)
            {
                this.Achieved(this);
                foreach (Delegate delegate2 in this.Achieved.GetInvocationList())
                {
                    this.Achieved -= ((Action<MySteamAchievementBase>) delegate2);
                }
            }
        }

        public virtual void SessionBeforeStart()
        {
        }

        public virtual void SessionLoad()
        {
        }

        public virtual void SessionSave()
        {
        }

        public virtual void SessionUnload()
        {
        }

        public virtual void SessionUpdate()
        {
        }

        public abstract string AchievementTag { get; }

        public bool IsAchieved { get; protected set; }

        public abstract bool NeedsUpdate { get; }
    }
}

