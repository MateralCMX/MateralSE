namespace Sandbox.Game.Screens.Helpers.InputRecording
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [Obfuscation(Feature="cw symbol renaming", Exclude=true)]
    public class MyInputSnapshot
    {
        public MyMouseSnapshot MouseSnapshot { get; set; }

        public List<byte> KeyboardSnapshot { get; set; }

        public List<char> KeyboardSnapshotText { get; set; }

        public MyJoystickStateSnapshot JoystickSnapshot { get; set; }

        public int SnapshotTimestamp { get; set; }

        public Vector2 MouseCursorPosition { get; set; }
    }
}

