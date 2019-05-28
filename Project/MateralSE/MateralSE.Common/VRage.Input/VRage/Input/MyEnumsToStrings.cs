namespace VRage.Input
{
    using System;

    public static class MyEnumsToStrings
    {
        public static string[] GuiInputDeviceEnum = new string[] { "None", "Keyboard", "Mouse", "Joystick", "JoystickAxis", "KeyboardSecond" };
        public static string[] MouseButtonsEnum = new string[] { "None", "Left", "Middle", "Right", "XButton1", "XButton2" };
        public static string[] JoystickButtonsEnum;
        public static string[] JoystickAxesEnum;
        public static string[] ControlTypeEnum;

        static MyEnumsToStrings()
        {
            string[] textArray3 = new string[0x15];
            textArray3[0] = "None";
            textArray3[1] = "JDLeft";
            textArray3[2] = "JDRight";
            textArray3[3] = "JDUp";
            textArray3[4] = "JDDown";
            textArray3[5] = "J01";
            textArray3[6] = "J02";
            textArray3[7] = "J03";
            textArray3[8] = "J04";
            textArray3[9] = "J05";
            textArray3[10] = "J06";
            textArray3[11] = "J07";
            textArray3[12] = "J08";
            textArray3[13] = "J09";
            textArray3[14] = "J10";
            textArray3[15] = "J11";
            textArray3[0x10] = "J12";
            textArray3[0x11] = "J13";
            textArray3[0x12] = "J14";
            textArray3[0x13] = "J15";
            textArray3[20] = "J16";
            JoystickButtonsEnum = textArray3;
            string[] textArray4 = new string[0x11];
            textArray4[0] = "None";
            textArray4[1] = "JXAxis+";
            textArray4[2] = "JXAxis-";
            textArray4[3] = "JYAxis+";
            textArray4[4] = "JYAxis-";
            textArray4[5] = "JZAxis+";
            textArray4[6] = "JZAxis-";
            textArray4[7] = "JXRotation+";
            textArray4[8] = "JXRotation-";
            textArray4[9] = "JYRotation+";
            textArray4[10] = "JYRotation-";
            textArray4[11] = "JZRotation+";
            textArray4[12] = "JZRotation-";
            textArray4[13] = "JSlider1+";
            textArray4[14] = "JSlider1-";
            textArray4[15] = "JSlider2+";
            textArray4[0x10] = "JSlider2-";
            JoystickAxesEnum = textArray4;
            ControlTypeEnum = new string[] { "General", "Navigation", "Communications", "Weapons", "SpecialWeapons", "Systems1", "Systems2", "Editor" };
        }
    }
}

