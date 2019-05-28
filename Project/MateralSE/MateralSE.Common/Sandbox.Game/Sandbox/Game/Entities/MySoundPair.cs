namespace Sandbox.Game.Entities
{
    using Sandbox;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Audio;
    using VRage.Utils;

    public class MySoundPair
    {
        public static MySoundPair Empty = new MySoundPair();
        [ThreadStatic]
        private static StringBuilder m_cache;
        private MyCueId m_arcade;
        private MyCueId m_realistic;

        public MySoundPair()
        {
            this.Init(null, true);
        }

        public MySoundPair(string cueName, bool useLog = true)
        {
            this.Init(cueName, useLog);
        }

        public override bool Equals(object obj) => 
            (!(obj is MySoundPair) ? base.Equals(obj) : ((this.Arcade == (obj as MySoundPair).Arcade) && (this.Realistic == (obj as MySoundPair).Realistic)));

        public static MyCueId GetCueId(string cueName)
        {
            if (string.IsNullOrEmpty(cueName))
            {
                return new MyCueId(MyStringHash.NullOrEmpty);
            }
            MyCueId cueId = MyAudio.Static.GetCueId(cueName);
            if (cueId.Hash != MyStringHash.NullOrEmpty)
            {
                return cueId;
            }
            Cache.Clear();
            if (!MySession.Static.Settings.RealisticSound || !MyFakes.ENABLE_NEW_SOUNDS)
            {
                Cache.Append("Arc").Append(cueName);
                return MyAudio.Static.GetCueId(Cache.ToString());
            }
            Cache.Append("Real").Append(cueName);
            return MyAudio.Static.GetCueId(Cache.ToString());
        }

        public override int GetHashCode() => 
            base.GetHashCode();

        public void Init(MyCueId cueId)
        {
            if (Game.IsDedicated)
            {
                this.m_arcade = new MyCueId(MyStringHash.NullOrEmpty);
                this.m_realistic = new MyCueId(MyStringHash.NullOrEmpty);
            }
            else if (!MySession.Static.Settings.RealisticSound || !MyFakes.ENABLE_NEW_SOUNDS)
            {
                this.m_arcade = cueId;
                this.m_realistic = new MyCueId(MyStringHash.NullOrEmpty);
            }
            else
            {
                this.m_realistic = cueId;
                this.m_arcade = new MyCueId(MyStringHash.NullOrEmpty);
            }
        }

        public void Init(string cueName, bool useLog = true)
        {
            if ((string.IsNullOrEmpty(cueName) || Game.IsDedicated) || (MyAudio.Static == null))
            {
                this.m_arcade = new MyCueId(MyStringHash.NullOrEmpty);
                this.m_realistic = new MyCueId(MyStringHash.NullOrEmpty);
            }
            else
            {
                this.m_arcade = MyAudio.Static.GetCueId(cueName);
                if (this.m_arcade.Hash != MyStringHash.NullOrEmpty)
                {
                    this.m_realistic = this.m_arcade;
                }
                else
                {
                    Cache.Clear();
                    Cache.Append("Arc").Append(cueName);
                    this.m_arcade = MyAudio.Static.GetCueId(Cache.ToString());
                    Cache.Clear();
                    Cache.Append("Real").Append(cueName);
                    this.m_realistic = MyAudio.Static.GetCueId(Cache.ToString());
                    if (useLog)
                    {
                        if ((this.m_arcade.Hash == MyStringHash.NullOrEmpty) && (this.m_realistic.Hash == MyStringHash.NullOrEmpty))
                        {
                            MySandboxGame.Log.WriteLine($"Could not find any sound for '{cueName}'");
                        }
                        else
                        {
                            if (this.m_arcade.IsNull)
                            {
                                $"Could not find arcade sound for '{cueName}'";
                            }
                            if (this.m_realistic.IsNull)
                            {
                                $"Could not find realistic sound for '{cueName}'";
                            }
                        }
                    }
                }
            }
        }

        public override string ToString() => 
            this.SoundId.ToString();

        private static StringBuilder Cache
        {
            get
            {
                if (m_cache == null)
                {
                    m_cache = new StringBuilder();
                }
                return m_cache;
            }
        }

        public MyCueId Arcade =>
            this.m_arcade;

        public MyCueId Realistic =>
            this.m_realistic;

        public MyCueId SoundId
        {
            get
            {
                if (MySession.Static == null)
                {
                    return this.m_arcade;
                }
                if (!MySession.Static.Settings.RealisticSound || !MyFakes.ENABLE_NEW_SOUNDS)
                {
                    return this.m_arcade;
                }
                return this.m_realistic;
            }
        }
    }
}

