namespace Sandbox.Engine.Platform.VideoMode
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyAspectRatio
    {
        public readonly MyAspectRatioEnum AspectRatioEnum;
        public readonly float AspectRatioNumber;
        public readonly string TextShort;
        public readonly bool IsTripleHead;
        public readonly bool IsSupported;
        public MyAspectRatio(bool isTripleHead, MyAspectRatioEnum aspectRatioEnum, float aspectRatioNumber, string textShort, bool isSupported)
        {
            this.IsTripleHead = isTripleHead;
            this.AspectRatioEnum = aspectRatioEnum;
            this.AspectRatioNumber = aspectRatioNumber;
            this.TextShort = textShort;
            this.IsSupported = isSupported;
        }
    }
}

