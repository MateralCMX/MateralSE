namespace Sandbox.Engine.Platform
{
    using ParallelTasks;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Debugging;
    using Sandbox.Game.World;
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Library.Utils;
    using VRage.Stats;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    public abstract class Game
    {
        public static bool IsDedicated;
        public static bool IsPirated;
        public static bool IgnoreLastSession;
        public static IPEndPoint ConnectToServer;
        public static bool EnableSimSpeedLocking;
        [Obsolete("Remove asap, it is here only because of main menu music..")]
        protected readonly MyGameTimer m_gameTimer = new MyGameTimer();
        private MyTimeSpan m_drawTime;
        private MyTimeSpan m_totalTime;
        private ulong m_updateCounter;
        private MyTimeSpan m_simulationTimeWithSpeed;
        public const double TARGET_MS_PER_FRAME = 16.666666666666668;
        private const int NUM_FRAMES_FOR_DROP = 5;
        private const float NUM_MS_TO_INCREASE = 2000f;
        private const float PEAK_TRESHOLD_RATIO = 0.4f;
        private const float RATIO_TO_INCREASE_INSTANTLY = 0.25f;
        private float m_currentFrameIncreaseTime;
        private long m_currentMin;
        private long m_targetTicks;
        private MyQueue<long> m_lastFrameTiming = new MyQueue<long>(5);
        private bool isFirstUpdateDone;
        private bool isMouseVisible;
        public long FrameTimeTicks;
        private ManualResetEventSlim m_waiter;
        private MyTimer.TimerEventHandler m_handler;
        private static long m_lastFrameTime;
        private static float m_targetMs;
        [CompilerGenerated]
        private Action OnGameExit;
        private readonly FixedLoop m_renderLoop = new FixedLoop(Stats.Generic, "WaitForUpdate");

        public event Action OnGameExit
        {
            [CompilerGenerated] add
            {
                Action onGameExit = this.OnGameExit;
                while (true)
                {
                    Action a = onGameExit;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onGameExit = Interlocked.CompareExchange<Action>(ref this.OnGameExit, action3, a);
                    if (ReferenceEquals(onGameExit, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onGameExit = this.OnGameExit;
                while (true)
                {
                    Action source = onGameExit;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onGameExit = Interlocked.CompareExchange<Action>(ref this.OnGameExit, action3, source);
                    if (ReferenceEquals(onGameExit, source))
                    {
                        return;
                    }
                }
            }
        }

        public Game()
        {
            this.IsActive = true;
            this.m_waiter = new ManualResetEventSlim(false, 0);
            this.m_handler = (a, b, c, d, e) => this.m_waiter.Set();
            this.CPULoadSmooth = 1f;
        }

        protected abstract void AfterDraw();
        public void Exit()
        {
            Action onGameExit = this.OnGameExit;
            if (onGameExit != null)
            {
                onGameExit();
            }
            this.m_renderLoop.IsDone = true;
        }

        protected abstract void LoadData_UpdateThread();
        private void Lock(long beforeUpdate)
        {
            long num = Math.Min(Math.Max(this.m_renderLoop.TickPerFrame, this.UpdateCurrentFrame()), 10 * this.m_renderLoop.TickPerFrame);
            this.m_currentMin = Math.Max(num, this.m_currentMin);
            this.m_currentFrameIncreaseTime += m_targetMs;
            if (num > this.m_targetTicks)
            {
                this.m_targetTicks = num;
                this.m_currentFrameIncreaseTime = 0f;
                this.m_currentMin = 0L;
                m_targetMs = (float) Sandbox.Game.Debugging.MyPerformanceCounter.TicksToMs(this.m_targetTicks);
            }
            else
            {
                bool flag = (this.m_targetTicks - this.m_currentMin) > (0.25f * this.m_renderLoop.TickPerFrame);
                if ((this.m_currentFrameIncreaseTime > 2000f) | flag)
                {
                    this.m_targetTicks = this.m_currentMin;
                    this.m_currentFrameIncreaseTime = 0f;
                    this.m_currentMin = 0L;
                    m_targetMs = (float) Sandbox.Game.Debugging.MyPerformanceCounter.TicksToMs(this.m_targetTicks);
                }
            }
            long num2 = Sandbox.Game.Debugging.MyPerformanceCounter.ElapsedTicks - beforeUpdate;
            int intervalMS = (int) (MyTimeSpan.FromTicks(this.m_targetTicks - num2).Milliseconds - 0.1);
            if ((intervalMS > 0) && !this.EnableMaxSpeed)
            {
                this.m_waiter.Reset();
                MyTimer.StartOneShot(intervalMS, this.m_handler);
                this.m_waiter.Wait((int) (intervalMS + 1));
            }
            for (num2 = Sandbox.Game.Debugging.MyPerformanceCounter.ElapsedTicks - beforeUpdate; this.m_targetTicks > num2; num2 = Sandbox.Game.Debugging.MyPerformanceCounter.ElapsedTicks - beforeUpdate)
            {
            }
        }

        protected abstract void PrepareForDraw();
        protected void RunLoop()
        {
            try
            {
                this.m_targetTicks = this.m_renderLoop.TickPerFrame;
                MyLog.Default.WriteLine("Timer Frequency: " + MyGameTimer.Frequency);
                MyLog.Default.WriteLine("Ticks per frame: " + this.m_renderLoop.TickPerFrame);
                this.m_renderLoop.Run(new GenericLoop.VoidAction(this.RunSingleFrame));
            }
            catch (SEHException exception)
            {
                MyLog.Default.WriteLine("SEHException caught. Error code: " + exception.ErrorCode.ToString());
                throw exception;
            }
        }

        public void RunSingleFrame()
        {
            bool isFirstUpdateDone = this.IsFirstUpdateDone;
            long elapsedTicks = Sandbox.Game.Debugging.MyPerformanceCounter.ElapsedTicks;
            this.UpdateInternal();
            this.FrameTimeTicks = Sandbox.Game.Debugging.MyPerformanceCounter.ElapsedTicks - elapsedTicks;
            float seconds = (float) new MyTimeSpan(this.FrameTimeTicks).Seconds;
            this.CPULoad = (seconds / 0.01666667f) * 100f;
            this.CPULoadSmooth = MathHelper.Smooth(this.CPULoad, this.CPULoadSmooth);
            this.CPUTimeSmooth = MathHelper.Smooth(seconds * 1000f, this.CPUTimeSmooth);
            float num3 = (float) new MyTimeSpan((long) Parallel.Scheduler.ReadAndClearExecutionTime()).Seconds;
            this.ThreadLoad = (num3 / 0.01666667f) * 100f;
            this.ThreadLoadSmooth = MathHelper.Smooth(this.ThreadLoad, this.ThreadLoadSmooth);
            this.ThreadTimeSmooth = MathHelper.Smooth(num3 * 1000f, this.ThreadTimeSmooth);
            if (MyFakes.PRECISE_SIM_SPEED)
            {
                m_targetMs = (float) Math.Max(16.666666666666668, Sandbox.Game.Debugging.MyPerformanceCounter.TicksToMs(Math.Min(Math.Max(this.m_renderLoop.TickPerFrame, this.UpdateCurrentFrame()), 10 * this.m_renderLoop.TickPerFrame)));
            }
            if (EnableSimSpeedLocking && MyFakes.ENABLE_SIMSPEED_LOCKING)
            {
                this.Lock(elapsedTicks);
            }
        }

        public void SetNextFrameDelayDelta(float delta)
        {
            this.m_renderLoop.SetNextFrameDelayDelta(delta);
        }

        protected abstract void UnloadData_UpdateThread();
        protected virtual void Update()
        {
            this.isFirstUpdateDone = true;
        }

        private long UpdateCurrentFrame()
        {
            if (this.m_lastFrameTiming.Count > 5)
            {
                this.m_lastFrameTiming.Dequeue();
            }
            this.m_lastFrameTiming.Enqueue(this.FrameTimeTicks);
            long num = 0x7fffffffffffffffL;
            long num2 = 0L;
            double num3 = 0.0;
            for (int i = 0; i < this.m_lastFrameTiming.Count; i++)
            {
                num = Math.Min(num, this.m_lastFrameTiming[i]);
                num2 = Math.Max(num2, this.m_lastFrameTiming[i]);
                num3 += (double) this.m_lastFrameTiming[i];
            }
            num3 /= (double) this.m_lastFrameTiming.Count;
            double num4 = (num2 - num) * 0.4f;
            long num5 = 0L;
            for (int j = 0; j < this.m_lastFrameTiming.Count; j++)
            {
                if (Math.Abs((double) (((double) this.m_lastFrameTiming[j]) - num3)) < num4)
                {
                    num5 = Math.Max(num2, this.m_lastFrameTiming[j]);
                }
            }
            return ((num5 != 0) ? num5 : ((long) num3));
        }

        private void UpdateInternal()
        {
            MyStatToken token;
            MySimpleProfiler.BeginBlock("UpdateFrame", MySimpleProfiler.ProfilingBlockType.INTERNAL);
            using (token = Stats.Generic.Measure("BeforeUpdate"))
            {
                MyRenderProxy.BeforeUpdate();
            }
            this.m_totalTime = this.m_gameTimer.Elapsed;
            this.m_updateCounter += (ulong) 1L;
            if (MySession.Static != null)
            {
                this.m_simulationTimeWithSpeed += MyTimeSpan.FromMilliseconds(16.666666666666668 * MyFakes.SIMULATION_SPEED);
            }
            bool enableNetworkPacketTracking = MyCompilationSymbols.EnableNetworkPacketTracking;
            this.Update();
            if (!IsDedicated)
            {
                this.PrepareForDraw();
            }
            using (token = Stats.Generic.Measure("AfterUpdate"))
            {
                this.AfterDraw();
            }
            MySimpleProfiler.End("UpdateFrame");
            MySimpleProfiler.Commit();
        }

        public MyTimeSpan DrawTime =>
            this.m_drawTime;

        public MyTimeSpan TotalTime =>
            this.m_totalTime;

        public ulong SimulationFrameCounter =>
            this.m_updateCounter;

        public MyTimeSpan SimulationTime =>
            MyTimeSpan.FromMilliseconds(this.m_updateCounter * 16.666666666666668);

        public MyTimeSpan SimulationTimeWithSpeed =>
            this.m_simulationTimeWithSpeed;

        public Thread UpdateThread { get; protected set; }

        public Thread DrawThread { get; protected set; }

        public float CPULoad { get; private set; }

        public float CPULoadSmooth { get; private set; }

        public float CPUTimeSmooth { get; private set; }

        public float ThreadLoad { get; private set; }

        public float ThreadLoadSmooth { get; private set; }

        public float ThreadTimeSmooth { get; private set; }

        public static float SimulationRatio =>
            (16.66667f / m_targetMs);

        public bool IsActive { get; private set; }

        public bool IsRunning { get; private set; }

        public bool IsFirstUpdateDone =>
            this.isFirstUpdateDone;

        public bool EnableMaxSpeed
        {
            get => 
                this.m_renderLoop.EnableMaxSpeed;
            set => 
                (this.m_renderLoop.EnableMaxSpeed = value);
        }
    }
}

