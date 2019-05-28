namespace Sandbox.Game.Gui
{
    using Havok;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Debugging;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Network;
    using VRage.Stats;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    [StaticEventOwner]
    internal class MyGuiScreenDebugTiming : MyGuiScreenDebugBase
    {
        private long m_startTime;
        private long m_ticks;
        private int m_frameCounter;
        private double m_updateLag;
        private static int m_callChainDepth;
        private static int m_numInstructions;

        public MyGuiScreenDebugTiming() : base(new Vector2(0.5f, 0.5f), new Vector2?(vector), nullable, true)
        {
            Vector2 vector = new Vector2();
            base.m_isTopMostScreen = true;
            base.m_drawEvenWithoutFocus = true;
            base.CanHaveFocus = false;
            base.m_canShareInput = false;
        }

        public override string GetFriendlyName() => 
            "DebugTimingScreen";

        public override void LoadData()
        {
            base.LoadData();
            MyRenderProxy.DrawRenderStats = MyRenderProxy.MyStatsState.SimpleTimingStats;
        }

        public override void UnloadData()
        {
            base.UnloadData();
            MyRenderProxy.DrawRenderStats = MyRenderProxy.MyStatsState.NoDraw;
        }

        public override bool Update(bool hasFocus)
        {
            this.m_ticks = Sandbox.Game.Debugging.MyPerformanceCounter.ElapsedTicks;
            this.m_frameCounter++;
            double num = Sandbox.Game.Debugging.MyPerformanceCounter.TicksToMs(this.m_ticks - this.m_startTime) / 1000.0;
            if (num > 1.0)
            {
                double num2 = num - (this.m_frameCounter * 0.01666667f);
                this.m_updateLag = (num2 / num) * 1000.0;
                this.m_startTime = this.m_ticks;
                this.m_frameCounter = 0;
            }
            Stats.Timing.Write(MyStatKeys.StatKeysEnum.Frame, (MySandboxGame.Static != null) ? ((float) MySandboxGame.Static.SimulationFrameCounter) : ((float) 0L), MyStatTypeEnum.CurrentValue, 0, 0, -1);
            Stats.Timing.Write(MyStatKeys.StatKeysEnum.FPS, (float) MyFpsManager.GetFps(), MyStatTypeEnum.CurrentValue, 0, 0, -1);
            Stats.Timing.Increment(MyStatKeys.StatKeysEnum.UPS, 0x3e8, -1);
            Stats.Timing.Write(MyStatKeys.StatKeysEnum.SimSpeed, MyPhysics.SimulationRatio, MyStatTypeEnum.CurrentValue, 100, 2, -1);
            Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.SimCpuLoad, (float) ((int) MySandboxGame.Static.CPULoadSmooth), MySandboxGame.Static.CPUTimeSmooth, MyStatTypeEnum.CurrentValue, 0, 0, -1);
            Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.ThreadCpuLoad, (float) ((int) MySandboxGame.Static.ThreadLoadSmooth), MySandboxGame.Static.ThreadTimeSmooth, MyStatTypeEnum.CurrentValue, 0, 0, -1);
            Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.RenderCpuLoad, (float) ((int) MyRenderProxy.CPULoadSmooth), MyRenderProxy.CPUTimeSmooth, MyStatTypeEnum.CurrentValue, 0, 0, -1);
            Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.RenderGpuLoad, (float) ((int) MyRenderProxy.GPULoadSmooth), MyRenderProxy.GPUTimeSmooth, MyStatTypeEnum.CurrentValue, 0, 0, -1);
            if (Sync.Layer != null)
            {
                Stats.Timing.Write(MyStatKeys.StatKeysEnum.ServerSimSpeed, Sync.ServerSimulationRatio, MyStatTypeEnum.CurrentValue, 100, 2, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.ServerSimCpuLoad, (float) ((int) Sync.ServerCPULoadSmooth), MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.ServerThreadCpuLoad, (float) ((int) Sync.ServerThreadLoadSmooth), MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.Up, MyGeneralStats.Static.SentPerSecond / 1024f, MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.Down, MyGeneralStats.Static.ReceivedPerSecond / 1024f, MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.ServerUp, MyGeneralStats.Static.ServerSentPerSecond / 1024f, MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.ServerDown, MyGeneralStats.Static.ServerReceivedPerSecond / 1024f, MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.Roundtrip, (float) MyGeneralStats.Static.Ping, MyStatTypeEnum.CurrentValue, 0, 0, -1);
            }
            if (MyRenderProxy.DrawRenderStats == MyRenderProxy.MyStatsState.ComplexTimingStats)
            {
                int cachedChuncks = 0;
                int pendingCachedChuncks = 0;
                if (MySession.Static != null)
                {
                    MySession.Static.VoxelMaps.GetCacheStats(out cachedChuncks, out pendingCachedChuncks);
                }
                Stats.Timing.WriteFormat("Voxel cache size: {0} / {3}", (float) cachedChuncks, (float) pendingCachedChuncks, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.FrameTime, MyFpsManager.FrameTime, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.FrameAvgTime, MyFpsManager.FrameTimeAvg, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.FrameMinTime, MyFpsManager.FrameTimeMin, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.FrameMaxTime, MyFpsManager.FrameTimeMax, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                Stats.Timing.Write(MyStatKeys.StatKeysEnum.UpdateLag, (float) this.m_updateLag, MyStatTypeEnum.CurrentValue, 0, 4, -1);
                Stats.Timing.Write(MyStatKeys.StatKeysEnum.GcMemory, (float) GC.GetTotalMemory(false), MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.Write(MyStatKeys.StatKeysEnum.ProcessMemory, (float) WinApi.WorkingSet, MyStatTypeEnum.CurrentValue, 0, 0, -1);
                Stats.Timing.Write(MyStatKeys.StatKeysEnum.ActiveParticleEffs, (float) MyParticlesManager.ParticleEffectsForUpdate.Count, MyStatTypeEnum.CurrentValue, 0, 0, -1);
                if (MyPhysics.GetClusterList() != null)
                {
                    double num5 = 0.0;
                    double num6 = 0.0;
                    double num7 = 0.0;
                    long num8 = 0L;
                    foreach (HkWorld world in MyPhysics.GetClusterList().Value)
                    {
                        num5++;
                        TimeSpan stepDuration = world.StepDuration;
                        double totalMilliseconds = stepDuration.TotalMilliseconds;
                        num6 += totalMilliseconds;
                        if (totalMilliseconds > num7)
                        {
                            num7 = totalMilliseconds;
                        }
                        num8 += world.ActiveRigidBodies.Count;
                    }
                    Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.PhysWorldCount, (float) num5, MyStatTypeEnum.CurrentValue, 0, 0, -1);
                    Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.ActiveRigBodies, (float) num8, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                    Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.PhysStepTimeSum, (float) num6, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                    Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.PhysStepTimeAvg, (float) (num6 / num5), MyStatTypeEnum.CurrentValue, 0, 1, -1);
                    Stats.Timing.WriteFormat(MyStatKeys.StatKeysEnum.PhysStepTimeMax, (float) num7, MyStatTypeEnum.CurrentValue, 0, 1, -1);
                }
            }
            return base.Update(hasFocus);
        }
    }
}

