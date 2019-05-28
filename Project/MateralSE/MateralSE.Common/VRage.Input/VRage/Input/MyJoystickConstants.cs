namespace VRage.Input
{
    using System;

    internal static class MyJoystickConstants
    {
        public const int MAX_AXIS = 0xffff;
        public const int MIN_AXIS = 0;
        public const int CENTER_AXIS = 0x7fff;
        public const float ANALOG_PRESSED_THRESHOLD = 0.5f;
        public const int MAXIMUM_BUTTON_COUNT = 0x10;
        public const bool BUTTON_JOYSTICK = true;
        public const float JOYSTICK_AS_MOUSE_MULTIPLIER = 4f;
    }
}

