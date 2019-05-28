namespace VRageRender
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MyRenderFlagsExtensions
    {
        public static bool HasFlags(this RenderFlags renderFlags, RenderFlags flags) => 
            ((renderFlags & flags) == flags);
    }
}

