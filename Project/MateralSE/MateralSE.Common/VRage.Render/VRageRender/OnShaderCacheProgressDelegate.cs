namespace VRageRender
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void OnShaderCacheProgressDelegate(float percents, string file, string hash, string profile, string vertexLayout, string macros, string message, bool importantMessage);
}

