namespace VRage.Game.Components
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyResourceSinkInfo
    {
        public MyDefinitionId ResourceTypeId;
        public float MaxRequiredInput;
        public Func<float> RequiredInputFunc;
    }
}

