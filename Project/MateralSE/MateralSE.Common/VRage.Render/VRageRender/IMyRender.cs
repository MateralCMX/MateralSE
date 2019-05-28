namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Messages;

    public interface IMyRender
    {
        void Ansel_DrawScene();
        void ApplySettings(MyRenderDeviceSettings settings);
        MyRenderDeviceSettings CreateDevice(IntPtr windowHandle, MyRenderDeviceSettings? settingsToTry, out MyAdapterInfo[] adaptersList);
        void DisposeDevice();
        void Draw(bool draw = true);
        void DrawBegin();
        void DrawEnd();
        void EnqueueMessage(MyRenderMessageBase message, bool limitMaxQueueSize);
        void EnqueueOutputMessage(MyRenderMessageBase message);
        void GenerateShaderCache(bool clean, OnShaderCacheProgressDelegate onShaderCacheProgress);
        long GetAvailableTextureMemory();
        string GetLastExecutedAnnotation();
        MyRenderProfiler GetRenderProfiler();
        string GetStatistics();
        VideoState GetVideoState(uint id);
        void HandleFocusMessage(MyWindowFocusMessage msg);
        bool IsVideoValid(uint id);
        void LoadContent(MyRenderQualityEnum quality);
        void Present();
        void ProcessMessages();
        void ReloadContent(MyRenderQualityEnum quality);
        bool ResetDevice();
        void ResetEnvironmentProbes();
        void SetTimings(MyTimeSpan cpuDraw, MyTimeSpan cpuWait);
        bool SettingsChanged(MyRenderDeviceSettings settings);
        MyRenderDeviceCooperativeLevel TestDeviceCooperativeLevel();
        void UnloadContent();
        void UnloadData();

        bool IsSupported { get; }

        string RootDirectory { get; set; }

        string RootDirectoryEffects { get; set; }

        string RootDirectoryDebug { get; set; }

        MySharedData SharedData { get; }

        MyTimeSpan CurrentDrawTime { get; set; }

        MyLog Log { get; }

        FrameProcessStatusEnum FrameProcessStatus { get; }

        MyViewport MainViewport { get; }

        Vector2I BackBufferResolution { get; }

        MyMessageQueue OutputQueue { get; }

        uint GlobalMessageCounter { get; set; }
    }
}

