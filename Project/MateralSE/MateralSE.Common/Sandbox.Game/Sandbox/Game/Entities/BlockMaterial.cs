namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BlockMaterial
    {
        private MyExportModel.Material Base;
        private Color DiffuseColor;
    }
}

