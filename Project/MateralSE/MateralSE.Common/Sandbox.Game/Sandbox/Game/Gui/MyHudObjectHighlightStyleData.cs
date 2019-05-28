namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyHudObjectHighlightStyleData
    {
        public string AtlasTexture;
        public MyAtlasTextureCoordinate TextureCoord;
    }
}

