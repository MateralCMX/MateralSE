namespace VRageRender.ExternalApp
{
    using System;
    using VRageMath;
    using VRageRender;

    public interface IMyRenderWindow
    {
        void BeforeDraw();
        void OnDeactivate();
        void OnModeChanged(MyWindowModeEnum mode, int width, int height, Rectangle desktopBounds);
        void SetMouseCapture(bool capture);

        bool DrawEnabled { get; }

        IntPtr Handle { get; }
    }
}

