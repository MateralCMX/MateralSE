namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct renderColoredTextureProperties
    {
        public string PathToSave_ColorAlpha;
        public string PathToSave_NormalGloss;
        public string TextureAddMaps;
        public string TextureAplhaMask;
        public string TextureColorMetal;
        public string TextureNormalGloss;
        public Vector3 ColorMaskHSV;
    }
}

