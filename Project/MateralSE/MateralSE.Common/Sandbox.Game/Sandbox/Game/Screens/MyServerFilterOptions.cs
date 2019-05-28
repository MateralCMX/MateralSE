namespace Sandbox.Game.Screens
{
    using Sandbox.Game;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.GameServices;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyServerFilterOptions
    {
        public bool AllowedGroups;
        public bool SameVersion;
        public bool SameData;
        public bool? HasPassword;
        public bool CreativeMode;
        public bool SurvivalMode;
        public bool AdvancedFilter;
        public int Ping;
        public bool CheckPlayer;
        public SerializableRange PlayerCount;
        public bool CheckMod;
        public SerializableRange ModCount;
        public bool CheckDistance;
        public SerializableRange ViewDistance;
        public bool ModsExclusive;
        public HashSet<ulong> Mods;
        private Dictionary<byte, IMyFilterOption> m_filters;
        public const int MAX_PING = 150;

        public MyServerFilterOptions()
        {
            this.Mods = new HashSet<ulong>();
            this.SetDefaults(false);
        }

        public MyServerFilterOptions(MyObjectBuilder_ServerFilterOptions ob)
        {
            this.Mods = new HashSet<ulong>();
            this.Init(ob);
        }

        protected abstract Dictionary<byte, IMyFilterOption> CreateFilters();
        public abstract bool FilterLobby(IMyLobby lobby);
        public abstract bool FilterServer(MyCachedServerItem server);
        public MyObjectBuilder_ServerFilterOptions GetObjectBuilder()
        {
            MyLog.Default.WriteLine("get");
            MyObjectBuilder_ServerFilterOptions options = new MyObjectBuilder_ServerFilterOptions {
                AllowedGroups = this.AllowedGroups,
                SameVersion = this.SameVersion,
                SameData = this.SameData,
                HasPassword = this.HasPassword,
                CreativeMode = this.CreativeMode,
                SurvivalMode = this.SurvivalMode,
                CheckPlayer = this.CheckPlayer,
                PlayerCount = this.PlayerCount,
                CheckMod = this.CheckMod,
                ModCount = this.ModCount,
                CheckDistance = this.CheckDistance,
                ViewDistance = this.ViewDistance,
                Advanced = this.AdvancedFilter,
                Ping = this.Ping,
                Mods = (this.Mods != null) ? this.Mods.ToList<ulong>() : null,
                ModsExclusive = this.ModsExclusive,
                Filters = new SerializableDictionary<byte, string>()
            };
            foreach (KeyValuePair<byte, IMyFilterOption> pair in this.Filters)
            {
                options.Filters[pair.Key] = pair.Value.SerializedValue;
            }
            return options;
        }

        public void Init(MyObjectBuilder_ServerFilterOptions ob)
        {
            this.AllowedGroups = ob.AllowedGroups;
            this.SameVersion = ob.SameVersion;
            this.SameData = ob.SameData;
            this.HasPassword = ob.HasPassword;
            this.CreativeMode = ob.CreativeMode;
            this.SurvivalMode = ob.SurvivalMode;
            this.CheckPlayer = ob.CheckPlayer;
            this.PlayerCount = ob.PlayerCount;
            this.CheckMod = ob.CheckMod;
            this.ModCount = ob.ModCount;
            this.CheckDistance = ob.CheckDistance;
            this.ViewDistance = ob.ViewDistance;
            this.AdvancedFilter = ob.Advanced;
            this.Ping = ob.Ping;
            this.ModsExclusive = ob.ModsExclusive;
            this.Mods = (ob.Mods == null) ? new HashSet<ulong>() : new HashSet<ulong>(ob.Mods);
            if (ob.Filters != null)
            {
                foreach (KeyValuePair<byte, string> pair in ob.Filters.Dictionary)
                {
                    IMyFilterOption option;
                    if (!this.Filters.TryGetValue(pair.Key, out option))
                    {
                        throw new Exception("Unrecognized filter key");
                    }
                    option.Configure(pair.Value);
                }
            }
        }

        public void SetDefaults(bool resetMods = false)
        {
            this.AdvancedFilter = false;
            this.CheckPlayer = false;
            this.CheckMod = false;
            this.CheckDistance = false;
            this.AllowedGroups = true;
            this.SameVersion = true;
            this.SameData = true;
            this.CreativeMode = true;
            this.SurvivalMode = true;
            this.HasPassword = null;
            this.Ping = 150;
            this.m_filters = this.CreateFilters();
            if (resetMods)
            {
                this.Mods.Clear();
            }
        }

        public Dictionary<byte, IMyFilterOption> Filters
        {
            get
            {
                Dictionary<byte, IMyFilterOption> filters = this.m_filters;
                if (this.m_filters == null)
                {
                    Dictionary<byte, IMyFilterOption> local1 = this.m_filters;
                    filters = this.m_filters = this.CreateFilters();
                }
                return filters;
            }
            set => 
                (this.m_filters = value);
        }
    }
}

