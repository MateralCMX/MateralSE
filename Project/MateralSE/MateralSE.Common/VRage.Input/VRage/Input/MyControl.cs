namespace VRage.Input
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.ModAPI;
    using VRage.Utils;

    public class MyControl : IMyControl
    {
        private const int DEFAULT_CAPACITY = 0x10;
        private static StringBuilder m_toStringCache = new StringBuilder(0x10);
        private Data m_data;

        public MyControl(MyControl other)
        {
            this.CopyFrom(other);
        }

        public MyControl(MyStringId controlId, MyStringId name, MyGuiControlTypeEnum controlType, MyMouseButtonsEnum? defaultControlMouse, MyKeys? defaultControlKey, MyStringId? helpText = new MyStringId?(), MyKeys? defaultControlKey2 = new MyKeys?(), MyStringId? description = new MyStringId?())
        {
            this.m_controlId = controlId;
            this.m_name = name;
            this.m_controlType = controlType;
            MyMouseButtonsEnum? nullable = defaultControlMouse;
            this.m_mouseButton = (nullable != null) ? nullable.GetValueOrDefault() : MyMouseButtonsEnum.None;
            MyKeys? nullable2 = defaultControlKey;
            this.m_keyboardKey = (nullable2 != null) ? nullable2.GetValueOrDefault() : MyKeys.None;
            nullable2 = defaultControlKey2;
            this.m_KeyboardKey2 = (nullable2 != null) ? nullable2.GetValueOrDefault() : MyKeys.None;
            this.m_data.Description = description;
        }

        public void AppendBoundButtonNames(ref StringBuilder output, MyGuiInputDeviceEnum device, string separator = null)
        {
            EnsureExists(ref output);
            switch (device)
            {
                case MyGuiInputDeviceEnum.Keyboard:
                    if (separator == null)
                    {
                        AppendName(ref output, this.m_keyboardKey);
                        return;
                    }
                    AppendName(ref output, this.m_keyboardKey, this.m_KeyboardKey2, separator);
                    return;

                case MyGuiInputDeviceEnum.Mouse:
                    AppendName(ref output, this.m_mouseButton);
                    break;

                case (MyGuiInputDeviceEnum.Keyboard | MyGuiInputDeviceEnum.Mouse):
                case ((MyGuiInputDeviceEnum) 4):
                    break;

                case MyGuiInputDeviceEnum.KeyboardSecond:
                    if (separator == null)
                    {
                        AppendName(ref output, this.m_KeyboardKey2);
                        return;
                    }
                    AppendName(ref output, this.m_keyboardKey, this.m_KeyboardKey2, separator);
                    return;

                default:
                    return;
            }
        }

        public void AppendBoundButtonNames(ref StringBuilder output, string separator = ", ", string unassignedText = null, bool includeSecondary = true)
        {
            EnsureExists(ref output);
            MyGuiInputDeviceEnum[] enumArray1 = new MyGuiInputDeviceEnum[] { MyGuiInputDeviceEnum.Keyboard, MyGuiInputDeviceEnum.Mouse };
            int num = 0;
            foreach (MyGuiInputDeviceEnum enum2 in enumArray1)
            {
                if (this.IsControlAssigned(enum2))
                {
                    if (num > 0)
                    {
                        output.Append(separator);
                    }
                    this.AppendBoundButtonNames(ref output, enum2, includeSecondary ? separator : null);
                    num++;
                }
            }
            if ((num == 0) && (unassignedText != null))
            {
                output.Append(unassignedText);
            }
        }

        public void AppendBoundKeyJustOne(ref StringBuilder output)
        {
            EnsureExists(ref output);
            if (this.m_keyboardKey != MyKeys.None)
            {
                AppendName(ref output, this.m_keyboardKey);
            }
            else
            {
                AppendName(ref output, this.m_KeyboardKey2);
            }
        }

        public static void AppendName(ref StringBuilder output, MyJoystickAxesEnum joystickAxis)
        {
            EnsureExists(ref output);
            output.Append(MyInput.Static.GetName(joystickAxis));
        }

        public static void AppendName(ref StringBuilder output, MyJoystickButtonsEnum joystickButton)
        {
            EnsureExists(ref output);
            output.Append(MyInput.Static.GetName(joystickButton));
        }

        public static void AppendName(ref StringBuilder output, MyKeys key)
        {
            EnsureExists(ref output);
            if (key != MyKeys.None)
            {
                output.Append(MyInput.Static.GetKeyName(key));
            }
        }

        public static void AppendName(ref StringBuilder output, MyMouseButtonsEnum mouseButton)
        {
            EnsureExists(ref output);
            output.Append(MyInput.Static.GetName(mouseButton));
        }

        public static void AppendName(ref StringBuilder output, MyKeys key1, MyKeys key2, string separator)
        {
            EnsureExists(ref output);
            string keyName = null;
            string keyName = null;
            if (key1 != MyKeys.None)
            {
                keyName = MyInput.Static.GetKeyName(key1);
            }
            if (key2 != MyKeys.None)
            {
                keyName = MyInput.Static.GetKeyName(key2);
            }
            if ((keyName != null) && (keyName != null))
            {
                output.Append(keyName).Append(separator).Append(keyName);
            }
            else if (keyName != null)
            {
                output.Append(keyName);
            }
            else if (keyName != null)
            {
                output.Append(keyName);
            }
        }

        public static void AppendUnknownTextIfNeeded(ref StringBuilder output, string unassignedText)
        {
            EnsureExists(ref output);
            if (output.Length == 0)
            {
                output.Append(unassignedText);
            }
        }

        public void CopyFrom(MyControl other)
        {
            this.m_data = other.m_data;
        }

        private static void EnsureExists(ref StringBuilder output)
        {
            if (output == null)
            {
                output = new StringBuilder(0x10);
            }
        }

        public float GetAnalogState()
        {
            bool flag = false;
            if (this.m_keyboardKey != MyKeys.None)
            {
                flag = MyInput.Static.IsKeyPress(this.m_keyboardKey);
            }
            if ((this.m_KeyboardKey2 != MyKeys.None) && !flag)
            {
                flag = MyInput.Static.IsKeyPress(this.m_KeyboardKey2);
            }
            if ((this.m_mouseButton != MyMouseButtonsEnum.None) && !flag)
            {
                switch (this.m_mouseButton)
                {
                    case MyMouseButtonsEnum.Left:
                        flag = MyInput.Static.IsLeftMousePressed();
                        break;

                    case MyMouseButtonsEnum.Middle:
                        flag = MyInput.Static.IsMiddleMousePressed();
                        break;

                    case MyMouseButtonsEnum.Right:
                        flag = MyInput.Static.IsRightMousePressed();
                        break;

                    case MyMouseButtonsEnum.XButton1:
                        flag = MyInput.Static.IsXButton1MousePressed();
                        break;

                    case MyMouseButtonsEnum.XButton2:
                        flag = MyInput.Static.IsXButton2MousePressed();
                        break;

                    default:
                        break;
                }
            }
            return (!flag ? 0f : 1f);
        }

        public string GetControlButtonName(MyGuiInputDeviceEnum deviceType)
        {
            m_toStringCache.Clear();
            this.AppendBoundButtonNames(ref m_toStringCache, deviceType, null);
            return m_toStringCache.ToString();
        }

        public MyStringId? GetControlDescription() => 
            this.m_data.Description;

        public MyStringId GetControlName() => 
            this.m_name;

        public MyGuiControlTypeEnum GetControlTypeEnum() => 
            this.m_controlType;

        public MyStringId GetGameControlEnum() => 
            this.m_controlId;

        public MyKeys GetKeyboardControl() => 
            this.m_keyboardKey;

        public MyMouseButtonsEnum GetMouseControl() => 
            this.m_mouseButton;

        public MyKeys GetSecondKeyboardControl() => 
            this.m_KeyboardKey2;

        public bool IsControlAssigned() => 
            ((this.m_keyboardKey != MyKeys.None) || (this.m_mouseButton != MyMouseButtonsEnum.None));

        public bool IsControlAssigned(MyGuiInputDeviceEnum deviceType)
        {
            bool flag = false;
            if (deviceType == MyGuiInputDeviceEnum.Keyboard)
            {
                flag = this.m_keyboardKey != MyKeys.None;
            }
            else if (deviceType == MyGuiInputDeviceEnum.Mouse)
            {
                flag = this.m_mouseButton != MyMouseButtonsEnum.None;
            }
            return flag;
        }

        public bool IsJoystickPressed() => 
            false;

        public bool IsNewJoystickPressed() => 
            false;

        public bool IsNewJoystickReleased() => 
            false;

        public bool IsNewPressed()
        {
            bool flag = false;
            if (this.m_keyboardKey != MyKeys.None)
            {
                flag = MyInput.Static.IsNewKeyPressed(this.m_keyboardKey);
            }
            if ((this.m_KeyboardKey2 != MyKeys.None) && !flag)
            {
                flag = MyInput.Static.IsNewKeyPressed(this.m_KeyboardKey2);
            }
            if ((this.m_mouseButton != MyMouseButtonsEnum.None) && !flag)
            {
                flag = MyInput.Static.IsNewMousePressed(this.m_mouseButton);
            }
            return flag;
        }

        public bool IsNewReleased()
        {
            bool flag = false;
            if (this.m_keyboardKey != MyKeys.None)
            {
                flag = MyInput.Static.IsNewKeyReleased(this.m_keyboardKey);
            }
            if ((this.m_KeyboardKey2 != MyKeys.None) && !flag)
            {
                flag = MyInput.Static.IsNewKeyReleased(this.m_KeyboardKey2);
            }
            if ((this.m_mouseButton != MyMouseButtonsEnum.None) && !flag)
            {
                switch (this.m_mouseButton)
                {
                    case MyMouseButtonsEnum.Left:
                        flag = MyInput.Static.IsNewLeftMouseReleased();
                        break;

                    case MyMouseButtonsEnum.Middle:
                        flag = MyInput.Static.IsNewMiddleMouseReleased();
                        break;

                    case MyMouseButtonsEnum.Right:
                        flag = MyInput.Static.IsNewRightMouseReleased();
                        break;

                    case MyMouseButtonsEnum.XButton1:
                        flag = MyInput.Static.IsNewXButton1MouseReleased();
                        break;

                    case MyMouseButtonsEnum.XButton2:
                        flag = MyInput.Static.IsNewXButton2MouseReleased();
                        break;

                    default:
                        break;
                }
            }
            return flag;
        }

        public bool IsPressed()
        {
            bool flag = false;
            if (this.m_keyboardKey != MyKeys.None)
            {
                flag = MyInput.Static.IsKeyPress(this.m_keyboardKey);
            }
            if ((this.m_KeyboardKey2 != MyKeys.None) && !flag)
            {
                flag = MyInput.Static.IsKeyPress(this.m_KeyboardKey2);
            }
            if ((this.m_mouseButton != MyMouseButtonsEnum.None) && !flag)
            {
                flag = MyInput.Static.IsMousePressed(this.m_mouseButton);
            }
            return flag;
        }

        public void SetControl(MyMouseButtonsEnum mouseButton)
        {
            this.m_mouseButton = mouseButton;
        }

        public void SetControl(MyGuiInputDeviceEnum device, MyKeys key)
        {
            if (device == MyGuiInputDeviceEnum.Keyboard)
            {
                this.m_keyboardKey = key;
            }
            else if (device == MyGuiInputDeviceEnum.KeyboardSecond)
            {
                this.m_KeyboardKey2 = key;
            }
            else
            {
                MyLog.Default.WriteLine("ERROR: Setting non-keyboard device to keyboard control.");
            }
        }

        public void SetNoControl()
        {
            this.m_mouseButton = MyMouseButtonsEnum.None;
            this.m_keyboardKey = MyKeys.None;
            this.m_KeyboardKey2 = MyKeys.None;
        }

        public override string ToString() => 
            this.ButtonNames.UpdateControlsToNotificationFriendly();

        public StringBuilder ToStringBuilder(string unassignedText)
        {
            m_toStringCache.Clear();
            this.AppendBoundButtonNames(ref m_toStringCache, ", ", unassignedText, true);
            return new StringBuilder(m_toStringCache.Length).AppendStringBuilder(m_toStringCache);
        }

        private MyStringId m_name
        {
            get => 
                this.m_data.Name;
            set => 
                (this.m_data.Name = value);
        }

        private MyStringId m_controlId
        {
            get => 
                this.m_data.ControlId;
            set => 
                (this.m_data.ControlId = value);
        }

        private MyGuiControlTypeEnum m_controlType
        {
            get => 
                this.m_data.ControlType;
            set => 
                (this.m_data.ControlType = value);
        }

        private MyKeys m_keyboardKey
        {
            get => 
                this.m_data.KeyboardKey;
            set => 
                (this.m_data.KeyboardKey = value);
        }

        private MyKeys m_KeyboardKey2
        {
            get => 
                this.m_data.KeyboardKey2;
            set => 
                (this.m_data.KeyboardKey2 = value);
        }

        private MyMouseButtonsEnum m_mouseButton
        {
            get => 
                this.m_data.MouseButton;
            set => 
                (this.m_data.MouseButton = value);
        }

        public string ButtonNames
        {
            get
            {
                m_toStringCache.Clear();
                this.AppendBoundButtonNames(ref m_toStringCache, ", ", MyInput.Static.GetUnassignedName(), true);
                return m_toStringCache.ToString();
            }
        }

        public string ButtonNamesIgnoreSecondary
        {
            get
            {
                m_toStringCache.Clear();
                this.AppendBoundButtonNames(ref m_toStringCache, ", ", null, false);
                return m_toStringCache.ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Data
        {
            public MyStringId Name;
            public MyStringId ControlId;
            public MyGuiControlTypeEnum ControlType;
            public MyKeys KeyboardKey;
            public MyKeys KeyboardKey2;
            public MyMouseButtonsEnum MouseButton;
            public MyStringId? Description;
        }
    }
}

