namespace Sandbox.Engine.Networking
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRage.Replication;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGeneralStats
    {
        private MyTimeSpan m_lastTime;
        private static int AVERAGE_WINDOW_SIZE = 60;
        private static int SERVER_AVERAGE_WINDOW_SIZE = 6;
        private readonly MyMovingAverage m_received = new MyMovingAverage(AVERAGE_WINDOW_SIZE, 0x3e8);
        private readonly MyMovingAverage m_sent = new MyMovingAverage(AVERAGE_WINDOW_SIZE, 0x3e8);
        private readonly MyMovingAverage m_timeIntervals = new MyMovingAverage(AVERAGE_WINDOW_SIZE, 0x3e8);
        private readonly MyMovingAverage m_serverReceived = new MyMovingAverage(SERVER_AVERAGE_WINDOW_SIZE, 0x3e8);
        private readonly MyMovingAverage m_serverSent = new MyMovingAverage(SERVER_AVERAGE_WINDOW_SIZE, 0x3e8);
        private readonly MyMovingAverage m_serverTimeIntervals = new MyMovingAverage(SERVER_AVERAGE_WINDOW_SIZE, 0x3e8);
        public MyTimeSpan LogInterval = MyTimeSpan.FromSeconds(60.0);
        private bool m_first = true;
        private MyTimeSpan m_lastLogTime;
        private MyTimeSpan m_firstLogTime;
        private int m_gridsCount;
        private Process m_process;
        private PerformanceCounter m_cpuCounter;
        private PerformanceCounter m_ramCounter;

        static MyGeneralStats()
        {
            Static = new MyGeneralStats();
        }

        public static void Clear()
        {
            int num;
            int num2;
            MyNetworkWriter.GetAndClearStats();
            MyNetworkReader.GetAndClearStats(out num, out num2);
        }

        public void LoadData()
        {
            this.m_gridsCount = 0;
            MyEntities.OnEntityCreate += new Action<MyEntity>(this.OnEntityCreate);
            MyEntities.OnEntityDelete += new Action<MyEntity>(this.OnEntityDelete);
        }

        private void OnEntityCreate(MyEntity entity)
        {
            if (entity is MyCubeGrid)
            {
                Interlocked.Increment(ref this.m_gridsCount);
            }
        }

        private void OnEntityDelete(MyEntity entity)
        {
            if (entity is MyCubeGrid)
            {
                Interlocked.Decrement(ref this.m_gridsCount);
            }
        }

        public static void ToggleProfiler()
        {
            Sandbox.MyRenderProfiler.EnableAutoscale(MyStatsGraph.PROFILER_NAME);
            Sandbox.MyRenderProfiler.ToggleProfiler(MyStatsGraph.PROFILER_NAME);
        }

        public void Update()
        {
            if (MySession.Static != null)
            {
                int num;
                int num2;
                MyNetworkReader.GetAndClearStats(out num, out num2);
                int andClearStats = MyNetworkWriter.GetAndClearStats();
                this.OverallReceived += num;
                this.OverallSent += andClearStats;
                MyTimeSpan simulationTime = MySandboxGame.Static.SimulationTime;
                float seconds = (float) (simulationTime - this.m_lastTime).Seconds;
                this.m_lastTime = simulationTime;
                this.m_received.Enqueue((float) num);
                this.m_sent.Enqueue((float) andClearStats);
                this.m_timeIntervals.Enqueue(seconds);
                this.Received = this.m_received.Avg;
                this.Sent = this.m_sent.Avg;
                this.ReceivedPerSecond = (float) (this.m_received.Sum / this.m_timeIntervals.Sum);
                this.SentPerSecond = (float) (this.m_sent.Sum / this.m_timeIntervals.Sum);
                if (this.ReceivedPerSecond > this.PeakReceivedPerSecond)
                {
                    this.PeakReceivedPerSecond = this.ReceivedPerSecond;
                }
                if (this.SentPerSecond > this.PeakSentPerSecond)
                {
                    this.PeakSentPerSecond = this.SentPerSecond;
                }
                float gcMemory = (((float) GC.GetTotalMemory(false)) / 1024f) / 1024f;
                if (this.m_process == null)
                {
                    this.m_process = Process.GetCurrentProcess();
                    try
                    {
                        this.m_cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                        this.m_ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                    }
                    catch (Exception exception)
                    {
                        MyLog.Default.WriteLine("Error initializing PerformanceCounters. CPU and Memory statistics logging will be suspended.\nTry running \"lodctr /r\" in admin command line to fix it.");
                        MyLog.Default.WriteLine(exception);
                    }
                }
                float processMemory = (((float) this.m_process.PrivateMemorySize64) / 1024f) / 1024f;
                if (Sync.MultiplayerActive && Sync.IsServer)
                {
                    MyMultiplayer.Static.ReplicationLayer.UpdateStatisticsData(andClearStats, num, num2, gcMemory, processMemory);
                }
                if (simulationTime > (this.m_lastLogTime + this.LogInterval))
                {
                    this.m_lastLogTime = simulationTime;
                    if (this.m_first)
                    {
                        this.m_firstLogTime = simulationTime;
                        this.m_first = false;
                    }
                    MyLog.Default.WriteLine("STATISTICS LEGEND,time,ReceivedPerSecond,SentPerSecond,PeakReceivedPerSecond,PeakSentPerSecond,OverallReceived,OverallSent,CPULoadSmooth,ThreadLoadSmooth,GetOnlinePlayerCount,Ping,GCMemory,ProcessMemory,PCUBuilt,PCU,GridsCount,RenderCPULoadSmooth,RenderGPULoadSmooth,HardwareCPULoad,HardwareAvailableMemory,FrameTime");
                    object[] objArray1 = new object[0x15];
                    objArray1[0] = (simulationTime - this.m_firstLogTime).Seconds;
                    objArray1[1] = (this.ReceivedPerSecond / 1024f) / 1024f;
                    objArray1[2] = (this.SentPerSecond / 1024f) / 1024f;
                    objArray1[3] = (this.PeakReceivedPerSecond / 1024f) / 1024f;
                    objArray1[4] = (this.PeakSentPerSecond / 1024f) / 1024f;
                    objArray1[5] = (((float) this.OverallReceived) / 1024f) / 1024f;
                    objArray1[6] = (((float) this.OverallSent) / 1024f) / 1024f;
                    objArray1[7] = MySandboxGame.Static.CPULoadSmooth;
                    objArray1[8] = MySandboxGame.Static.ThreadLoadSmooth;
                    objArray1[9] = Sync.Players.GetOnlinePlayerCount();
                    objArray1[10] = this.Ping;
                    objArray1[11] = gcMemory;
                    objArray1[12] = processMemory;
                    objArray1[13] = MySession.Static.GlobalBlockLimits.PCUBuilt;
                    objArray1[14] = MySession.Static.GlobalBlockLimits.PCU;
                    objArray1[15] = this.GridsCount;
                    objArray1[0x10] = MyRenderProxy.CPULoadSmooth;
                    objArray1[0x11] = MyRenderProxy.GPULoadSmooth;
                    objArray1[0x12] = (this.m_cpuCounter != null) ? this.m_cpuCounter.NextValue() : 0f;
                    object[] local2 = objArray1;
                    object[] local3 = objArray1;
                    local3[0x13] = (this.m_ramCounter != null) ? this.m_ramCounter.NextValue() : 0f;
                    object[] args = local3;
                    args[20] = MyFpsManager.FrameTimeAvg;
                    MyLog.Default.WriteLine(string.Format("STATISTICS,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}", args));
                }
                if (!Game.IsDedicated)
                {
                    MyPacketStatistics statistics = new MyPacketStatistics();
                    if (Sync.IsServer)
                    {
                        this.ServerReceivedPerSecond = this.ReceivedPerSecond;
                        this.ServerSentPerSecond = this.SentPerSecond;
                    }
                    else if (Sync.MultiplayerActive)
                    {
                        statistics = MyMultiplayer.Static.ReplicationLayer.ClearServerStatistics();
                        if (statistics.TimeInterval > 0f)
                        {
                            this.m_serverReceived.Enqueue((float) statistics.IncomingData);
                            this.m_serverSent.Enqueue((float) statistics.OutgoingData);
                            this.m_serverTimeIntervals.Enqueue(statistics.TimeInterval);
                            this.ServerReceivedPerSecond = (float) (this.m_serverReceived.Sum / this.m_serverTimeIntervals.Sum);
                            this.ServerSentPerSecond = (float) (this.m_serverSent.Sum / this.m_serverTimeIntervals.Sum);
                            this.ServerGCMemory = statistics.GCMemory;
                            this.ServerProcessMemory = statistics.ProcessMemory;
                        }
                    }
                    MyStatsGraph.Begin("Client Traffic Avg", 0x7fffffff, "Update", 0xad, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Outgoing avg", this.SentPerSecond / 1024f, "{0} kB/s", "Update", 0xae, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Incoming avg", this.ReceivedPerSecond / 1024f, "{0} kB/s", "Update", 0xaf, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(new float?((this.SentPerSecond + this.ReceivedPerSecond) / 1024f), 0f, "", "{0} kB/s", null, "Update", 0xb0, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("Server Traffic Avg", 0x7fffffff, "Update", 0xb1, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Outgoing avg", this.ServerSentPerSecond / 1024f, "{0} kB/s", "Update", 0xb2, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Incoming avg", this.ServerReceivedPerSecond / 1024f, "{0} kB/s", "Update", 0xb3, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(new float?((this.ServerSentPerSecond + this.ServerReceivedPerSecond) / 1024f), 0f, "", "{0} kB/s", null, "Update", 180, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("Client Perf Avg", 0x7fffffff, "Update", 0xb6, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Main CPU", MySandboxGame.Static.CPULoadSmooth, "{0}%", "Update", 0xb7, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Threads", MySandboxGame.Static.ThreadLoadSmooth, "{0}%", "Update", 0xb8, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Render CPU", MyRenderProxy.CPULoadSmooth, "{0}%", "Update", 0xb9, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Render GPU", MyRenderProxy.GPULoadSmooth, "{0}%", "Update", 0xba, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Render Frame", MyFpsManager.FrameTimeAvg, "{0}ms", "Update", 0xbb, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(new float?(MySandboxGame.Static.CPULoadSmooth), 0f, null, "{0}%", null, "Update", 0xbc, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("Server Perf Avg", 0x7fffffff, "Update", 0xbd, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Main CPU", Sync.ServerCPULoadSmooth, "{0}%", "Update", 190, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Threads", Sync.ServerThreadLoadSmooth, "{0}%", "Update", 0xbf, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(new float?(Sync.ServerCPULoadSmooth), 0f, null, "{0}%", null, "Update", 0xc0, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("World", 0x7fffffff, "Update", 0xc2, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("PCUBuilt", (float) MySession.Static.GlobalBlockLimits.PCUBuilt, "{0}", "Update", 0xc3, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("PCU", (float) MySession.Static.GlobalBlockLimits.PCU, "{0}", "Update", 0xc4, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("PiratePCUBuilt", (float) MySession.Static.PirateBlockLimits.PCUBuilt, "{0}", "Update", 0xc5, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("PiratePCU", (float) MySession.Static.PirateBlockLimits.PCU, "{0}", "Update", 0xc6, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("GridsCount", (float) this.GridsCount, "{0}", "Update", 0xc7, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    float? bytesTransfered = null;
                    MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "Update", 200, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("Memory", 0x7fffffff, "Update", 0xc9, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Client GC", gcMemory, "{0}M", "Update", 0xca, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Client Process", processMemory, "{0}M", "Update", 0xcb, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Server GC", this.ServerGCMemory, "{0}M", "Update", 0xcc, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Server Process", this.ServerProcessMemory, "{0}M", "Update", 0xcd, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    bytesTransfered = null;
                    MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "Update", 0xce, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    if (!Sync.MultiplayerActive)
                    {
                        this.LowNetworkQuality = false;
                    }
                    else
                    {
                        MyPacketStatistics statistics2 = MyMultiplayer.Static.ReplicationLayer.ClearClientStatistics();
                        int num7 = (((((statistics2.Drops + statistics2.OutOfOrder) + statistics2.Duplicates) + statistics.PendingPackets) + statistics.Drops) + statistics.OutOfOrder) + statistics.Duplicates;
                        MyStatsGraph.Begin("Packet errors", 0x7fffffff, "Update", 0xd5, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Client Drops", (float) statistics2.Drops, "{0}", "Update", 0xd6, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Client OutOfOrder", (float) statistics2.OutOfOrder, "{0}", "Update", 0xd7, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Client Duplicates", (float) statistics2.Duplicates, "{0}", "Update", 0xd8, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Client Tamperred", (float) statistics2.Tamperred, "{0}", "Update", 0xd9, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Server Pending Packets", (float) statistics.PendingPackets, "{0}", "Update", 0xda, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Server Drops", (float) statistics.Drops, "{0}", "Update", 0xdb, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Server OutOfOrder", (float) statistics.OutOfOrder, "{0}", "Update", 220, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Server Duplicates", (float) statistics.Duplicates, "{0}", "Update", 0xdd, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.CustomTime("Server Tamperred", (float) statistics2.Tamperred, "{0}", "Update", 0xde, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        MyStatsGraph.End(new float?((float) num7), 0f, null, "{0}", null, "Update", 0xdf, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                        this.LowNetworkQuality = num7 > 5;
                    }
                    MyStatsGraph.Begin("Physics", 0x7fffffff, "Update", 0xe7, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Clusters", (float) MyPhysics.Clusters.GetClusters().Count, "{0}", "Update", 0xe9, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("VoxelBodies", (float) MyVoxelPhysicsBody.ActiveVoxelPhysicsBodies, "{0}", "Update", 0xea, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("LargeVoxelBodies", (float) MyVoxelPhysicsBody.ActiveVoxelPhysicsBodiesWithExtendedCache, "{0}", "Update", 0xeb, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(0f, 0f, null, "{0}", null, "Update", 0xed, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.ProfileAdvanced(true);
                    MyStatsGraph.Begin("Traffic", 0x7fffffff, "Update", 0xf1, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Outgoing", this.Sent / 1024f, "{0} kB", "Update", 0xf2, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Incoming", this.Received / 1024f, "{0} kB", "Update", 0xf3, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(new float?((this.SentPerSecond + this.ReceivedPerSecond) / 1024f), 0f, "", "{0} kB", null, "Update", 0xf4, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("Server Perf Avg", 0x7fffffff, "Update", 0xf6, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Main CPU", Sync.ServerCPULoadSmooth, "{0}%", "Update", 0xf7, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Threads", Sync.ServerThreadLoadSmooth, "{0}%", "Update", 0xf8, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(0f, 0f, null, "{0}", null, "Update", 0xf9, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("Client Performance", 0x7fffffff, "Update", 0xfb, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Main CPU", MySandboxGame.Static.CPULoad, "{0}%", "Update", 0xfc, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Threads", MySandboxGame.Static.ThreadLoad, "{0}%", "Update", 0xfd, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Render CPU", MyRenderProxy.CPULoad, "{0}%", "Update", 0xfe, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Render GPU", MyRenderProxy.GPULoad, "{0}%", "Update", 0xff, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(0f, 0f, null, "{0}", null, "Update", 0x100, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.Begin("Server Performance", 0x7fffffff, "Update", 0x102, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Main CPU", Sync.ServerCPULoad, "{0}%", "Update", 0x103, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.CustomTime("Threads", Sync.ServerThreadLoad, "{0}%", "Update", 260, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.End(0f, 0f, null, "{0}", null, "Update", 0x105, @"E:\Repo1\Sources\Sandbox.Game\Engine\Networking\MyGeneralStats.cs");
                    MyStatsGraph.ProfileAdvanced(false);
                }
            }
        }

        public static MyGeneralStats Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        public float Received { get; private set; }

        public float Sent { get; private set; }

        public float ReceivedPerSecond { get; private set; }

        public float SentPerSecond { get; private set; }

        public float PeakReceivedPerSecond { get; private set; }

        public float PeakSentPerSecond { get; private set; }

        public long OverallReceived { get; private set; }

        public long OverallSent { get; private set; }

        public float ServerReceivedPerSecond { get; private set; }

        public float ServerSentPerSecond { get; private set; }

        public float ServerGCMemory { get; private set; }

        public float ServerProcessMemory { get; private set; }

        public int GridsCount =>
            this.m_gridsCount;

        public long Ping { get; set; }

        public bool LowNetworkQuality { get; private set; }
    }
}

