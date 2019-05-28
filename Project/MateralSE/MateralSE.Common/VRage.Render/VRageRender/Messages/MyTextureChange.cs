namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTextureChange
    {
        public string ColorMetalFileName;
        public string NormalGlossFileName;
        public string ExtensionsFileName;
        public string AlphamaskFileName;
        public bool IsDefault() => 
            ((this.ColorMetalFileName == null) && ((this.NormalGlossFileName == null) && ((this.ExtensionsFileName == null) && ReferenceEquals(this.AlphamaskFileName, null))));
    }
}

