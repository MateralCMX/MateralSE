namespace VRageRender.ExternalApp
{
    using SharpDX.Windows;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Security;
    using System.Threading;
    using System.Windows.Forms;
    using VRage;
    using VRage.Ansel;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Library.Exceptions;
    using VRage.Library.Utils;
    using VRage.Stats;
    using VRage.Utils;
    using VRageRender;
    using VRageRender.Utils;

    public class MyRenderThread
    {
        private readonly MyGameTimer m_timer;
        private readonly WaitForTargetFrameRate m_waiter;
        private MyTimeSpan m_messageProcessingStart;
        private MyTimeSpan m_frameStart;
        private MyTimeSpan m_appEventsTime;
        private int m_stopped;
        private IMyRenderWindow m_renderWindow;
        private MyRenderQualityEnum m_currentQuality;
        private Control m_form;
        private MyRenderDeviceSettings m_settings;
        private MyRenderDeviceSettings? m_newSettings;
        private int m_newQuality = -1;
        private readonly MyConcurrentQueue<Action> m_invokeQueue = new MyConcurrentQueue<Action>(0x10);
        public readonly Thread SystemThread;
        [CompilerGenerated]
        private Action BeforeDraw;
        [CompilerGenerated]
        private SizeChangedHandler SizeChanged;
        private readonly bool m_separateThread;
        private readonly MyConcurrentQueue<EventWaitHandle> m_debugWaitForPresentHandles = new MyConcurrentQueue<EventWaitHandle>(0x10);
        private int m_debugWaitForPresentHandleCount;
        private MyAdapterInfo[] m_adapterList;
        private MyTimeSpan m_waitStart;
        private MyTimeSpan m_drawStart;

        public event Action BeforeDraw
        {
            [CompilerGenerated] add
            {
                Action beforeDraw = this.BeforeDraw;
                while (true)
                {
                    Action a = beforeDraw;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    beforeDraw = Interlocked.CompareExchange<Action>(ref this.BeforeDraw, action3, a);
                    if (ReferenceEquals(beforeDraw, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action beforeDraw = this.BeforeDraw;
                while (true)
                {
                    Action source = beforeDraw;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    beforeDraw = Interlocked.CompareExchange<Action>(ref this.BeforeDraw, action3, source);
                    if (ReferenceEquals(beforeDraw, source))
                    {
                        return;
                    }
                }
            }
        }

        public event SizeChangedHandler SizeChanged
        {
            [CompilerGenerated] add
            {
                SizeChangedHandler sizeChanged = this.SizeChanged;
                while (true)
                {
                    SizeChangedHandler a = sizeChanged;
                    SizeChangedHandler handler3 = (SizeChangedHandler) Delegate.Combine(a, value);
                    sizeChanged = Interlocked.CompareExchange<SizeChangedHandler>(ref this.SizeChanged, handler3, a);
                    if (ReferenceEquals(sizeChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                SizeChangedHandler sizeChanged = this.SizeChanged;
                while (true)
                {
                    SizeChangedHandler source = sizeChanged;
                    SizeChangedHandler handler3 = (SizeChangedHandler) Delegate.Remove(source, value);
                    sizeChanged = Interlocked.CompareExchange<SizeChangedHandler>(ref this.SizeChanged, handler3, source);
                    if (ReferenceEquals(sizeChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        private MyRenderThread(MyGameTimer timer, bool separateThread, float maxFrameRate)
        {
            this.m_timer = timer;
            this.m_waiter = new WaitForTargetFrameRate(timer, maxFrameRate);
            this.m_separateThread = separateThread;
            if (!separateThread)
            {
                this.SystemThread = Thread.CurrentThread;
            }
            else
            {
                this.SystemThread = new Thread(new ParameterizedThreadStart(this.RenderThreadStart));
                this.SystemThread.IsBackground = true;
                this.SystemThread.Name = "Render thread";
                this.SystemThread.CurrentCulture = CultureInfo.InvariantCulture;
                this.SystemThread.CurrentUICulture = CultureInfo.InvariantCulture;
            }
        }

        private void ApplySettingsChanges()
        {
            if (MyRenderProxy.TestDeviceCooperativeLevel() == MyRenderDeviceCooperativeLevel.Ok)
            {
                int num = Interlocked.Exchange(ref this.m_newQuality, -1);
                if (num != -1)
                {
                    this.m_currentQuality = (MyRenderQualityEnum) num;
                }
                if ((this.m_newSettings != null) && MyRenderProxy.SettingsChanged(this.m_newSettings.Value))
                {
                    this.m_settings = this.m_newSettings.Value;
                    this.m_newSettings = null;
                    this.UnloadContent();
                    MyRenderProxy.ApplySettings(this.m_settings);
                    this.LoadContent();
                    this.UpdateSize();
                }
                else if (num != -1)
                {
                    MyRenderProxy.ReloadContent(this.m_currentQuality);
                }
            }
        }

        public void Deactivate()
        {
            this.m_renderWindow.OnDeactivate();
        }

        public void DebugAddWaitingForPresent(EventWaitHandle handle)
        {
            this.m_debugWaitForPresentHandles.Enqueue(handle);
        }

        private void DeviceReset()
        {
            this.UnloadContent();
            if (MyRenderProxy.ResetDevice())
            {
                this.LoadContent();
            }
        }

        private void DoAfterPresent()
        {
            for (int i = 0; i < this.m_debugWaitForPresentHandleCount; i++)
            {
                EventWaitHandle handle;
                if (this.m_debugWaitForPresentHandles.TryDequeue(out handle) && (handle != null))
                {
                    handle.Set();
                }
            }
            this.m_debugWaitForPresentHandleCount = 0;
        }

        private void DoBeforePresent()
        {
            this.m_debugWaitForPresentHandleCount = this.m_debugWaitForPresentHandles.Count;
        }

        private void Draw()
        {
            MyRenderProxy.DrawBegin();
            MyRenderProxy.Draw();
            MyRenderProxy.GetRenderProfiler().Draw("Draw", 0x23c, @"E:\Repo1\Sources\VRage.Render\ExternalApp\MyRenderThread.cs");
            MyRenderProxy.DrawEnd();
        }

        public void Exit()
        {
            if (Interlocked.Exchange(ref this.m_stopped, 1) != 1)
            {
                if (this.SystemThread == null)
                {
                    this.UnloadContent();
                    MyRenderProxy.DisposeDevice();
                }
                else
                {
                    try
                    {
                        if (!this.m_form.IsDisposed)
                        {
                            this.m_form.Invoke(new Action(this.OnExit));
                        }
                    }
                    catch
                    {
                    }
                    if (!ReferenceEquals(Thread.CurrentThread, this.SystemThread))
                    {
                        this.SystemThread.Join();
                    }
                }
            }
        }

        public void Invoke(Action action)
        {
            this.m_invokeQueue.Enqueue(action);
        }

        private void LoadContent()
        {
            MyRenderProxy.LoadContent(this.m_currentQuality);
        }

        private void OnExit()
        {
            this.m_form.Dispose();
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void RenderCallback(bool async)
        {
            try
            {
                this.RenderFrame(async);
            }
            catch (Exception exception)
            {
                MyMiniDump.CollectExceptionDump(exception, MyFileSystem.UserDataPath);
                string msg = string.Format("Exception in render!\n\nAftermath: {0}\nException: {2}\nStatistics: {1}", MyRenderProxy.GetLastExecutedAnnotation(), MyRenderProxy.GetStatistics(), exception);
                MyLog.Default.WriteLine(msg);
                MyLog.Default.Flush();
                string text = "Graphics device driver has crashed.\n\nYour card is probably overheating or driver is malfunctioning. Please, update your graphics drivers and remove any overclocking";
                MyMessageBox.Show("Game crashed", text);
                throw;
            }
        }

        private void RenderFrame(bool async)
        {
            if (this.SystemThread != null)
            {
                ThreadPriority priority = MyRenderProxy.Settings.RenderThreadHighPriority ? ThreadPriority.AboveNormal : ThreadPriority.Normal;
                if (this.SystemThread.Priority != priority)
                {
                    this.SystemThread.Priority = priority;
                }
            }
            if (MyAnsel.IsAnselCaptureRunning)
            {
                MyRenderProxy.Ansel_DrawScene();
                MyRenderProxy.Present();
            }
            else
            {
                Action action;
                if (this.m_messageProcessingStart != MyTimeSpan.Zero)
                {
                    MyTimeSpan span1 = this.m_timer.Elapsed - this.m_messageProcessingStart;
                    this.m_waiter.Wait();
                    bool flag1 = async;
                }
                MySimpleProfiler.BeginBlock("RenderFrame", MySimpleProfiler.ProfilingBlockType.RENDER);
                this.m_drawStart = this.m_timer.Elapsed;
                MyTimeSpan cpuWait = this.m_drawStart - this.m_waitStart;
                this.m_frameStart = this.m_timer.Elapsed;
                switch (MyRenderProxy.FrameProcessStatus)
                {
                }
                while (this.m_invokeQueue.TryDequeue(out action))
                {
                    action();
                }
                this.ApplySettingsChanges();
                MyRenderStats.Generic.WriteFormat("Available GPU memory: {0} MB", (float) ((((float) MyRenderProxy.GetAvailableTextureMemory()) / 1024f) / 1024f), MyStatTypeEnum.CurrentValue, 300, 2, -1);
                MyRenderProxy.BeforeRender(new MyTimeSpan?(this.m_frameStart));
                this.m_renderWindow.BeforeDraw();
                if (this.BeforeDraw != null)
                {
                    this.BeforeDraw();
                }
                MyRenderDeviceCooperativeLevel level = MyRenderProxy.TestDeviceCooperativeLevel();
                if (!this.m_renderWindow.DrawEnabled)
                {
                    MyRenderProxy.ProcessMessages();
                }
                else if (level == MyRenderDeviceCooperativeLevel.Ok)
                {
                    this.Draw();
                }
                else
                {
                    MyRenderProxy.ProcessMessages();
                    if (level == MyRenderDeviceCooperativeLevel.Lost)
                    {
                        Thread.Sleep(20);
                    }
                    else if (level == MyRenderDeviceCooperativeLevel.NotReset)
                    {
                        Thread.Sleep(20);
                        this.DeviceReset();
                    }
                }
                MyRenderProxy.AfterRender();
                bool separateThread = this.m_separateThread;
                this.m_waitStart = this.m_timer.Elapsed;
                MyTimeSpan cpuDraw = this.m_waitStart - this.m_drawStart;
                MySimpleProfiler.End("RenderFrame");
                if ((level == MyRenderDeviceCooperativeLevel.Ok) && this.m_renderWindow.DrawEnabled)
                {
                    this.DoBeforePresent();
                    try
                    {
                        MyRenderProxy.Present();
                    }
                    catch (MyDeviceErrorException exception1)
                    {
                        MyRenderProxy.Error(exception1.Message, 0, true);
                        this.Exit();
                    }
                    this.DoAfterPresent();
                }
                MyRenderProxy.SetTimings(cpuDraw, cpuWait);
                this.m_messageProcessingStart = this.m_timer.Elapsed;
                if (MyRenderProxy.Settings.ForceSlowCPU)
                {
                    Thread.Sleep(200);
                }
                bool flag3 = async;
            }
        }

        private void RenderThreadStart(object param)
        {
            StartParams @params = (StartParams) param;
            this.m_renderWindow = @params.InitHandler();
            Control control = Control.FromHandle(this.m_renderWindow.Handle);
            this.m_settings = MyRenderProxy.CreateDevice(this, this.m_renderWindow.Handle, @params.SettingsToTry, out this.m_adapterList);
            if (this.m_settings.AdapterOrdinal != -1)
            {
                MyRenderProxy.SendCreatedDeviceSettings(this.m_settings);
                this.m_currentQuality = @params.RenderQuality;
                this.m_form = control;
                this.LoadContent();
                this.UpdateSize();
                if (MyRenderProxy.Settings.EnableAnsel)
                {
                    MyAnsel.Init(this.m_renderWindow.Handle, MyRenderProxy.Settings.EnableAnselWithSprites);
                }
                RenderLoop loop1 = new RenderLoop(this.m_form);
                loop1.UseApplicationDoEvents = false;
                using (RenderLoop loop = loop1)
                {
                    while (loop.NextFrame())
                    {
                        if (this.RenderUpdateSyncEvent != null)
                        {
                            this.RenderUpdateSyncEvent.WaitOne();
                        }
                        this.RenderCallback(true);
                    }
                }
                MyTimeSpan? updateTimestamp = null;
                MyRenderProxy.AfterUpdate(updateTimestamp);
                MyRenderProxy.BeforeUpdate();
                MyRenderProxy.ProcessMessages();
                this.UnloadContent();
                MyRenderProxy.DisposeDevice();
            }
        }

        public void SetMouseCapture(bool capture)
        {
            this.m_renderWindow.SetMouseCapture(capture);
        }

        public static MyRenderThread Start(MyGameTimer timer, InitHandler initHandler, MyRenderDeviceSettings? settingsToTry, MyRenderQualityEnum renderQuality, float maxFrameRate)
        {
            MyRenderThread thread1 = new MyRenderThread(timer, true, maxFrameRate);
            StartParams parameter = new StartParams();
            parameter.InitHandler = initHandler;
            parameter.SettingsToTry = settingsToTry;
            parameter.RenderQuality = renderQuality;
            thread1.SystemThread.Start(parameter);
            return thread1;
        }

        public static MyRenderThread StartSync(MyGameTimer timer, IMyRenderWindow renderWindow, MyRenderDeviceSettings? settingsToTry, MyRenderQualityEnum renderQuality, float maxFrameRate)
        {
            MyRenderThread thread1 = new MyRenderThread(timer, false, maxFrameRate);
            thread1.m_renderWindow = renderWindow;
            MyRenderThread renderThread = thread1;
            renderThread.m_settings = MyRenderProxy.CreateDevice(renderThread, renderWindow.Handle, settingsToTry, out renderThread.m_adapterList);
            MyRenderProxy.SendCreatedDeviceSettings(renderThread.m_settings);
            renderThread.m_currentQuality = renderQuality;
            renderThread.m_form = Control.FromHandle(renderWindow.Handle);
            renderThread.LoadContent();
            renderThread.UpdateSize();
            renderThread.m_form.Show();
            return renderThread;
        }

        public void SwitchQuality(MyRenderQualityEnum quality)
        {
            this.m_newQuality = (int) quality;
        }

        public void SwitchSettings(MyRenderDeviceSettings settings)
        {
            this.m_newSettings = new MyRenderDeviceSettings?(settings);
        }

        public void TickSync()
        {
            if (MyRenderProxy.EnableAppEventsCall)
            {
                if ((this.m_timer.Elapsed - this.m_appEventsTime).Milliseconds > 10.0)
                {
                    Application.DoEvents();
                    this.m_appEventsTime = this.m_timer.Elapsed;
                }
                Application.DoEvents();
            }
            this.RenderCallback(false);
        }

        private void UnloadContent()
        {
            MyRenderProxy.UnloadContent();
        }

        public void UpdateSize()
        {
            this.m_renderWindow.OnModeChanged(this.m_settings.WindowMode, this.m_settings.BackBufferWidth, this.m_settings.BackBufferHeight, this.m_adapterList[this.m_settings.AdapterOrdinal].DesktopBounds);
            SizeChangedHandler sizeChanged = this.SizeChanged;
            if (sizeChanged != null)
            {
                sizeChanged(MyRenderProxy.BackBufferResolution.X, MyRenderProxy.BackBufferResolution.Y, MyRenderProxy.MainViewport);
            }
        }

        public int CurrentAdapter =>
            this.m_settings.AdapterOrdinal;

        public MyRenderDeviceSettings CurrentSettings =>
            this.m_settings;

        public ManualResetEvent RenderUpdateSyncEvent { get; set; }

        private class StartParams
        {
            public VRageRender.ExternalApp.InitHandler InitHandler;
            public MyRenderDeviceSettings? SettingsToTry;
            public MyRenderQualityEnum RenderQuality;
        }
    }
}

