namespace Sandbox.Engine.Analytics
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPlanetNamesData
    {
        public string planetName;
        public string planetType;
    }
}

