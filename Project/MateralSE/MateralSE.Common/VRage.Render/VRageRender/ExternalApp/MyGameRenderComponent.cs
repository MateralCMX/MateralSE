namespace VRageRender.ExternalApp
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Library.Utils;
    using VRageRender;

    public class MyGameRenderComponent : IDisposable
    {
        public void Dispose()
        {
            if (this.RenderThread != null)
            {
                this.Stop();
            }
            MyRenderProxy.DisposeDevice();
        }

        public void Start(MyGameTimer timer, InitHandler windowInitializer, MyRenderDeviceSettings? settingsToTry, MyRenderQualityEnum renderQuality, float maxFrameRate)
        {
            this.RenderThread = MyRenderThread.Start(timer, windowInitializer, settingsToTry, renderQuality, maxFrameRate);
        }

        public void StartSync(MyGameTimer timer, IMyRenderWindow window, MyRenderDeviceSettings? settings, MyRenderQualityEnum renderQuality, float maxFrameRate)
        {
            this.RenderThread = MyRenderThread.StartSync(timer, window, settings, renderQuality, maxFrameRate);
        }

        public void Stop()
        {
            this.RenderThread.Exit();
            this.RenderThread = null;
        }

        public MyRenderThread RenderThread { get; private set; }
    }
}

