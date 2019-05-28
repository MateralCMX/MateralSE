namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct ColoredIcon
    {
        public string Icon;
        public Vector4 Color;
        public ColoredIcon(string icon, Vector4 color)
        {
            this.Icon = icon;
            this.Color = color;
        }
    }
}

