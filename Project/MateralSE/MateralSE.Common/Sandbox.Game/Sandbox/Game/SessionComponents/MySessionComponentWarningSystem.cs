namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Network;
    using VRage.Scripting;
    using VRage.Serialization;
    using VRage.Utils;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x3e8, typeof(MyObjectBuilder_SessionComponent), (System.Type) null)]
    public class MySessionComponentWarningSystem : MySessionComponentBase
    {
        private static MySessionComponentWarningSystem m_static;
        private bool m_warningsDirty;
        private HashSet<Warning> m_warnings;
        private bool m_updateRequested;
        private List<WarningData> m_serverWarnings;
        private Dictionary<long, WarningData> m_warningData;
        private int m_updateCounter;
        private List<WarningData> m_cachedUpdateList;

        public void AddWarning(long id, WarningData warning)
        {
            this.m_warningData[id] = warning;
            this.RequestUpdate();
        }

        public override void LoadData()
        {
            base.LoadData();
            m_static = this;
            this.m_warningData = new Dictionary<long, WarningData>();
            this.m_serverWarnings = new List<WarningData>();
            this.m_warnings = new HashSet<Warning>();
        }

        private static void MergeWarning(HashSet<Warning> warnings, Warning warning)
        {
            Warning warning2;
            if (!warnings.TryGetValue<Warning>(warning, out warning2))
            {
                warnings.Add(warning);
            }
            else if (((warning2.Time != null) == (warning.Time != null)) && ((warning.Time != null) && (warning.Time.Value > warning2.Time.Value)))
            {
                warnings.Remove(warning2);
                warnings.Add(warning);
            }
        }

        [Event(null, 0xa3), Reliable, Broadcast]
        private static unsafe void OnUpdateWarnings(List<WarningData> warnings)
        {
            DateTime now = DateTime.Now;
            for (int i = 0; i < warnings.Count; i++)
            {
                WarningData data = warnings[i];
                if (data.LastOccurence != null)
                {
                    WarningData* dataPtr1 = (WarningData*) ref data;
                    dataPtr1->LastOccurence = new DateTime?(now + (data.LastOccurence.Value - DateTime.MinValue));
                    warnings[i] = data;
                }
            }
            MySessionComponentWarningSystem @static = Static;
            @static.m_serverWarnings = warnings;
            @static.RequestUpdate();
        }

        public void RequestUpdate()
        {
            this.m_warningsDirty = true;
            this.m_updateRequested = true;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            m_static = null;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.m_updateCounter++;
            bool isServer = Sync.IsServer;
            bool isDedicated = Sync.IsDedicated;
            int num = isDedicated ? 60 : 10;
            if ((this.m_updateCounter >= (60f * num)) || ((this.m_updateCounter >= 60f) && this.m_updateRequested))
            {
                this.m_updateCounter = 0;
                this.m_updateRequested = false;
                if (isServer)
                {
                    this.UpdateServerWarnings();
                }
                if (!isDedicated)
                {
                    this.UpdateClientWarnings();
                }
                this.m_warningsDirty = true;
            }
        }

        private void UpdateClientWarnings()
        {
            int num1;
            bool flag = MyDebugDrawSettings.DEBUG_DRAW_SERVER_WARNINGS;
            if ((!Sync.IsServer && (Sync.ServerSimulationRatio < 0.8f)) | flag)
            {
                this.AddWarning((long) MyCommonTexts.PerformanceWarningIssuesServer_Simspeed.Id, new WarningData(MyCommonTexts.PerformanceWarningAreaPhysics, MyCommonTexts.PerformanceWarningIssuesServer_Simspeed, Category.Server));
            }
            if (MySession.Static.ServerSaving | flag)
            {
                this.AddWarning((long) MyCommonTexts.PerformanceWarningIssuesServer_Saving.Id, new WarningData(MyCommonTexts.PerformanceWarningHeading_Saving, MyCommonTexts.PerformanceWarningIssuesServer_Saving, Category.Server));
            }
            if ((!MySession.Static.MultiplayerAlive && !MySession.Static.ServerSaving) | flag)
            {
                this.AddWarning((long) MyCommonTexts.PerformanceWarningIssuesServer_NoConnection.Id, new WarningData(MyCommonTexts.PerformanceWarningIssuesServer_NoConnection, MyCommonTexts.Multiplayer_NoConnection, Category.Server));
            }
            if (!MySession.Static.MultiplayerDirect | flag)
            {
                this.AddWarning((long) MyCommonTexts.PerformanceWarningIssuesServer_Direct.Id, new WarningData(MyCommonTexts.PerformanceWarningIssuesServer_Direct, MyCommonTexts.Multiplayer_IndirectConnection, Category.Server));
            }
            if (!Sync.IsServer)
            {
                num1 = (int) (MySession.Static.MultiplayerPing.Milliseconds > 250.0);
            }
            else
            {
                num1 = 0;
            }
            if ((num1 | flag) != 0)
            {
                this.AddWarning((long) MyCommonTexts.PerformanceWarningIssuesServer_Latency.Id, new WarningData(MyCommonTexts.PerformanceWarningIssuesServer_Latency, MyCommonTexts.Multiplayer_HighPing, Category.Server));
            }
            if (MyGeneralStats.Static.LowNetworkQuality | flag)
            {
                this.AddWarning((long) MyCommonTexts.PerformanceWarningIssuesServer_PoorConnection.Id, new WarningData(MyCommonTexts.PerformanceWarningIssuesServer_PoorConnection, MyCommonTexts.Multiplayer_PacketLossDescription, Category.Server));
            }
            if (!MySession.Static.HighSimulationQualityNotification | flag)
            {
                this.AddWarning((long) MyCommonTexts.PerformanceWarningIssues_LowSimulationQuality.Id, new WarningData(MyCommonTexts.PerformanceWarningIssues_LowSimulationQuality, MyCommonTexts.Performance_LowSimulationQuality, Category.Performance));
            }
        }

        private void UpdateImmediateWarnings(Action<WarningData> add)
        {
            foreach (MyCubeGrid grid in MyUnsafeGridsSessionComponent.UnsafeGrids.Values)
            {
                string descriptionString = string.Join(", ", (IEnumerable<string>) (from x in grid.UnsafeBlocks select x.DisplayNameText));
                add(new WarningData(grid.DisplayName, descriptionString, Category.UnsafeGrids));
            }
            foreach (KeyValuePair<long, MyTuple<string, MyStringId>> pair in MyModWatchdog.Warnings)
            {
                MyTuple<string, MyStringId> tuple = pair.Value;
                DateTime? lastOccurence = null;
                add(new WarningData(Category.Other, MyStringId.NullOrEmpty, tuple.Item1, tuple.Item2, tuple.Item1, lastOccurence));
            }
        }

        private void UpdateServerWarnings()
        {
            using (MyUtils.ClearCollectionToken<List<WarningData>, WarningData> token = MyUtils.ReuseCollection<WarningData>(ref this.m_cachedUpdateList))
            {
                List<WarningData> collection = token.Collection;
                this.UpdateImmediateWarnings(new Action<WarningData>(collection.Add));
                collection.AddRange(this.m_warningData.Values);
                DateTime now = DateTime.Now;
                int num = 0;
                while (true)
                {
                    if (num >= collection.Count)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<List<WarningData>>(x => new Action<List<WarningData>>(MySessionComponentWarningSystem.OnUpdateWarnings), collection, targetEndpoint, position);
                        break;
                    }
                    WarningData data = collection[num];
                    if (data.LastOccurence != null)
                    {
                        TimeSpan zero = (TimeSpan) (now - data.LastOccurence.Value);
                        if (zero < TimeSpan.Zero)
                        {
                            zero = TimeSpan.Zero;
                        }
                        data.LastOccurence = new DateTime?(DateTime.MinValue + zero);
                        collection[num] = data;
                    }
                    num++;
                }
            }
        }

        public static MySessionComponentWarningSystem Static =>
            m_static;

        public HashSet<Warning> CurrentWarnings
        {
            get
            {
                if (this.m_warningsDirty)
                {
                    this.m_warnings.Clear();
                    this.m_warningsDirty = false;
                    foreach (WarningData data in this.m_serverWarnings)
                    {
                        MergeWarning(this.m_warnings, data.ConstructWarning());
                    }
                    foreach (WarningData data2 in this.m_warningData.Values)
                    {
                        MergeWarning(this.m_warnings, data2.ConstructWarning());
                    }
                    this.UpdateImmediateWarnings(delegate (WarningData x) {
                        m_static.m_warnings.Add(x.ConstructWarning());
                    });
                }
                return this.m_warnings;
            }
        }

        public override System.Type[] Dependencies =>
            new System.Type[] { typeof(MyUnsafeGridsSessionComponent) };

        public override bool IsRequiredByGame =>
            true;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentWarningSystem.<>c <>9 = new MySessionComponentWarningSystem.<>c();
            public static Action<MySessionComponentWarningSystem.WarningData> <>9__7_0;
            public static Func<IMyEventOwner, Action<List<MySessionComponentWarningSystem.WarningData>>> <>9__16_0;
            public static Func<MyCubeBlock, string> <>9__18_0;

            internal void <get_CurrentWarnings>b__7_0(MySessionComponentWarningSystem.WarningData x)
            {
                MySessionComponentWarningSystem.m_static.m_warnings.Add(x.ConstructWarning());
            }

            internal string <UpdateImmediateWarnings>b__18_0(MyCubeBlock x) => 
                x.DisplayNameText;

            internal Action<List<MySessionComponentWarningSystem.WarningData>> <UpdateServerWarnings>b__16_0(IMyEventOwner x) => 
                new Action<List<MySessionComponentWarningSystem.WarningData>>(MySessionComponentWarningSystem.OnUpdateWarnings);
        }

        public enum Category
        {
            Graphics,
            Blocks,
            Other,
            UnsafeGrids,
            BlockLimits,
            Server,
            Performance,
            General
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Warning : IEquatable<MySessionComponentWarningSystem.Warning>
        {
            public DateTime? Time;
            public readonly string Title;
            public readonly string Description;
            public readonly Sandbox.Game.SessionComponents.MySessionComponentWarningSystem.Category Category;
            public Warning(string title, string description, Sandbox.Game.SessionComponents.MySessionComponentWarningSystem.Category category, DateTime? time)
            {
                this.Time = time;
                this.Title = title;
                this.Category = category;
                this.Description = description;
            }

            public bool Equals(MySessionComponentWarningSystem.Warning other) => 
                ((this.Title == other.Title) && ((this.Category == other.Category) && (this.Description == other.Description)));

            public override int GetHashCode() => 
                MyTuple.CombineHashCodes(this.Title.GetHashCode(), this.Description.GetHashCode(), (int) this.Category);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WarningData
        {
            public DateTime? LastOccurence;
            public Sandbox.Game.SessionComponents.MySessionComponentWarningSystem.Category Category;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string TitleIdKey;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string TitleString;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string DescriptionIdKey;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string DescriptionString;
            public WarningData(MyStringId title, MyStringId description, Sandbox.Game.SessionComponents.MySessionComponentWarningSystem.Category category) : this(title, description, category, new DateTime?(DateTime.Now))
            {
            }

            public WarningData(MyStringId title, MyStringId description, Sandbox.Game.SessionComponents.MySessionComponentWarningSystem.Category category, DateTime? time)
            {
                this.Category = category;
                this.LastOccurence = time;
                this.TitleIdKey = title.String;
                this.DescriptionIdKey = description.String;
                this.TitleString = null;
                this.DescriptionString = null;
            }

            public WarningData(string titleString, string descriptionString, Sandbox.Game.SessionComponents.MySessionComponentWarningSystem.Category category)
            {
                this.Category = category;
                this.TitleString = titleString;
                this.DescriptionString = descriptionString;
                this.TitleIdKey = null;
                this.LastOccurence = null;
                this.DescriptionIdKey = null;
            }

            public WarningData(Sandbox.Game.SessionComponents.MySessionComponentWarningSystem.Category category, MyStringId title, string titleString, MyStringId description, string descriptionString, DateTime? lastOccurence)
            {
                this.Category = category;
                this.TitleIdKey = title.String;
                this.TitleString = titleString;
                this.LastOccurence = lastOccurence;
                this.DescriptionIdKey = description.String;
                this.DescriptionString = descriptionString;
            }

            public MySessionComponentWarningSystem.Warning ConstructWarning() => 
                new MySessionComponentWarningSystem.Warning(ConstructLocalizedString(this.TitleIdKey, this.TitleString), ConstructLocalizedString(this.DescriptionIdKey, this.DescriptionString), this.Category, this.LastOccurence);

            private static string ConstructLocalizedString(string formatKey, string strData)
            {
                if (string.IsNullOrEmpty(formatKey))
                {
                    return strData;
                }
                string format = MyTexts.GetString(formatKey);
                return ((strData != null) ? string.Format(format, strData) : format);
            }
        }
    }
}

