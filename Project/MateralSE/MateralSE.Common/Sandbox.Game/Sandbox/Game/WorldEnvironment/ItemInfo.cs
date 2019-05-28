namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct ItemInfo
    {
        public Vector3 Position;
        public short DefinitionIndex;
        public short ModelIndex;
        public Quaternion Rotation;
        public override string ToString() => 
            $"Model: {this.ModelIndex}; Def: {this.DefinitionIndex}";
    }
}

