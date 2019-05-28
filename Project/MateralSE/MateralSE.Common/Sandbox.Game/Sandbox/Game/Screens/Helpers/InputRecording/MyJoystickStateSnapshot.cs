namespace Sandbox.Game.Screens.Helpers.InputRecording
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [Obfuscation(Feature="cw symbol renaming", Exclude=true)]
    public class MyJoystickStateSnapshot
    {
        public int[] AccelerationSliders { get; set; }

        public int AccelerationX { get; set; }

        public int AccelerationY { get; set; }

        public int AccelerationZ { get; set; }

        public int AngularAccelerationX { get; set; }

        public int AngularAccelerationY { get; set; }

        public int AngularAccelerationZ { get; set; }

        public int AngularVelocityX { get; set; }

        public int AngularVelocityY { get; set; }

        public int AngularVelocityZ { get; set; }

        public List<int> Buttons { get; set; }

        public int[] ForceSliders { get; set; }

        public int ForceX { get; set; }

        public int ForceY { get; set; }

        public int ForceZ { get; set; }

        public int[] PointOfViewControllers { get; set; }

        public int RotationX { get; set; }

        public int RotationY { get; set; }

        public int RotationZ { get; set; }

        public int[] Sliders { get; set; }

        public int TorqueX { get; set; }

        public int TorqueY { get; set; }

        public int TorqueZ { get; set; }

        public int[] VelocitySliders { get; set; }

        public int VelocityX { get; set; }

        public int VelocityY { get; set; }

        public int VelocityZ { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }
    }
}

