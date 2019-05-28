namespace VRage.Entities.Components
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Common;

    public class VoxelPostprocessingAttribute : MyFactoryTagAttribute
    {
        public VoxelPostprocessingAttribute(Type objectBuilderType, bool mainBuilder = true) : base(objectBuilderType, mainBuilder)
        {
        }
    }
}

