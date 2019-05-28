namespace Sandbox.Engine.Voxels.Planet
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class MyPlanetMapProviderAttribute : MyFactoryTagAttribute
    {
        public MyPlanetMapProviderAttribute(Type objectBuilderType, bool mainBuilder = true) : base(objectBuilderType, mainBuilder)
        {
        }
    }
}

