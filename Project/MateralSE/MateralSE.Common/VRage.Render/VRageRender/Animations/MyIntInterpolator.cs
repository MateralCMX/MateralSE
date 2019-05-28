namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;

    internal static class MyIntInterpolator
    {
        public static void Lerp(ref int val1, ref int val2, float time, out int value)
        {
            value = val1 + ((int) ((val2 - val1) * time));
        }

        public static void Switch(ref int val1, ref int val2, float time, out int value)
        {
            value = (time < 0.5f) ? val1 : val2;
        }
    }
}

