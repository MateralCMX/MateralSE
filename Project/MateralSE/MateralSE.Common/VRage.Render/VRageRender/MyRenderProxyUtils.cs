namespace VRageRender
{
    using System;
    using VRageMath;

    public static class MyRenderProxyUtils
    {
        public static Vector2I GetFixedWindowResolution(Vector2I inResolution, MyAdapterInfo adapterInfo)
        {
            Vector2I vectori = new Vector2I(150);
            Vector2I vectori2 = new Vector2I(adapterInfo.DesktopBounds.Width, adapterInfo.DesktopBounds.Height) - vectori;
            vectori2 = Vector2I.Min(inResolution, vectori2);
            if (inResolution != vectori2)
            {
                Vector2I zero = Vector2I.Zero;
                foreach (MyDisplayMode mode in adapterInfo.SupportedDisplayModes)
                {
                    Vector2I vectori4 = new Vector2I(mode.Width, mode.Height);
                    if (((vectori4.X <= vectori2.X) && (vectori4.Y <= vectori2.Y)) && ((vectori4.X * vectori4.Y) > (zero.X * zero.Y)))
                    {
                        zero = vectori4;
                    }
                }
                if (zero != Vector2I.Zero)
                {
                    vectori2 = zero;
                }
            }
            return vectori2;
        }
    }
}

