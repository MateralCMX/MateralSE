namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;

    internal static class MyFloatInterpolator
    {
        public static void Lerp(ref float val1, ref float val2, float time, out float value)
        {
            value = val1 + ((val2 - val1) * time);
        }
    }
}

