namespace Sandbox.Game.Localization
{
    using System;
    using VRage;
    using VRage.Input;

    public class MyKeysToString : IMyControlNameLookup
    {
        private readonly string[] m_systemKeyNamesLower = new string[0x100];
        private readonly string[] m_systemKeyNamesUpper = new string[0x100];
        private readonly MyUtilKeyToString[] m_keyToString;

        public MyKeysToString()
        {
            MyUtilKeyToString[] textArray1 = new MyUtilKeyToString[0x53];
            textArray1[0] = new MyUtilKeyToStringLocalized(MyKeys.Left, MyCommonTexts.KeysLeft);
            textArray1[1] = new MyUtilKeyToStringLocalized(MyKeys.Right, MyCommonTexts.KeysRight);
            textArray1[2] = new MyUtilKeyToStringLocalized(MyKeys.Up, MyCommonTexts.KeysUp);
            textArray1[3] = new MyUtilKeyToStringLocalized(MyKeys.Down, MyCommonTexts.KeysDown);
            textArray1[4] = new MyUtilKeyToStringLocalized(MyKeys.Home, MyCommonTexts.KeysHome);
            textArray1[5] = new MyUtilKeyToStringLocalized(MyKeys.End, MyCommonTexts.KeysEnd);
            textArray1[6] = new MyUtilKeyToStringLocalized(MyKeys.Delete, MyCommonTexts.KeysDelete);
            textArray1[7] = new MyUtilKeyToStringLocalized(MyKeys.Back, MyCommonTexts.KeysBackspace);
            textArray1[8] = new MyUtilKeyToStringLocalized(MyKeys.Insert, MyCommonTexts.KeysInsert);
            textArray1[9] = new MyUtilKeyToStringLocalized(MyKeys.PageDown, MyCommonTexts.KeysPageDown);
            textArray1[10] = new MyUtilKeyToStringLocalized(MyKeys.PageUp, MyCommonTexts.KeysPageUp);
            textArray1[11] = new MyUtilKeyToStringLocalized(MyKeys.LeftAlt, MyCommonTexts.KeysLeftAlt);
            textArray1[12] = new MyUtilKeyToStringLocalized(MyKeys.LeftControl, MyCommonTexts.KeysLeftControl);
            textArray1[13] = new MyUtilKeyToStringLocalized(MyKeys.LeftShift, MyCommonTexts.KeysLeftShift);
            textArray1[14] = new MyUtilKeyToStringLocalized(MyKeys.RightAlt, MyCommonTexts.KeysRightAlt);
            textArray1[15] = new MyUtilKeyToStringLocalized(MyKeys.RightControl, MyCommonTexts.KeysRightControl);
            textArray1[0x10] = new MyUtilKeyToStringLocalized(MyKeys.RightShift, MyCommonTexts.KeysRightShift);
            textArray1[0x11] = new MyUtilKeyToStringLocalized(MyKeys.CapsLock, MyCommonTexts.KeysCapsLock);
            textArray1[0x12] = new MyUtilKeyToStringLocalized(MyKeys.Enter, MyCommonTexts.KeysEnter);
            textArray1[0x13] = new MyUtilKeyToStringLocalized(MyKeys.Tab, MyCommonTexts.KeysTab);
            textArray1[20] = new MyUtilKeyToStringLocalized(MyKeys.OemOpenBrackets, MyCommonTexts.KeysOpenBracket);
            textArray1[0x15] = new MyUtilKeyToStringLocalized(MyKeys.OemCloseBrackets, MyCommonTexts.KeysCloseBracket);
            textArray1[0x16] = new MyUtilKeyToStringLocalized(MyKeys.Multiply, MyCommonTexts.KeysMultiply);
            textArray1[0x17] = new MyUtilKeyToStringLocalized(MyKeys.Subtract, MyCommonTexts.KeysSubtract);
            textArray1[0x18] = new MyUtilKeyToStringLocalized(MyKeys.Add, MyCommonTexts.KeysAdd);
            textArray1[0x19] = new MyUtilKeyToStringLocalized(MyKeys.Divide, MyCommonTexts.KeysDivide);
            textArray1[0x1a] = new MyUtilKeyToStringLocalized(MyKeys.NumPad0, MyCommonTexts.KeysNumPad0);
            textArray1[0x1b] = new MyUtilKeyToStringLocalized(MyKeys.NumPad1, MyCommonTexts.KeysNumPad1);
            textArray1[0x1c] = new MyUtilKeyToStringLocalized(MyKeys.NumPad2, MyCommonTexts.KeysNumPad2);
            textArray1[0x1d] = new MyUtilKeyToStringLocalized(MyKeys.NumPad3, MyCommonTexts.KeysNumPad3);
            textArray1[30] = new MyUtilKeyToStringLocalized(MyKeys.NumPad4, MyCommonTexts.KeysNumPad4);
            textArray1[0x1f] = new MyUtilKeyToStringLocalized(MyKeys.NumPad5, MyCommonTexts.KeysNumPad5);
            textArray1[0x20] = new MyUtilKeyToStringLocalized(MyKeys.NumPad6, MyCommonTexts.KeysNumPad6);
            textArray1[0x21] = new MyUtilKeyToStringLocalized(MyKeys.NumPad7, MyCommonTexts.KeysNumPad7);
            textArray1[0x22] = new MyUtilKeyToStringLocalized(MyKeys.NumPad8, MyCommonTexts.KeysNumPad8);
            textArray1[0x23] = new MyUtilKeyToStringLocalized(MyKeys.NumPad9, MyCommonTexts.KeysNumPad9);
            textArray1[0x24] = new MyUtilKeyToStringLocalized(MyKeys.Decimal, MyCommonTexts.KeysDecimal);
            textArray1[0x25] = new MyUtilKeyToStringLocalized(MyKeys.OemBackslash, MyCommonTexts.KeysBackslash);
            textArray1[0x26] = new MyUtilKeyToStringLocalized(MyKeys.OemComma, MyCommonTexts.KeysComma);
            textArray1[0x27] = new MyUtilKeyToStringLocalized(MyKeys.OemMinus, MyCommonTexts.KeysMinus);
            textArray1[40] = new MyUtilKeyToStringLocalized(MyKeys.OemPeriod, MyCommonTexts.KeysPeriod);
            textArray1[0x29] = new MyUtilKeyToStringLocalized(MyKeys.OemPipe, MyCommonTexts.KeysPipe);
            textArray1[0x2a] = new MyUtilKeyToStringLocalized(MyKeys.OemPlus, MyCommonTexts.KeysPlus);
            textArray1[0x2b] = new MyUtilKeyToStringLocalized(MyKeys.OemQuestion, MyCommonTexts.KeysQuestion);
            textArray1[0x2c] = new MyUtilKeyToStringLocalized(MyKeys.OemQuotes, MyCommonTexts.KeysQuotes);
            textArray1[0x2d] = new MyUtilKeyToStringLocalized(MyKeys.OemSemicolon, MyCommonTexts.KeysSemicolon);
            textArray1[0x2e] = new MyUtilKeyToStringLocalized(MyKeys.OemTilde, MyCommonTexts.KeysTilde);
            textArray1[0x2f] = new MyUtilKeyToStringLocalized(MyKeys.Space, MyCommonTexts.KeysSpace);
            textArray1[0x30] = new MyUtilKeyToStringLocalized(MyKeys.Pause, MyCommonTexts.KeysPause);
            textArray1[0x31] = new MyUtilKeyToStringSimple(MyKeys.D0, "0");
            textArray1[50] = new MyUtilKeyToStringSimple(MyKeys.D1, "1");
            textArray1[0x33] = new MyUtilKeyToStringSimple(MyKeys.D2, "2");
            textArray1[0x34] = new MyUtilKeyToStringSimple(MyKeys.D3, "3");
            textArray1[0x35] = new MyUtilKeyToStringSimple(MyKeys.D4, "4");
            textArray1[0x36] = new MyUtilKeyToStringSimple(MyKeys.D5, "5");
            textArray1[0x37] = new MyUtilKeyToStringSimple(MyKeys.D6, "6");
            textArray1[0x38] = new MyUtilKeyToStringSimple(MyKeys.D7, "7");
            textArray1[0x39] = new MyUtilKeyToStringSimple(MyKeys.D8, "8");
            textArray1[0x3a] = new MyUtilKeyToStringSimple(MyKeys.D9, "9");
            textArray1[0x3b] = new MyUtilKeyToStringSimple(MyKeys.F1, "F1");
            textArray1[60] = new MyUtilKeyToStringSimple(MyKeys.F2, "F2");
            textArray1[0x3d] = new MyUtilKeyToStringSimple(MyKeys.F3, "F3");
            textArray1[0x3e] = new MyUtilKeyToStringSimple(MyKeys.F4, "F4");
            textArray1[0x3f] = new MyUtilKeyToStringSimple(MyKeys.F5, "F5");
            textArray1[0x40] = new MyUtilKeyToStringSimple(MyKeys.F6, "F6");
            textArray1[0x41] = new MyUtilKeyToStringSimple(MyKeys.F7, "F7");
            textArray1[0x42] = new MyUtilKeyToStringSimple(MyKeys.F8, "F8");
            textArray1[0x43] = new MyUtilKeyToStringSimple(MyKeys.F9, "F9");
            textArray1[0x44] = new MyUtilKeyToStringSimple(MyKeys.F10, "F10");
            textArray1[0x45] = new MyUtilKeyToStringSimple(MyKeys.F11, "F11");
            textArray1[70] = new MyUtilKeyToStringSimple(MyKeys.F12, "F12");
            textArray1[0x47] = new MyUtilKeyToStringSimple(MyKeys.F13, "F13");
            textArray1[0x48] = new MyUtilKeyToStringSimple(MyKeys.F14, "F14");
            textArray1[0x49] = new MyUtilKeyToStringSimple(MyKeys.F15, "F15");
            textArray1[0x4a] = new MyUtilKeyToStringSimple(MyKeys.F16, "F16");
            textArray1[0x4b] = new MyUtilKeyToStringSimple(MyKeys.F17, "F17");
            textArray1[0x4c] = new MyUtilKeyToStringSimple(MyKeys.F18, "F18");
            textArray1[0x4d] = new MyUtilKeyToStringSimple(MyKeys.F19, "F19");
            textArray1[0x4e] = new MyUtilKeyToStringSimple(MyKeys.F20, "F20");
            textArray1[0x4f] = new MyUtilKeyToStringSimple(MyKeys.F21, "F21");
            textArray1[80] = new MyUtilKeyToStringSimple(MyKeys.F22, "F22");
            textArray1[0x51] = new MyUtilKeyToStringSimple(MyKeys.F23, "F23");
            textArray1[0x52] = new MyUtilKeyToStringSimple(MyKeys.F24, "F24");
            this.m_keyToString = textArray1;
            for (int i = 0; i < this.m_systemKeyNamesLower.Length; i++)
            {
                this.m_systemKeyNamesLower[i] = ((char) i).ToString().ToLower();
                char ch = (char) i;
                this.m_systemKeyNamesUpper[i] = ch.ToString().ToUpper();
            }
        }

        string IMyControlNameLookup.GetKeyName(MyKeys key)
        {
            if (((int) key) >= this.m_systemKeyNamesUpper.Length)
            {
                return null;
            }
            string name = this.m_systemKeyNamesUpper[(int) key];
            int index = 0;
            while (true)
            {
                if (index < this.m_keyToString.Length)
                {
                    if (this.m_keyToString[index].Key != key)
                    {
                        index++;
                        continue;
                    }
                    name = this.m_keyToString[index].Name;
                }
                return name;
            }
        }

        string IMyControlNameLookup.GetName(MyJoystickAxesEnum joystickAxis)
        {
            switch (joystickAxis)
            {
                case MyJoystickAxesEnum.Xpos:
                    return "JX+";

                case MyJoystickAxesEnum.Xneg:
                    return "JX-";

                case MyJoystickAxesEnum.Ypos:
                    return "JY+";

                case MyJoystickAxesEnum.Yneg:
                    return "JY-";

                case MyJoystickAxesEnum.Zpos:
                    return "JZ+";

                case MyJoystickAxesEnum.Zneg:
                    return "JZ-";

                case MyJoystickAxesEnum.RotationXpos:
                    return MyTexts.GetString(MyCommonTexts.JoystickRotationXpos);

                case MyJoystickAxesEnum.RotationXneg:
                    return MyTexts.GetString(MyCommonTexts.JoystickRotationXneg);

                case MyJoystickAxesEnum.RotationYpos:
                    return MyTexts.GetString(MyCommonTexts.JoystickRotationYpos);

                case MyJoystickAxesEnum.RotationYneg:
                    return MyTexts.GetString(MyCommonTexts.JoystickRotationYneg);

                case MyJoystickAxesEnum.RotationZpos:
                    return MyTexts.GetString(MyCommonTexts.JoystickRotationZpos);

                case MyJoystickAxesEnum.RotationZneg:
                    return MyTexts.GetString(MyCommonTexts.JoystickRotationZneg);

                case MyJoystickAxesEnum.Slider1pos:
                    return MyTexts.GetString(MyCommonTexts.JoystickSlider1pos);

                case MyJoystickAxesEnum.Slider1neg:
                    return MyTexts.GetString(MyCommonTexts.JoystickSlider1neg);

                case MyJoystickAxesEnum.Slider2pos:
                    return MyTexts.GetString(MyCommonTexts.JoystickSlider2pos);

                case MyJoystickAxesEnum.Slider2neg:
                    return MyTexts.GetString(MyCommonTexts.JoystickSlider2neg);
            }
            return "";
        }

        string IMyControlNameLookup.GetName(MyJoystickButtonsEnum joystickButton)
        {
            if (joystickButton == MyJoystickButtonsEnum.None)
            {
                return "";
            }
            switch (joystickButton)
            {
                case MyJoystickButtonsEnum.JDLeft:
                    return MyTexts.GetString(MyCommonTexts.JoystickButtonLeft);

                case MyJoystickButtonsEnum.JDRight:
                    return MyTexts.GetString(MyCommonTexts.JoystickButtonRight);

                case MyJoystickButtonsEnum.JDUp:
                    return MyTexts.GetString(MyCommonTexts.JoystickButtonUp);

                case MyJoystickButtonsEnum.JDDown:
                    return MyTexts.GetString(MyCommonTexts.JoystickButtonDown);
            }
            return ("JB" + (((int) joystickButton) - 4));
        }

        string IMyControlNameLookup.GetName(MyMouseButtonsEnum button)
        {
            switch (button)
            {
                case MyMouseButtonsEnum.Left:
                    return MyTexts.GetString(MyCommonTexts.LeftMouseButton);

                case MyMouseButtonsEnum.Middle:
                    return MyTexts.GetString(MyCommonTexts.MiddleMouseButton);

                case MyMouseButtonsEnum.Right:
                    return MyTexts.GetString(MyCommonTexts.RightMouseButton);

                case MyMouseButtonsEnum.XButton1:
                    return MyTexts.GetString(MyCommonTexts.MouseXButton1);

                case MyMouseButtonsEnum.XButton2:
                    return MyTexts.GetString(MyCommonTexts.MouseXButton2);
            }
            return MyTexts.GetString(MySpaceTexts.Blank);
        }

        string IMyControlNameLookup.UnassignedText =>
            MyTexts.GetString(MyCommonTexts.UnknownControl_Unassigned);
    }
}

