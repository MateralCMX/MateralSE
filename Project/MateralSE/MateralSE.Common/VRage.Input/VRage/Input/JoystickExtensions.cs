namespace VRage.Input
{
    using SharpDX.DirectInput;
    using System;
    using System.Runtime.CompilerServices;

    internal static class JoystickExtensions
    {
        public static bool IsPressed(this JoystickState state, int button) => 
            state.Buttons[button];

        public static bool IsReleased(this JoystickState state, int button) => 
            !state.IsPressed(button);
    }
}

