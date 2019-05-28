namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    internal static class MyVector4Interpolator
    {
        public static void Lerp(ref Vector4 val1, ref Vector4 val2, float time, out Vector4 value)
        {
            value.X = val1.X + ((val2.X - val1.X) * time);
            value.Y = val1.Y + ((val2.Y - val1.Y) * time);
            value.Z = val1.Z + ((val2.Z - val1.Z) * time);
            value.W = val1.W + ((val2.W - val1.W) * time);
        }
    }
}

