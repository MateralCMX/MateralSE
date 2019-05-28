namespace VRageRender
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyDecalMaterialDesc
    {
        [ProtoMember(0x55)]
        public string NormalmapTexture;
        [ProtoMember(0x58)]
        public string ColorMetalTexture;
        [ProtoMember(0x5b)]
        public string AlphamaskTexture;
        [ProtoMember(0x5e)]
        public string ExtensionsTexture;
    }
}

