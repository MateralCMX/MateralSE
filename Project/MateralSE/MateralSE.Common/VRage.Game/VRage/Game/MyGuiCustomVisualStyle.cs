namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyGuiCustomVisualStyle
    {
        [ProtoMember(0x1a)]
        public string NormalTexture;
        [ProtoMember(0x1c)]
        public string HighlightTexture;
        [ProtoMember(30)]
        public Vector2 Size;
        [ProtoMember(0x20)]
        public string NormalFont;
        [ProtoMember(0x22)]
        public string HighlightFont;
        [ProtoMember(0x24)]
        public float HorizontalPadding;
        [ProtoMember(0x26)]
        public float VerticalPadding;
    }
}

