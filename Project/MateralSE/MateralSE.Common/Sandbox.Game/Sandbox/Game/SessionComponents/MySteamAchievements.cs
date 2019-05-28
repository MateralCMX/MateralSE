namespace Sandbox.Game.SessionComponents
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Plugins;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x7d0)]
    public class MySteamAchievements : MySessionComponentBase
    {
        public static readonly bool OFFLINE_ACHIEVEMENT_INFO = false;
        private static readonly List<MySteamAchievementBase> m_achievements = new List<MySteamAchievementBase>();
        private static bool m_initialized = false;
        private double m_lastTimestamp;

        public override void BeforeStart()
        {
            if (m_initialized)
            {
                foreach (MySteamAchievementBase base2 in m_achievements)
                {
                    if (!base2.IsAchieved)
                    {
                        base2.SessionBeforeStart();
                    }
                }
            }
        }

        private static void Init()
        {
            if (!Game.IsDedicated && MyGameService.IsActive)
            {
                MyGameService.LoadStats();
                foreach (Type type in MyPlugins.GameAssembly.GetTypes())
                {
                    try
                    {
                        if (typeof(MySteamAchievementBase).IsAssignableFrom(type))
                        {
                            MySteamAchievementBase item = (MySteamAchievementBase) Activator.CreateInstance(type);
                            item.Init();
                            if (!item.IsAchieved)
                            {
                                item.Achieved += x => MyGameService.StoreStats();
                                m_achievements.Add(item);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        MySandboxGame.Log.WriteLine("Initialization of achivement failed: " + type.Name);
                        MySandboxGame.Log.IncreaseIndent();
                        MySandboxGame.Log.WriteLine(exception);
                        MySandboxGame.Log.DecreaseIndent();
                    }
                }
                m_initialized = true;
            }
        }

        public override void LoadData()
        {
            if (!m_initialized)
            {
                Init();
            }
            if (m_initialized)
            {
                foreach (MySteamAchievementBase base2 in m_achievements)
                {
                    if (!base2.IsAchieved)
                    {
                        base2.SessionLoad();
                    }
                }
            }
        }

        public override void SaveData()
        {
            if (m_initialized)
            {
                foreach (MySteamAchievementBase base2 in m_achievements)
                {
                    if (!base2.IsAchieved)
                    {
                        base2.SessionSave();
                    }
                }
                MyGameService.StoreStats();
            }
        }

        protected override void UnloadData()
        {
            if (m_initialized)
            {
                foreach (MySteamAchievementBase base2 in m_achievements)
                {
                    if (!base2.IsAchieved)
                    {
                        base2.SessionUnload();
                    }
                }
                MyGameService.StoreStats();
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (m_initialized)
            {
                foreach (MySteamAchievementBase base2 in m_achievements)
                {
                    if (!base2.NeedsUpdate)
                    {
                        continue;
                    }
                    if (!base2.IsAchieved)
                    {
                        base2.SessionUpdate();
                    }
                }
                if (MySession.Static.ElapsedPlayTime.Minutes > this.m_lastTimestamp)
                {
                    this.m_lastTimestamp = MySession.Static.ElapsedPlayTime.Minutes;
                    MyGameService.StoreStats();
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySteamAchievements.<>c <>9 = new MySteamAchievements.<>c();
            public static Action<MySteamAchievementBase> <>9__4_0;

            internal void <Init>b__4_0(MySteamAchievementBase x)
            {
                MyGameService.StoreStats();
            }
        }
    }
}

