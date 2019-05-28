namespace Sandbox.Game.World
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class MyEnvironmentalParticleLogicTypeAttribute : MyFactoryTagAttribute
    {
        public MyEnvironmentalParticleLogicTypeAttribute(Type objectBuilderType, bool mainBuilder = true) : base(objectBuilderType, mainBuilder)
        {
        }
    }
}

