namespace VRage.Input
{
    using SharpDX;
    using SharpDX.DirectInput;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using VRage;
    using VRage.Collections;
    using VRage.ModAPI;
    using VRage.OpenVRWrapper;
    using VRage.Serialization;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender.ExternalApp;

    public class MyDirectXInput : VRage.ModAPI.IMyInput, VRage.Input.IMyInput, ITextEvaluator
    {
        internal bool OverrideUpdate;
        private Vector2 m_absoluteMousePosition;
        private MyMouseState m_previousMouseState;
        private JoystickState m_previousJoystickState;
        private MyGuiLocalizedKeyboardState m_keyboardState;
        private MyMouseState m_actualMouseState;
        private MyMouseState m_actualMouseStateRaw;
        private JoystickState m_actualJoystickState;
        private bool m_joystickXAxisSupported;
        private bool m_joystickYAxisSupported;
        private bool m_joystickZAxisSupported;
        private bool m_joystickRotationXAxisSupported;
        private bool m_joystickRotationYAxisSupported;
        private bool m_joystickRotationZAxisSupported;
        private bool m_joystickSlider1AxisSupported;
        private bool m_joystickSlider2AxisSupported;
        private bool m_mouseXIsInverted;
        private bool m_mouseYIsInverted;
        private float m_mouseSensitivity;
        private string m_joystickInstanceName;
        private float m_joystickSensitivity;
        private float m_joystickDeadzone;
        private float m_joystickExponent;
        private string m_joystickInstanceNameSnapshot;
        private MyMouseState mouse;
        private bool m_enabled = true;
        private readonly MyKeyHasher m_hasher = new MyKeyHasher();
        private readonly Dictionary<MyStringId, MyControl> m_defaultGameControlsList;
        private readonly Dictionary<MyStringId, MyControl> m_gameControlsList = new Dictionary<MyStringId, MyControl>(MyStringId.Comparer);
        private readonly Dictionary<MyStringId, MyControl> m_gameControlsSnapshot = new Dictionary<MyStringId, MyControl>(MyStringId.Comparer);
        private readonly HashSet<MyStringId> m_gameControlsBlacklist = new HashSet<MyStringId>();
        private readonly List<MyKeys> m_validKeyboardKeys = new List<MyKeys>();
        private readonly List<MyJoystickButtonsEnum> m_validJoystickButtons = new List<MyJoystickButtonsEnum>();
        private readonly List<MyJoystickAxesEnum> m_validJoystickAxes = new List<MyJoystickAxesEnum>();
        private readonly List<MyMouseButtonsEnum> m_validMouseButtons = new List<MyMouseButtonsEnum>();
        private readonly List<MyKeys> m_digitKeys = new List<MyKeys>();
        private Device m_joystick;
        private DeviceType? m_joystickType;
        private bool m_joystickConnected;
        private readonly IMyBufferedInputSource m_bufferedInputSource;
        private readonly IMyControlNameLookup m_nameLookup;
        private List<char> m_currentTextInput = new List<char>();
        private IntPtr m_windowHandle;
        private bool m_gameWasFocused;
        [CompilerGenerated]
        private Action<bool> JoystickConnected;

        public event Action<bool> JoystickConnected
        {
            [CompilerGenerated] add
            {
                Action<bool> joystickConnected = this.JoystickConnected;
                while (true)
                {
                    Action<bool> a = joystickConnected;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    joystickConnected = Interlocked.CompareExchange<Action<bool>>(ref this.JoystickConnected, action3, a);
                    if (ReferenceEquals(joystickConnected, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> joystickConnected = this.JoystickConnected;
                while (true)
                {
                    Action<bool> source = joystickConnected;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    joystickConnected = Interlocked.CompareExchange<Action<bool>>(ref this.JoystickConnected, action3, source);
                    if (ReferenceEquals(joystickConnected, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<bool> VRage.ModAPI.IMyInput.JoystickConnected
        {
            add
            {
                this.JoystickConnected += value;
            }
            remove
            {
                this.JoystickConnected -= value;
            }
        }

        public MyDirectXInput(IMyBufferedInputSource textInputBuffer, IMyControlNameLookup nameLookup, Dictionary<MyStringId, MyControl> gameControls, bool enableDevKeys)
        {
            this.m_bufferedInputSource = textInputBuffer;
            this.m_nameLookup = nameLookup;
            this.m_defaultGameControlsList = gameControls;
            this.m_gameControlsList = new Dictionary<MyStringId, MyControl>(MyStringId.Comparer);
            this.m_gameControlsSnapshot = new Dictionary<MyStringId, MyControl>(MyStringId.Comparer);
            this.CloneControls(this.m_defaultGameControlsList, this.m_gameControlsList);
            this.ENABLE_DEVELOPER_KEYS = enableDevKeys;
        }

        public void AddDefaultControl(MyStringId stringId, MyControl control)
        {
            this.m_gameControlsList[stringId] = control;
            this.m_defaultGameControlsList[stringId] = control;
        }

        private void CheckValidControls(Dictionary<MyStringId, MyControl> controls)
        {
            foreach (MyControl local1 in controls.Values)
            {
            }
        }

        public void ClearBlacklist()
        {
            this.m_gameControlsBlacklist.Clear();
        }

        internal void ClearStates()
        {
            this.m_keyboardState.ClearStates();
            this.m_previousMouseState = this.m_actualMouseState;
            this.m_actualMouseState = new MyMouseState();
            this.m_actualMouseStateRaw.ClearPosition();
            MyOpenVR.ClearButtonStates();
        }

        private void CloneControls(Dictionary<MyStringId, MyControl> original, Dictionary<MyStringId, MyControl> copy)
        {
            foreach (KeyValuePair<MyStringId, MyControl> pair in original)
            {
                MyControl control;
                if (copy.TryGetValue(pair.Key, out control))
                {
                    control.CopyFrom(pair.Value);
                    continue;
                }
                copy[pair.Key] = new MyControl(pair.Value);
            }
        }

        public int DeltaMouseScrollWheelValue() => 
            (this.MouseScrollWheelValue() - this.PreviousMouseScrollWheelValue());

        private void DeviceChangeCallback(ref Message m)
        {
            if ((this.m_joystick == null) || !MyDirectInput.DirectInput.IsDeviceAttached(this.m_joystick.Information.InstanceGuid))
            {
                this.InitializeJoystickIfPossible();
            }
        }

        public void EnableInput(bool enable)
        {
            this.Enabled = enable;
        }

        public List<string> EnumerateJoystickNames()
        {
            List<string> list = new List<string>();
            IList<DeviceInstance> devices = MyDirectInput.DirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            for (int i = 0; i < devices.Count; i++)
            {
                DeviceInstance instance = devices[i];
                list.Add(instance.InstanceName.Replace("\0", string.Empty));
            }
            return list;
        }

        private MyControl FindNotAssignedGameControl(MyStringId controlId, MyGuiInputDeviceEnum deviceType)
        {
            MyControl control;
            if (!this.m_gameControlsList.TryGetValue(controlId, out control))
            {
                throw new Exception("Game control \"" + controlId.ToString() + "\" not found in control list.");
            }
            if (control.IsControlAssigned(deviceType))
            {
                throw new Exception("Game control \"" + controlId.ToString() + "\" is already assigned.");
            }
            return control;
        }

        public void GetActualJoystickState(StringBuilder text)
        {
            if (this.m_actualJoystickState == null)
            {
                text.Append("No joystick detected.");
            }
            else
            {
                JoystickState actualJoystickState = this.m_actualJoystickState;
                text.Append("Supported axes: ");
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.Xpos))
                {
                    text.Append("X ");
                }
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.Ypos))
                {
                    text.Append("Y ");
                }
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.Zpos))
                {
                    text.Append("Z ");
                }
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.RotationXpos))
                {
                    text.Append("Rx ");
                }
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.RotationYpos))
                {
                    text.Append("Ry ");
                }
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.RotationZpos))
                {
                    text.Append("Rz ");
                }
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.Slider1pos))
                {
                    text.Append("S1 ");
                }
                if (this.IsJoystickAxisSupported(MyJoystickAxesEnum.Slider2pos))
                {
                    text.Append("S2 ");
                }
                text.AppendLine();
                text.Append("accX: ");
                text.AppendInt32(actualJoystickState.AccelerationX);
                text.AppendLine();
                text.Append("accY: ");
                text.AppendInt32(actualJoystickState.AccelerationY);
                text.AppendLine();
                text.Append("accZ: ");
                text.AppendInt32(actualJoystickState.AccelerationZ);
                text.AppendLine();
                text.Append("angAccX: ");
                text.AppendInt32(actualJoystickState.AngularAccelerationX);
                text.AppendLine();
                text.Append("angAccY: ");
                text.AppendInt32(actualJoystickState.AngularAccelerationY);
                text.AppendLine();
                text.Append("angAccZ: ");
                text.AppendInt32(actualJoystickState.AngularAccelerationZ);
                text.AppendLine();
                text.Append("angVelX: ");
                text.AppendInt32(actualJoystickState.AngularVelocityX);
                text.AppendLine();
                text.Append("angVelY: ");
                text.AppendInt32(actualJoystickState.AngularVelocityY);
                text.AppendLine();
                text.Append("angVelZ: ");
                text.AppendInt32(actualJoystickState.AngularVelocityZ);
                text.AppendLine();
                text.Append("forX: ");
                text.AppendInt32(actualJoystickState.ForceX);
                text.AppendLine();
                text.Append("forY: ");
                text.AppendInt32(actualJoystickState.ForceY);
                text.AppendLine();
                text.Append("forZ: ");
                text.AppendInt32(actualJoystickState.ForceZ);
                text.AppendLine();
                text.Append("rotX: ");
                text.AppendInt32(actualJoystickState.RotationX);
                text.AppendLine();
                text.Append("rotY: ");
                text.AppendInt32(actualJoystickState.RotationY);
                text.AppendLine();
                text.Append("rotZ: ");
                text.AppendInt32(actualJoystickState.RotationZ);
                text.AppendLine();
                text.Append("torqX: ");
                text.AppendInt32(actualJoystickState.TorqueX);
                text.AppendLine();
                text.Append("torqY: ");
                text.AppendInt32(actualJoystickState.TorqueY);
                text.AppendLine();
                text.Append("torqZ: ");
                text.AppendInt32(actualJoystickState.TorqueZ);
                text.AppendLine();
                text.Append("velX: ");
                text.AppendInt32(actualJoystickState.VelocityX);
                text.AppendLine();
                text.Append("velY: ");
                text.AppendInt32(actualJoystickState.VelocityY);
                text.AppendLine();
                text.Append("velZ: ");
                text.AppendInt32(actualJoystickState.VelocityZ);
                text.AppendLine();
                text.Append("X: ");
                text.AppendInt32(actualJoystickState.X);
                text.AppendLine();
                text.Append("Y: ");
                text.AppendInt32(actualJoystickState.Y);
                text.AppendLine();
                text.Append("Z: ");
                text.AppendInt32(actualJoystickState.Z);
                text.AppendLine();
                text.AppendLine();
                text.Append("AccSliders: ");
                foreach (int num2 in actualJoystickState.AccelerationSliders)
                {
                    text.AppendInt32(num2);
                    text.Append(" ");
                }
                text.AppendLine();
                text.Append("Buttons: ");
                foreach (bool flag in actualJoystickState.Buttons)
                {
                    text.Append(flag ? "#" : "_");
                    text.Append(" ");
                }
                text.AppendLine();
                text.Append("ForSliders: ");
                foreach (int num3 in actualJoystickState.ForceSliders)
                {
                    text.AppendInt32(num3);
                    text.Append(" ");
                }
                text.AppendLine();
                text.Append("POVControllers: ");
                foreach (int num4 in actualJoystickState.PointOfViewControllers)
                {
                    text.AppendInt32(num4);
                    text.Append(" ");
                }
                text.AppendLine();
                text.Append("Sliders: ");
                foreach (int num5 in actualJoystickState.Sliders)
                {
                    text.AppendInt32(num5);
                    text.Append(" ");
                }
                text.AppendLine();
                text.Append("VelocitySliders: ");
                foreach (int num6 in actualJoystickState.VelocitySliders)
                {
                    text.AppendInt32(num6);
                    text.Append(" ");
                }
                text.AppendLine();
            }
        }

        public MyControl GetControl(MyKeys key)
        {
            using (Dictionary<MyStringId, MyControl>.ValueCollection.Enumerator enumerator = this.m_gameControlsList.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyControl current = enumerator.Current;
                    if ((current.GetKeyboardControl() == key) || (current.GetSecondKeyboardControl() == key))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public MyControl GetControl(MyMouseButtonsEnum button)
        {
            using (Dictionary<MyStringId, MyControl>.ValueCollection.Enumerator enumerator = this.m_gameControlsList.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyControl current = enumerator.Current;
                    if (current.GetMouseControl() == button)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        public MyControl GetGameControl(MyStringId controlId)
        {
            MyControl control;
            this.m_gameControlsList.TryGetValue(controlId, out control);
            return control;
        }

        public float GetGameControlAnalogState(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (!this.m_gameControlsList.TryGetValue(controlId, out control) ? 0f : control.GetAnalogState()) : 0f);
        }

        public DictionaryValuesReader<MyStringId, MyControl> GetGameControlsList() => 
            this.m_gameControlsList;

        public string GetGameControlTextEnum(MyStringId controlId) => 
            this.m_gameControlsList[controlId].GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);

        public bool GetGamepadKeyDirections(out int actual, out int previous)
        {
            if ((this.m_joystickConnected && (this.m_actualJoystickState != null)) && (this.m_previousJoystickState != null))
            {
                int[] pointOfViewControllers = this.m_actualJoystickState.PointOfViewControllers;
                int[] numArray2 = this.m_previousJoystickState.PointOfViewControllers;
                if ((pointOfViewControllers != null) && (numArray2 != null))
                {
                    actual = pointOfViewControllers[0];
                    previous = numArray2[0];
                    return true;
                }
            }
            actual = -1;
            previous = -1;
            return false;
        }

        public static object GetHighlightedControl(MyStringId controlId)
        {
            string controlButtonName = MyInput.Static.GetGameControl(controlId)?.GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
            string str2 = MyInput.Static.GetGameControl(controlId)?.GetControlButtonName(MyGuiInputDeviceEnum.Mouse);
            if (string.IsNullOrEmpty(controlButtonName))
            {
                return ("[" + str2 + "]");
            }
            if (string.IsNullOrEmpty(str2))
            {
                return ("[" + controlButtonName + "]");
            }
            string[] textArray1 = new string[] { "[", controlButtonName, "'/'", str2, "]" };
            return string.Concat(textArray1);
        }

        public float GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis)
        {
            if (this.m_joystickConnected && this.IsJoystickAxisSupported(axis))
            {
                float num = (this.GetJoystickAxisStateRaw(axis) - 32767f) / 32767f;
                switch (axis)
                {
                    case MyJoystickAxesEnum.Xpos:
                    case MyJoystickAxesEnum.Ypos:
                    case MyJoystickAxesEnum.Zpos:
                    case MyJoystickAxesEnum.RotationXpos:
                    case MyJoystickAxesEnum.RotationYpos:
                    case MyJoystickAxesEnum.RotationZpos:
                    case MyJoystickAxesEnum.Slider1pos:
                    case MyJoystickAxesEnum.Slider2pos:
                        if (num > 0f)
                        {
                            break;
                        }
                        return 0f;

                    case MyJoystickAxesEnum.Xneg:
                    case MyJoystickAxesEnum.Yneg:
                    case MyJoystickAxesEnum.Zneg:
                    case MyJoystickAxesEnum.RotationXneg:
                    case MyJoystickAxesEnum.RotationYneg:
                    case MyJoystickAxesEnum.RotationZneg:
                    case MyJoystickAxesEnum.Slider1neg:
                    case MyJoystickAxesEnum.Slider2neg:
                        if (num < 0f)
                        {
                            break;
                        }
                        return 0f;

                    default:
                        break;
                }
                float num2 = Math.Abs(num);
                if (num2 > this.m_joystickDeadzone)
                {
                    num2 = (num2 - this.m_joystickDeadzone) / (1f - this.m_joystickDeadzone);
                    return (this.m_joystickSensitivity * ((float) Math.Pow((double) num2, (double) this.m_joystickExponent)));
                }
            }
            return 0f;
        }

        public float GetJoystickAxisStateRaw(MyJoystickAxesEnum axis)
        {
            int x = 0x8000;
            if ((this.m_joystickConnected && ((axis != MyJoystickAxesEnum.None) && (this.m_actualJoystickState != null))) && this.IsJoystickAxisSupported(axis))
            {
                switch (axis)
                {
                    case MyJoystickAxesEnum.Xpos:
                    case MyJoystickAxesEnum.Xneg:
                        x = this.m_actualJoystickState.X;
                        break;

                    case MyJoystickAxesEnum.Ypos:
                    case MyJoystickAxesEnum.Yneg:
                        x = this.m_actualJoystickState.Y;
                        break;

                    case MyJoystickAxesEnum.Zpos:
                    case MyJoystickAxesEnum.Zneg:
                        x = this.m_actualJoystickState.Z;
                        break;

                    case MyJoystickAxesEnum.RotationXpos:
                    case MyJoystickAxesEnum.RotationXneg:
                        x = this.m_actualJoystickState.RotationX;
                        break;

                    case MyJoystickAxesEnum.RotationYpos:
                    case MyJoystickAxesEnum.RotationYneg:
                        x = this.m_actualJoystickState.RotationY;
                        break;

                    case MyJoystickAxesEnum.RotationZpos:
                    case MyJoystickAxesEnum.RotationZneg:
                        x = this.m_actualJoystickState.RotationZ;
                        break;

                    case MyJoystickAxesEnum.Slider1pos:
                    case MyJoystickAxesEnum.Slider1neg:
                    {
                        int[] sliders = this.m_actualJoystickState.Sliders;
                        x = (sliders.Length < 1) ? 0x8000 : sliders[0];
                        break;
                    }
                    case MyJoystickAxesEnum.Slider2pos:
                    case MyJoystickAxesEnum.Slider2neg:
                    {
                        int[] sliders = this.m_actualJoystickState.Sliders;
                        x = (sliders.Length < 2) ? 0x8000 : sliders[1];
                        break;
                    }
                    default:
                        break;
                }
            }
            return (float) x;
        }

        public float GetJoystickDeadzone() => 
            this.m_joystickDeadzone;

        public float GetJoystickExponent() => 
            this.m_joystickExponent;

        public float GetJoystickSensitivity() => 
            this.m_joystickSensitivity;

        public float GetJoystickX() => 
            this.GetJoystickAxisStateRaw(MyJoystickAxesEnum.Xpos);

        public float GetJoystickY() => 
            this.GetJoystickAxisStateRaw(MyJoystickAxesEnum.Ypos);

        public string GetKeyName(MyKeys key) => 
            this.m_nameLookup.GetKeyName(key);

        public string GetKeyName(MyStringId controlId) => 
            this.GetGameControl(controlId).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern short GetKeyState(int keyCode);
        public void GetListOfPressedKeys(List<MyKeys> keys)
        {
            this.GetPressedKeys(keys);
        }

        public void GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result)
        {
            result.Clear();
            if (this.IsLeftMousePressed())
            {
                result.Add(MyMouseButtonsEnum.Left);
            }
            if (this.IsRightMousePressed())
            {
                result.Add(MyMouseButtonsEnum.Right);
            }
            if (this.IsMiddleMousePressed())
            {
                result.Add(MyMouseButtonsEnum.Middle);
            }
            if (this.IsXButton1MousePressed())
            {
                result.Add(MyMouseButtonsEnum.XButton1);
            }
            if (this.IsXButton2MousePressed())
            {
                result.Add(MyMouseButtonsEnum.XButton2);
            }
        }

        public Vector2 GetMouseAreaSize() => 
            this.m_bufferedInputSource.MouseAreaSize;

        public Vector2 GetMousePosition() => 
            this.m_absoluteMousePosition;

        public float GetMouseSensitivity() => 
            this.m_mouseSensitivity;

        public int GetMouseX() => 
            this.m_actualMouseState.X;

        public int GetMouseXForGamePlay()
        {
            int num = this.m_mouseXIsInverted ? -1 : 1;
            return (int) (this.m_mouseSensitivity * (num * this.m_actualMouseState.X));
        }

        public float GetMouseXForGamePlayF()
        {
            float num = this.m_mouseXIsInverted ? -1f : 1f;
            return (this.m_mouseSensitivity * (num * this.m_actualMouseState.X));
        }

        public bool GetMouseXInversion() => 
            this.m_mouseXIsInverted;

        public int GetMouseY() => 
            this.m_actualMouseState.Y;

        public int GetMouseYForGamePlay()
        {
            int num = this.m_mouseYIsInverted ? -1 : 1;
            return (int) (this.m_mouseSensitivity * (num * this.m_actualMouseState.Y));
        }

        public float GetMouseYForGamePlayF()
        {
            float num = this.m_mouseYIsInverted ? -1f : 1f;
            return (this.m_mouseSensitivity * (num * this.m_actualMouseState.Y));
        }

        public bool GetMouseYInversion() => 
            this.m_mouseYIsInverted;

        public string GetName(MyJoystickAxesEnum joystickAxis) => 
            this.m_nameLookup.GetName(joystickAxis);

        public string GetName(MyJoystickButtonsEnum joystickButton) => 
            this.m_nameLookup.GetName(joystickButton);

        public string GetName(MyMouseButtonsEnum mouseButton) => 
            this.m_nameLookup.GetName(mouseButton);

        public void GetPressedKeys(List<MyKeys> keys)
        {
            this.m_keyboardState.GetActualPressedKeys(keys);
        }

        public float GetPreviousJoystickAxisStateForGameplay(MyJoystickAxesEnum axis)
        {
            if (this.m_joystickConnected && this.IsJoystickAxisSupported(axis))
            {
                float num = (this.GetPreviousJoystickAxisStateRaw(axis) - 32767f) / 32767f;
                switch (axis)
                {
                    case MyJoystickAxesEnum.Xpos:
                    case MyJoystickAxesEnum.Ypos:
                    case MyJoystickAxesEnum.Zpos:
                    case MyJoystickAxesEnum.RotationXpos:
                    case MyJoystickAxesEnum.RotationYpos:
                    case MyJoystickAxesEnum.RotationZpos:
                    case MyJoystickAxesEnum.Slider1pos:
                    case MyJoystickAxesEnum.Slider2pos:
                        if (num > 0f)
                        {
                            break;
                        }
                        return 0f;

                    case MyJoystickAxesEnum.Xneg:
                    case MyJoystickAxesEnum.Yneg:
                    case MyJoystickAxesEnum.Zneg:
                    case MyJoystickAxesEnum.RotationXneg:
                    case MyJoystickAxesEnum.RotationYneg:
                    case MyJoystickAxesEnum.RotationZneg:
                    case MyJoystickAxesEnum.Slider1neg:
                    case MyJoystickAxesEnum.Slider2neg:
                        if (num < 0f)
                        {
                            break;
                        }
                        return 0f;

                    default:
                        break;
                }
                float num2 = Math.Abs(num);
                if (num2 > this.m_joystickDeadzone)
                {
                    num2 = (num2 - this.m_joystickDeadzone) / (1f - this.m_joystickDeadzone);
                    return (this.m_joystickSensitivity * ((float) Math.Pow((double) num2, (double) this.m_joystickExponent)));
                }
            }
            return 0f;
        }

        public float GetPreviousJoystickAxisStateRaw(MyJoystickAxesEnum axis)
        {
            int x = 0x8000;
            if ((this.m_joystickConnected && ((axis != MyJoystickAxesEnum.None) && (this.m_previousJoystickState != null))) && this.IsJoystickAxisSupported(axis))
            {
                switch (axis)
                {
                    case MyJoystickAxesEnum.Xpos:
                    case MyJoystickAxesEnum.Xneg:
                        x = this.m_previousJoystickState.X;
                        break;

                    case MyJoystickAxesEnum.Ypos:
                    case MyJoystickAxesEnum.Yneg:
                        x = this.m_previousJoystickState.Y;
                        break;

                    case MyJoystickAxesEnum.Zpos:
                    case MyJoystickAxesEnum.Zneg:
                        x = this.m_previousJoystickState.Z;
                        break;

                    case MyJoystickAxesEnum.RotationXpos:
                    case MyJoystickAxesEnum.RotationXneg:
                        x = this.m_previousJoystickState.RotationX;
                        break;

                    case MyJoystickAxesEnum.RotationYpos:
                    case MyJoystickAxesEnum.RotationYneg:
                        x = this.m_previousJoystickState.RotationY;
                        break;

                    case MyJoystickAxesEnum.RotationZpos:
                    case MyJoystickAxesEnum.RotationZneg:
                        x = this.m_previousJoystickState.RotationZ;
                        break;

                    case MyJoystickAxesEnum.Slider1pos:
                    case MyJoystickAxesEnum.Slider1neg:
                    {
                        int[] sliders = this.m_previousJoystickState.Sliders;
                        x = (sliders.Length < 1) ? 0x8000 : sliders[0];
                        break;
                    }
                    case MyJoystickAxesEnum.Slider2pos:
                    case MyJoystickAxesEnum.Slider2neg:
                    {
                        int[] sliders = this.m_previousJoystickState.Sliders;
                        x = (sliders.Length < 2) ? 0x8000 : sliders[1];
                        break;
                    }
                    default:
                        break;
                }
            }
            return (float) x;
        }

        public string GetUnassignedName() => 
            this.m_nameLookup.UnassignedText;

        private void InitDevicePluginHandlerCallBack()
        {
            MyMessageLoop.AddMessageHandler(WinApi.WM.DEVICECHANGE, new ActionRef<Message>(this.DeviceChangeCallback));
        }

        private void InitializeJoystickIfPossible()
        {
            if (this.m_joystick != null)
            {
                this.m_joystick.Dispose();
                this.m_joystick = null;
                this.SetJoystickConnected(false);
                this.m_joystickType = null;
            }
            if ((this.m_joystick == null) && (this.m_joystickInstanceName != null))
            {
                foreach (DeviceInstance instance in MyDirectInput.DirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
                {
                    if (instance.InstanceName.Contains(this.m_joystickInstanceName))
                    {
                        try
                        {
                            this.m_joystick = new Joystick(MyDirectInput.DirectInput, instance.InstanceGuid);
                            this.m_joystickType = new DeviceType?(instance.Type);
                            this.m_joystick.SetCooperativeLevel(this.m_windowHandle, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
                        }
                        catch (SharpDXException)
                        {
                            continue;
                        }
                        break;
                    }
                }
                if (this.m_joystick != null)
                {
                    bool flag;
                    int num = 0;
                    this.m_joystickZAxisSupported = flag = false;
                    this.m_joystickYAxisSupported = flag = flag;
                    this.m_joystickXAxisSupported = flag;
                    this.m_joystickRotationZAxisSupported = flag = false;
                    this.m_joystickRotationXAxisSupported = this.m_joystickRotationYAxisSupported = flag;
                    this.m_joystickSlider1AxisSupported = this.m_joystickSlider2AxisSupported = false;
                    foreach (DeviceObjectInstance instance2 in this.m_joystick.GetObjects())
                    {
                        if ((instance2.ObjectId.Flags & DeviceObjectTypeFlags.Axis) == DeviceObjectTypeFlags.All)
                        {
                            continue;
                        }
                        this.m_joystick.GetObjectPropertiesById(instance2.ObjectId).Range = new InputRange(0, 0xffff);
                        if (instance2.ObjectType == ObjectGuid.XAxis)
                        {
                            this.m_joystickXAxisSupported = true;
                            continue;
                        }
                        if (instance2.ObjectType == ObjectGuid.YAxis)
                        {
                            this.m_joystickYAxisSupported = true;
                            continue;
                        }
                        if (instance2.ObjectType == ObjectGuid.ZAxis)
                        {
                            this.m_joystickZAxisSupported = true;
                            continue;
                        }
                        if (instance2.ObjectType == ObjectGuid.RxAxis)
                        {
                            this.m_joystickRotationXAxisSupported = true;
                            continue;
                        }
                        if (instance2.ObjectType == ObjectGuid.RyAxis)
                        {
                            this.m_joystickRotationYAxisSupported = true;
                            continue;
                        }
                        if (instance2.ObjectType == ObjectGuid.RzAxis)
                        {
                            this.m_joystickRotationZAxisSupported = true;
                            continue;
                        }
                        if (instance2.ObjectType == ObjectGuid.Slider)
                        {
                            num++;
                            if (num >= 1)
                            {
                                this.m_joystickSlider1AxisSupported = true;
                            }
                            if (num >= 2)
                            {
                                this.m_joystickSlider2AxisSupported = true;
                            }
                        }
                    }
                    try
                    {
                        this.m_joystick.Acquire();
                        this.SetJoystickConnected(true);
                    }
                    catch (SharpDXException)
                    {
                    }
                }
            }
        }

        public bool IsAnyAltKeyPressed() => 
            (this.IsKeyPress(MyKeys.Alt) || (this.IsKeyPress(MyKeys.LeftAlt) || this.IsKeyPress(MyKeys.RightAlt)));

        public bool IsAnyCtrlKeyPressed() => 
            (this.IsKeyPress(MyKeys.LeftControl) || this.IsKeyPress(MyKeys.RightControl));

        private bool IsAnyJoystickAxisPressed()
        {
            if (this.m_joystickConnected)
            {
                using (List<MyJoystickAxesEnum>.Enumerator enumerator = this.m_validJoystickAxes.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyJoystickAxesEnum current = enumerator.Current;
                        if ((current != MyJoystickAxesEnum.None) && this.IsJoystickAxisPressed(current))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsAnyJoystickButtonPressed()
        {
            if (this.m_joystickConnected)
            {
                int num1;
                if ((this.IsGamepadKeyDownPressed() || this.IsGamepadKeyLeftPressed()) || this.IsGamepadKeyRightPressed())
                {
                    num1 = 1;
                }
                else
                {
                    num1 = (int) this.IsGamepadKeyUpPressed();
                }
                if (num1 != 0)
                {
                    return true;
                }
                for (int i = 0; i < 0x10; i++)
                {
                    if (this.m_actualJoystickState.Buttons[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsAnyKeyPress() => 
            this.m_keyboardState.IsAnyKeyPressed();

        public bool IsAnyMouseOrJoystickPressed() => 
            (this.IsAnyMousePressed() || this.IsAnyJoystickButtonPressed());

        public bool IsAnyMousePressed() => 
            (this.m_actualMouseState.LeftButton || (this.m_actualMouseState.MiddleButton || (this.m_actualMouseState.RightButton || (this.m_actualMouseState.XButton1 || this.m_actualMouseState.XButton2))));

        public bool IsAnyNewJoystickButtonPressed()
        {
            if ((this.m_joystickConnected && (this.m_actualJoystickState != null)) && (this.m_previousJoystickState != null))
            {
                for (int i = 0; i < 0x10; i++)
                {
                    if (this.m_actualJoystickState.Buttons[i] && !this.m_previousJoystickState.Buttons[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsAnyNewKeyPress() => 
            (this.m_keyboardState.IsAnyKeyPressed() && !this.m_keyboardState.GetPreviousKeyboardState().IsAnyKeyPressed());

        public bool IsAnyNewMouseOrJoystickPressed() => 
            (this.IsAnyNewMousePressed() || this.IsAnyNewJoystickButtonPressed());

        public bool IsAnyNewMousePressed() => 
            (this.IsNewLeftMousePressed() || (this.IsNewMiddleMousePressed() || (this.IsNewRightMousePressed() || (this.IsNewXButton1MousePressed() || this.IsNewXButton2MousePressed()))));

        public bool IsAnyShiftKeyPressed() => 
            (this.IsKeyPress(MyKeys.LeftShift) || this.IsKeyPress(MyKeys.RightShift));

        public bool IsAnyWinKeyPressed() => 
            (this.IsKeyPress(MyKeys.LeftWindows) || this.IsKeyPress(MyKeys.RightWindows));

        public bool IsButtonPressed(MySharedButtonsEnum button) => 
            ((button == MySharedButtonsEnum.Primary) ? this.IsPrimaryButtonPressed() : ((button == MySharedButtonsEnum.Secondary) ? this.IsSecondaryButtonPressed() : false));

        public bool IsButtonReleased(MySharedButtonsEnum button) => 
            ((button == MySharedButtonsEnum.Primary) ? this.IsPrimaryButtonReleased() : ((button == MySharedButtonsEnum.Secondary) ? this.IsSecondaryButtonReleased() : false));

        public bool IsControlBlocked(MyStringId controlEnum) => 
            this.m_gameControlsBlacklist.Contains(controlEnum);

        public bool IsEnabled() => 
            this.m_enabled;

        public bool IsGameControlJoystickOnlyPressed(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (this.m_gameControlsList.TryGetValue(controlId, out control) && control.IsJoystickPressed()) : false);
        }

        public bool IsGameControlPressed(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (this.m_gameControlsList.TryGetValue(controlId, out control) && control.IsPressed()) : false);
        }

        public bool IsGameControlReleased(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (this.m_gameControlsList.TryGetValue(controlId, out control) && control.IsNewReleased()) : false);
        }

        public bool IsGamepadKeyDownPressed()
        {
            int num;
            int num2;
            return (this.GetGamepadKeyDirections(out num, out num2) && ((num >= 0x34bc) && (num <= 0x57e4)));
        }

        public bool IsGamepadKeyLeftPressed()
        {
            int num;
            int num2;
            return (this.GetGamepadKeyDirections(out num, out num2) && ((num >= 0x57e4) && (num <= 0x7b0c)));
        }

        public bool IsGamepadKeyRightPressed()
        {
            int num;
            int num2;
            return (this.GetGamepadKeyDirections(out num, out num2) && ((num >= 0x1194) && (num <= 0x34bc)));
        }

        public bool IsGamepadKeyUpPressed()
        {
            int num;
            int num2;
            if (!this.GetGamepadKeyDirections(out num, out num2))
            {
                return false;
            }
            if ((num < 0) || (num > 0x1194))
            {
                return ((num >= 0x7b0c) && (num <= 0x8ca0));
            }
            return true;
        }

        public bool IsJoystickAxisNewPressed(MyJoystickAxesEnum axis)
        {
            bool flag = false;
            if ((this.m_joystickConnected && ((axis != MyJoystickAxesEnum.None) && (this.m_actualJoystickState != null))) && (this.m_previousJoystickState != null))
            {
                float previousJoystickAxisStateForGameplay = this.GetPreviousJoystickAxisStateForGameplay(axis);
                flag = (this.GetJoystickAxisStateForGameplay(axis) > 0.5f) && (previousJoystickAxisStateForGameplay <= 0.5f);
            }
            if (flag || (axis != MyJoystickAxesEnum.None))
            {
                return (this.IsJoystickAxisSupported(axis) ? flag : false);
            }
            return true;
        }

        public bool IsJoystickAxisPressed(MyJoystickAxesEnum axis)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (axis != MyJoystickAxesEnum.None)) && (this.m_actualJoystickState != null))
            {
                flag = this.GetJoystickAxisStateForGameplay(axis) > 0.5f;
            }
            if (flag || (axis != MyJoystickAxesEnum.None))
            {
                return (this.IsJoystickAxisSupported(axis) ? flag : false);
            }
            return true;
        }

        public bool IsJoystickAxisReleased(MyJoystickAxesEnum axis)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (axis != MyJoystickAxesEnum.None)) && (this.m_actualJoystickState != null))
            {
                flag = this.GetJoystickAxisStateForGameplay(axis) <= 0.5f;
            }
            if (flag || (axis != MyJoystickAxesEnum.None))
            {
                return (this.IsJoystickAxisSupported(axis) ? flag : false);
            }
            return true;
        }

        public bool IsJoystickAxisSupported(MyJoystickAxesEnum axis)
        {
            if (this.m_joystickConnected)
            {
                switch (axis)
                {
                    case MyJoystickAxesEnum.Xpos:
                    case MyJoystickAxesEnum.Xneg:
                        return this.m_joystickXAxisSupported;

                    case MyJoystickAxesEnum.Ypos:
                    case MyJoystickAxesEnum.Yneg:
                        return this.m_joystickYAxisSupported;

                    case MyJoystickAxesEnum.Zpos:
                    case MyJoystickAxesEnum.Zneg:
                        return this.m_joystickZAxisSupported;

                    case MyJoystickAxesEnum.RotationXpos:
                    case MyJoystickAxesEnum.RotationXneg:
                        return this.m_joystickRotationXAxisSupported;

                    case MyJoystickAxesEnum.RotationYpos:
                    case MyJoystickAxesEnum.RotationYneg:
                        return this.m_joystickRotationYAxisSupported;

                    case MyJoystickAxesEnum.RotationZpos:
                    case MyJoystickAxesEnum.RotationZneg:
                        return this.m_joystickRotationZAxisSupported;

                    case MyJoystickAxesEnum.Slider1pos:
                    case MyJoystickAxesEnum.Slider1neg:
                        return this.m_joystickSlider1AxisSupported;

                    case MyJoystickAxesEnum.Slider2pos:
                    case MyJoystickAxesEnum.Slider2neg:
                        return this.m_joystickSlider2AxisSupported;
                }
            }
            return false;
        }

        public bool IsJoystickAxisValid(MyJoystickAxesEnum axis)
        {
            using (List<MyJoystickAxesEnum>.Enumerator enumerator = this.m_validJoystickAxes.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (((MyJoystickAxesEnum) enumerator.Current) == axis)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsJoystickButtonNewPressed(MyJoystickButtonsEnum button)
        {
            bool flag = false;
            if ((this.m_joystickConnected && ((button != MyJoystickButtonsEnum.None) && (this.m_actualJoystickState != null))) && (this.m_previousJoystickState != null))
            {
                switch (button)
                {
                    case MyJoystickButtonsEnum.JDLeft:
                        flag = this.IsNewGamepadKeyLeftPressed();
                        break;

                    case MyJoystickButtonsEnum.JDRight:
                        flag = this.IsNewGamepadKeyRightPressed();
                        break;

                    case MyJoystickButtonsEnum.JDUp:
                        flag = this.IsNewGamepadKeyUpPressed();
                        break;

                    case MyJoystickButtonsEnum.JDDown:
                        flag = this.IsNewGamepadKeyDownPressed();
                        break;

                    default:
                        flag = this.m_actualJoystickState.IsPressed((((int) button) - 5)) && !this.m_previousJoystickState.IsPressed((((int) button) - 5));
                        break;
                }
            }
            if (flag || (button != MyJoystickButtonsEnum.None))
            {
                return flag;
            }
            return true;
        }

        public bool IsJoystickButtonPressed(MyJoystickButtonsEnum button)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (button != MyJoystickButtonsEnum.None)) && (this.m_actualJoystickState != null))
            {
                switch (button)
                {
                    case MyJoystickButtonsEnum.JDLeft:
                        flag = this.IsGamepadKeyLeftPressed();
                        break;

                    case MyJoystickButtonsEnum.JDRight:
                        flag = this.IsGamepadKeyRightPressed();
                        break;

                    case MyJoystickButtonsEnum.JDUp:
                        flag = this.IsGamepadKeyUpPressed();
                        break;

                    case MyJoystickButtonsEnum.JDDown:
                        flag = this.IsGamepadKeyDownPressed();
                        break;

                    default:
                        flag = this.m_actualJoystickState.Buttons[((int) button) - 5];
                        break;
                }
            }
            if (flag || (button != MyJoystickButtonsEnum.None))
            {
                return flag;
            }
            return true;
        }

        public bool IsJoystickButtonReleased(MyJoystickButtonsEnum button)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (button != MyJoystickButtonsEnum.None)) && (this.m_actualJoystickState != null))
            {
                switch (button)
                {
                    case MyJoystickButtonsEnum.JDLeft:
                        flag = !this.IsGamepadKeyLeftPressed();
                        break;

                    case MyJoystickButtonsEnum.JDRight:
                        flag = !this.IsGamepadKeyRightPressed();
                        break;

                    case MyJoystickButtonsEnum.JDUp:
                        flag = !this.IsGamepadKeyUpPressed();
                        break;

                    case MyJoystickButtonsEnum.JDDown:
                        flag = !this.IsGamepadKeyDownPressed();
                        break;

                    default:
                        flag = this.m_actualJoystickState.IsReleased(((int) button) - 5);
                        break;
                }
            }
            if (flag || (button != MyJoystickButtonsEnum.None))
            {
                return flag;
            }
            return true;
        }

        public bool IsJoystickButtonValid(MyJoystickButtonsEnum button)
        {
            using (List<MyJoystickButtonsEnum>.Enumerator enumerator = this.m_validJoystickButtons.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (((MyJoystickButtonsEnum) enumerator.Current) == button)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsJoystickConnected() => 
            this.m_joystickConnected;

        public bool IsKeyDigit(MyKeys key) => 
            this.m_digitKeys.Contains(key);

        public bool IsKeyPress(MyKeys key) => 
            this.m_keyboardState.IsKeyDown(key);

        public bool IsKeyValid(MyKeys key)
        {
            using (List<MyKeys>.Enumerator enumerator = this.m_validKeyboardKeys.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (((MyKeys) enumerator.Current) == key)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsLeftCtrlKeyPressed() => 
            this.IsKeyPress(MyKeys.LeftControl);

        public bool IsLeftMousePressed() => 
            this.m_actualMouseState.LeftButton;

        public bool IsLeftMouseReleased() => 
            !this.m_actualMouseState.LeftButton;

        public bool IsMiddleMousePressed() => 
            this.m_actualMouseState.MiddleButton;

        public bool IsMiddleMouseReleased() => 
            !this.m_actualMouseState.MiddleButton;

        public bool IsMouseButtonValid(MyMouseButtonsEnum button)
        {
            using (List<MyMouseButtonsEnum>.Enumerator enumerator = this.m_validMouseButtons.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (((MyMouseButtonsEnum) enumerator.Current) == button)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsMousePressed(MyMouseButtonsEnum button)
        {
            switch (button)
            {
                case MyMouseButtonsEnum.Left:
                    return this.IsLeftMousePressed();

                case MyMouseButtonsEnum.Middle:
                    return this.IsMiddleMousePressed();

                case MyMouseButtonsEnum.Right:
                    return this.IsRightMousePressed();

                case MyMouseButtonsEnum.XButton1:
                    return this.IsXButton1MousePressed();

                case MyMouseButtonsEnum.XButton2:
                    return this.IsXButton2MousePressed();
            }
            return false;
        }

        public bool IsMouseReleased(MyMouseButtonsEnum button)
        {
            switch (button)
            {
                case MyMouseButtonsEnum.Left:
                    return this.IsLeftMouseReleased();

                case MyMouseButtonsEnum.Middle:
                    return this.IsMiddleMouseReleased();

                case MyMouseButtonsEnum.Right:
                    return this.IsRightMouseReleased();

                case MyMouseButtonsEnum.XButton1:
                    return this.IsXButton1MouseReleased();

                case MyMouseButtonsEnum.XButton2:
                    return this.IsXButton2MouseReleased();
            }
            return false;
        }

        public bool IsNewButtonPressed(MySharedButtonsEnum button) => 
            ((button == MySharedButtonsEnum.Primary) ? this.IsNewPrimaryButtonPressed() : ((button == MySharedButtonsEnum.Secondary) ? this.IsNewSecondaryButtonPressed() : false));

        public bool IsNewButtonReleased(MySharedButtonsEnum button) => 
            ((button == MySharedButtonsEnum.Primary) ? this.IsNewPrimaryButtonReleased() : ((button == MySharedButtonsEnum.Secondary) ? this.IsNewSecondaryButtonReleased() : false));

        public bool IsNewGameControlJoystickOnlyPressed(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (this.m_gameControlsList.TryGetValue(controlId, out control) && control.IsNewJoystickPressed()) : false);
        }

        public bool IsNewGameControlJoystickOnlyReleased(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (this.m_gameControlsList.TryGetValue(controlId, out control) && control.IsNewJoystickReleased()) : false);
        }

        public bool IsNewGameControlPressed(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (this.m_gameControlsList.TryGetValue(controlId, out control) && control.IsNewPressed()) : false);
        }

        public bool IsNewGameControlReleased(MyStringId controlId)
        {
            MyControl control;
            return (!this.IsControlBlocked(controlId) ? (this.m_gameControlsList.TryGetValue(controlId, out control) && control.IsNewReleased()) : false);
        }

        public bool IsNewGamepadKeyDownPressed() => 
            (!this.WasGamepadKeyDownPressed() && this.IsGamepadKeyDownPressed());

        public bool IsNewGamepadKeyDownReleased() => 
            (this.WasGamepadKeyDownPressed() && !this.IsGamepadKeyDownPressed());

        public bool IsNewGamepadKeyLeftPressed() => 
            (!this.WasGamepadKeyLeftPressed() && this.IsGamepadKeyLeftPressed());

        public bool IsNewGamepadKeyLeftReleased() => 
            (this.WasGamepadKeyLeftPressed() && !this.IsGamepadKeyLeftPressed());

        public bool IsNewGamepadKeyRightPressed() => 
            (!this.WasGamepadKeyRightPressed() && this.IsGamepadKeyRightPressed());

        public bool IsNewGamepadKeyRightReleased() => 
            (this.WasGamepadKeyRightPressed() && !this.IsGamepadKeyRightPressed());

        public bool IsNewGamepadKeyUpPressed() => 
            (!this.WasGamepadKeyUpPressed() && this.IsGamepadKeyUpPressed());

        public bool IsNewGamepadKeyUpReleased() => 
            (this.WasGamepadKeyUpPressed() && !this.IsGamepadKeyUpPressed());

        public bool IsNewJoystickAxisReleased(MyJoystickAxesEnum axis)
        {
            bool flag = false;
            if ((this.m_joystickConnected && ((axis != MyJoystickAxesEnum.None) && (this.m_actualJoystickState != null))) && (this.m_previousJoystickState != null))
            {
                flag = (this.GetJoystickAxisStateForGameplay(axis) <= 0.5f) && (this.GetPreviousJoystickAxisStateForGameplay(axis) > 0.5f);
            }
            if (flag || (axis != MyJoystickAxesEnum.None))
            {
                return (this.IsJoystickAxisSupported(axis) ? flag : false);
            }
            return true;
        }

        public bool IsNewJoystickButtonReleased(MyJoystickButtonsEnum button)
        {
            bool flag = false;
            if ((this.m_joystickConnected && ((button != MyJoystickButtonsEnum.None) && (this.m_actualJoystickState != null))) && (this.m_previousJoystickState != null))
            {
                switch (button)
                {
                    case MyJoystickButtonsEnum.JDLeft:
                        flag = this.IsNewGamepadKeyLeftReleased();
                        break;

                    case MyJoystickButtonsEnum.JDRight:
                        flag = this.IsNewGamepadKeyRightReleased();
                        break;

                    case MyJoystickButtonsEnum.JDUp:
                        flag = this.IsNewGamepadKeyUpReleased();
                        break;

                    case MyJoystickButtonsEnum.JDDown:
                        flag = this.IsNewGamepadKeyDownReleased();
                        break;

                    default:
                        flag = this.m_actualJoystickState.IsReleased((((int) button) - 5)) && this.m_previousJoystickState.IsPressed((((int) button) - 5));
                        break;
                }
            }
            if (flag || (button != MyJoystickButtonsEnum.None))
            {
                return flag;
            }
            return true;
        }

        public bool IsNewKeyPressed(MyKeys key) => 
            (this.m_keyboardState.IsKeyDown(key) && this.m_keyboardState.IsPreviousKeyUp(key));

        public bool IsNewKeyReleased(MyKeys key) => 
            (this.m_keyboardState.IsKeyUp(key) && this.m_keyboardState.IsPreviousKeyDown(key));

        public bool IsNewLeftMousePressed() => 
            (this.IsLeftMousePressed() && this.WasLeftMouseReleased());

        public bool IsNewLeftMouseReleased() => 
            (this.IsLeftMouseReleased() && this.WasLeftMousePressed());

        public bool IsNewMiddleMousePressed() => 
            (this.m_actualMouseState.MiddleButton && !this.m_previousMouseState.MiddleButton);

        public bool IsNewMiddleMouseReleased() => 
            (!this.m_actualMouseState.MiddleButton && this.m_previousMouseState.MiddleButton);

        public bool IsNewMousePressed(MyMouseButtonsEnum button)
        {
            switch (button)
            {
                case MyMouseButtonsEnum.Left:
                    return this.IsNewLeftMousePressed();

                case MyMouseButtonsEnum.Middle:
                    return this.IsNewMiddleMousePressed();

                case MyMouseButtonsEnum.Right:
                    return this.IsNewRightMousePressed();

                case MyMouseButtonsEnum.XButton1:
                    return this.IsNewXButton1MousePressed();

                case MyMouseButtonsEnum.XButton2:
                    return this.IsNewXButton2MousePressed();
            }
            return false;
        }

        public bool IsNewMouseReleased(MyMouseButtonsEnum button)
        {
            switch (button)
            {
                case MyMouseButtonsEnum.Left:
                    return this.IsNewLeftMouseReleased();

                case MyMouseButtonsEnum.Middle:
                    return this.IsNewMiddleMouseReleased();

                case MyMouseButtonsEnum.Right:
                    return this.IsNewRightMouseReleased();

                case MyMouseButtonsEnum.XButton1:
                    return this.IsNewXButton1MouseReleased();

                case MyMouseButtonsEnum.XButton2:
                    return this.IsNewXButton2MouseReleased();
            }
            return false;
        }

        public bool IsNewPrimaryButtonPressed() => 
            (this.IsNewLeftMousePressed() || this.IsJoystickButtonNewPressed(MyJoystickButtonsEnum.J01));

        public bool IsNewPrimaryButtonReleased() => 
            (this.IsNewLeftMouseReleased() || this.IsNewJoystickButtonReleased(MyJoystickButtonsEnum.J01));

        public bool IsNewRightMousePressed() => 
            (this.m_actualMouseState.RightButton && !this.m_previousMouseState.RightButton);

        public bool IsNewRightMouseReleased() => 
            (!this.m_actualMouseState.RightButton && this.m_previousMouseState.RightButton);

        public bool IsNewSecondaryButtonPressed() => 
            (this.IsNewRightMousePressed() || this.IsJoystickButtonNewPressed(MyJoystickButtonsEnum.J02));

        public bool IsNewSecondaryButtonReleased() => 
            (this.IsNewRightMouseReleased() || this.IsNewJoystickButtonReleased(MyJoystickButtonsEnum.J02));

        public bool IsNewXButton1MousePressed() => 
            (this.m_actualMouseState.XButton1 && !this.m_previousMouseState.XButton1);

        public bool IsNewXButton1MouseReleased() => 
            (!this.m_actualMouseState.XButton1 && this.m_previousMouseState.XButton1);

        public bool IsNewXButton2MousePressed() => 
            (this.m_actualMouseState.XButton2 && !this.m_previousMouseState.XButton2);

        public bool IsNewXButton2MouseReleased() => 
            (!this.m_actualMouseState.XButton2 && this.m_previousMouseState.XButton2);

        public bool IsPrimaryButtonPressed() => 
            (this.IsLeftMousePressed() || this.IsJoystickButtonPressed(MyJoystickButtonsEnum.J01));

        public bool IsPrimaryButtonReleased() => 
            (this.IsLeftMouseReleased() || this.IsJoystickButtonReleased(MyJoystickButtonsEnum.J01));

        public bool IsRightCtrlKeyPressed() => 
            this.IsKeyPress(MyKeys.RightControl);

        public bool IsRightMousePressed() => 
            this.m_actualMouseState.RightButton;

        public bool IsRightMouseReleased() => 
            !this.m_actualMouseState.RightButton;

        public bool IsSecondaryButtonPressed() => 
            (this.IsRightMousePressed() || this.IsJoystickButtonPressed(MyJoystickButtonsEnum.J02));

        public bool IsSecondaryButtonReleased() => 
            (this.IsRightMouseReleased() || this.IsJoystickButtonReleased(MyJoystickButtonsEnum.J02));

        public bool IsXButton1MousePressed() => 
            this.m_actualMouseState.XButton1;

        public bool IsXButton1MouseReleased() => 
            !this.m_actualMouseState.XButton1;

        public bool IsXButton2MousePressed() => 
            this.m_actualMouseState.XButton2;

        public bool IsXButton2MouseReleased() => 
            !this.m_actualMouseState.XButton2;

        public void LoadContent(IntPtr windowHandle)
        {
            this.m_windowHandle = windowHandle;
            MyWindowsMouse.SetWindow(windowHandle);
            MyDirectInput.Initialize(windowHandle);
            this.InitDevicePluginHandlerCallBack();
            if (this.ENABLE_DEVELOPER_KEYS)
            {
                MyLog.Default.WriteLine("DEVELOPER KEYS ENABLED");
            }
            this.m_keyboardState = new MyGuiLocalizedKeyboardState();
        }

        private bool LoadControls(SerializableDictionary<string, object> controlsGeneral, SerializableDictionary<string, object> controlsButtons)
        {
            if (controlsGeneral.Dictionary.Count == 0)
            {
                MyLog.Default.WriteLine("    Loading default controls");
                this.RevertToDefaultControls();
                return false;
            }
            try
            {
                this.m_mouseXIsInverted = bool.Parse((string) controlsGeneral["mouseXIsInverted"]);
                this.m_mouseYIsInverted = bool.Parse((string) controlsGeneral["mouseYIsInverted"]);
                this.m_mouseSensitivity = float.Parse((string) controlsGeneral["mouseSensitivity"], CultureInfo.InvariantCulture);
                this.JoystickInstanceName = (string) controlsGeneral["joystickInstanceName"];
                this.m_joystickSensitivity = float.Parse((string) controlsGeneral["joystickSensitivity"], CultureInfo.InvariantCulture);
                this.m_joystickExponent = float.Parse((string) controlsGeneral["joystickExponent"], CultureInfo.InvariantCulture);
                this.m_joystickDeadzone = float.Parse((string) controlsGeneral["joystickDeadzone"], CultureInfo.InvariantCulture);
                this.LoadGameControls(controlsButtons);
                return true;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("    Error loading controls from config:");
                MyLog.Default.WriteLine(exception);
                MyLog.Default.WriteLine("    Loading default controls");
                this.RevertToDefaultControls();
                return false;
            }
        }

        public void LoadData(SerializableDictionary<string, object> controlsGeneral, SerializableDictionary<string, object> controlsButtons)
        {
            this.m_mouseXIsInverted = this.IsMouseXInvertedDefault;
            this.m_mouseYIsInverted = this.IsMouseYInvertedDefault;
            this.m_mouseSensitivity = this.MouseSensitivityDefault;
            this.m_joystickInstanceName = this.JoystickInstanceNameDefault;
            this.m_joystickSensitivity = this.JoystickSensitivityDefault;
            this.m_joystickDeadzone = this.JoystickDeadzoneDefault;
            this.m_joystickExponent = this.JoystickExponentDefault;
            this.m_digitKeys.Add(MyKeys.D0);
            this.m_digitKeys.Add(MyKeys.D1);
            this.m_digitKeys.Add(MyKeys.D2);
            this.m_digitKeys.Add(MyKeys.D3);
            this.m_digitKeys.Add(MyKeys.D4);
            this.m_digitKeys.Add(MyKeys.D5);
            this.m_digitKeys.Add(MyKeys.D6);
            this.m_digitKeys.Add(MyKeys.D7);
            this.m_digitKeys.Add(MyKeys.D8);
            this.m_digitKeys.Add(MyKeys.D9);
            this.m_digitKeys.Add(MyKeys.NumPad0);
            this.m_digitKeys.Add(MyKeys.NumPad1);
            this.m_digitKeys.Add(MyKeys.NumPad2);
            this.m_digitKeys.Add(MyKeys.NumPad3);
            this.m_digitKeys.Add(MyKeys.NumPad4);
            this.m_digitKeys.Add(MyKeys.NumPad5);
            this.m_digitKeys.Add(MyKeys.NumPad6);
            this.m_digitKeys.Add(MyKeys.NumPad7);
            this.m_digitKeys.Add(MyKeys.NumPad8);
            this.m_digitKeys.Add(MyKeys.NumPad9);
            this.m_validKeyboardKeys.Add(MyKeys.A);
            this.m_validKeyboardKeys.Add(MyKeys.Add);
            this.m_validKeyboardKeys.Add(MyKeys.B);
            this.m_validKeyboardKeys.Add(MyKeys.Back);
            this.m_validKeyboardKeys.Add(MyKeys.C);
            this.m_validKeyboardKeys.Add(MyKeys.CapsLock);
            this.m_validKeyboardKeys.Add(MyKeys.D);
            this.m_validKeyboardKeys.Add(MyKeys.D0);
            this.m_validKeyboardKeys.Add(MyKeys.D1);
            this.m_validKeyboardKeys.Add(MyKeys.D2);
            this.m_validKeyboardKeys.Add(MyKeys.D3);
            this.m_validKeyboardKeys.Add(MyKeys.D4);
            this.m_validKeyboardKeys.Add(MyKeys.D5);
            this.m_validKeyboardKeys.Add(MyKeys.D6);
            this.m_validKeyboardKeys.Add(MyKeys.D7);
            this.m_validKeyboardKeys.Add(MyKeys.D8);
            this.m_validKeyboardKeys.Add(MyKeys.D9);
            this.m_validKeyboardKeys.Add(MyKeys.Decimal);
            this.m_validKeyboardKeys.Add(MyKeys.Delete);
            this.m_validKeyboardKeys.Add(MyKeys.Divide);
            this.m_validKeyboardKeys.Add(MyKeys.Down);
            this.m_validKeyboardKeys.Add(MyKeys.E);
            this.m_validKeyboardKeys.Add(MyKeys.End);
            this.m_validKeyboardKeys.Add(MyKeys.Enter);
            this.m_validKeyboardKeys.Add(MyKeys.F);
            this.m_validKeyboardKeys.Add(MyKeys.G);
            this.m_validKeyboardKeys.Add(MyKeys.H);
            this.m_validKeyboardKeys.Add(MyKeys.Home);
            this.m_validKeyboardKeys.Add(MyKeys.I);
            this.m_validKeyboardKeys.Add(MyKeys.Insert);
            this.m_validKeyboardKeys.Add(MyKeys.J);
            this.m_validKeyboardKeys.Add(MyKeys.K);
            this.m_validKeyboardKeys.Add(MyKeys.L);
            this.m_validKeyboardKeys.Add(MyKeys.Left);
            this.m_validKeyboardKeys.Add(MyKeys.LeftAlt);
            this.m_validKeyboardKeys.Add(MyKeys.LeftControl);
            this.m_validKeyboardKeys.Add(MyKeys.LeftShift);
            this.m_validKeyboardKeys.Add(MyKeys.M);
            this.m_validKeyboardKeys.Add(MyKeys.Multiply);
            this.m_validKeyboardKeys.Add(MyKeys.N);
            this.m_validKeyboardKeys.Add(MyKeys.None);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad0);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad1);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad2);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad3);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad4);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad5);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad6);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad7);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad8);
            this.m_validKeyboardKeys.Add(MyKeys.NumPad9);
            this.m_validKeyboardKeys.Add(MyKeys.O);
            this.m_validKeyboardKeys.Add(MyKeys.OemCloseBrackets);
            this.m_validKeyboardKeys.Add(MyKeys.OemComma);
            this.m_validKeyboardKeys.Add(MyKeys.OemMinus);
            this.m_validKeyboardKeys.Add(MyKeys.OemOpenBrackets);
            this.m_validKeyboardKeys.Add(MyKeys.OemPeriod);
            this.m_validKeyboardKeys.Add(MyKeys.OemPipe);
            this.m_validKeyboardKeys.Add(MyKeys.OemPlus);
            this.m_validKeyboardKeys.Add(MyKeys.OemQuestion);
            this.m_validKeyboardKeys.Add(MyKeys.OemQuotes);
            this.m_validKeyboardKeys.Add(MyKeys.OemSemicolon);
            this.m_validKeyboardKeys.Add(MyKeys.OemTilde);
            this.m_validKeyboardKeys.Add(MyKeys.OemBackslash);
            this.m_validKeyboardKeys.Add(MyKeys.P);
            this.m_validKeyboardKeys.Add(MyKeys.PageDown);
            this.m_validKeyboardKeys.Add(MyKeys.PageUp);
            this.m_validKeyboardKeys.Add(MyKeys.Pause);
            this.m_validKeyboardKeys.Add(MyKeys.Q);
            this.m_validKeyboardKeys.Add(MyKeys.R);
            this.m_validKeyboardKeys.Add(MyKeys.Right);
            this.m_validKeyboardKeys.Add(MyKeys.RightAlt);
            this.m_validKeyboardKeys.Add(MyKeys.RightControl);
            this.m_validKeyboardKeys.Add(MyKeys.RightShift);
            this.m_validKeyboardKeys.Add(MyKeys.S);
            this.m_validKeyboardKeys.Add(MyKeys.Space);
            this.m_validKeyboardKeys.Add(MyKeys.Subtract);
            this.m_validKeyboardKeys.Add(MyKeys.T);
            this.m_validKeyboardKeys.Add(MyKeys.Tab);
            this.m_validKeyboardKeys.Add(MyKeys.U);
            this.m_validKeyboardKeys.Add(MyKeys.Up);
            this.m_validKeyboardKeys.Add(MyKeys.V);
            this.m_validKeyboardKeys.Add(MyKeys.W);
            this.m_validKeyboardKeys.Add(MyKeys.X);
            this.m_validKeyboardKeys.Add(MyKeys.Y);
            this.m_validKeyboardKeys.Add(MyKeys.Z);
            this.m_validKeyboardKeys.Add(MyKeys.F1);
            this.m_validKeyboardKeys.Add(MyKeys.F2);
            this.m_validKeyboardKeys.Add(MyKeys.F3);
            this.m_validKeyboardKeys.Add(MyKeys.F4);
            this.m_validKeyboardKeys.Add(MyKeys.F5);
            this.m_validKeyboardKeys.Add(MyKeys.F6);
            this.m_validKeyboardKeys.Add(MyKeys.F7);
            this.m_validKeyboardKeys.Add(MyKeys.F8);
            this.m_validKeyboardKeys.Add(MyKeys.F9);
            this.m_validKeyboardKeys.Add(MyKeys.F10);
            this.m_validKeyboardKeys.Add(MyKeys.F11);
            this.m_validKeyboardKeys.Add(MyKeys.F12);
            this.m_validMouseButtons.Add(MyMouseButtonsEnum.Left);
            this.m_validMouseButtons.Add(MyMouseButtonsEnum.Middle);
            this.m_validMouseButtons.Add(MyMouseButtonsEnum.Right);
            this.m_validMouseButtons.Add(MyMouseButtonsEnum.XButton1);
            this.m_validMouseButtons.Add(MyMouseButtonsEnum.XButton2);
            this.m_validMouseButtons.Add(MyMouseButtonsEnum.None);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J01);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J02);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J03);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J04);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J05);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J06);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J07);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J08);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J09);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J10);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J11);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J12);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J13);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J14);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J15);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.J16);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.JDLeft);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.JDRight);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.JDUp);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.JDDown);
            this.m_validJoystickButtons.Add(MyJoystickButtonsEnum.None);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Xpos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Xneg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Ypos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Yneg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Zpos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Zneg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.RotationXpos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.RotationXneg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.RotationYpos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.RotationYneg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.RotationZpos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.RotationZneg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Slider1pos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Slider1neg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Slider2pos);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.Slider2neg);
            this.m_validJoystickAxes.Add(MyJoystickAxesEnum.None);
            this.CheckValidControls(this.m_defaultGameControlsList);
            this.LoadControls(controlsGeneral, controlsButtons);
            this.TakeSnapshot();
            this.ClearBlacklist();
        }

        private void LoadGameControl(string controlName, MyStringId controlType, MyGuiInputDeviceEnum device)
        {
            switch (device)
            {
                case MyGuiInputDeviceEnum.None:
                case (MyGuiInputDeviceEnum.Keyboard | MyGuiInputDeviceEnum.Mouse):
                case ((MyGuiInputDeviceEnum) 4):
                    break;

                case MyGuiInputDeviceEnum.Keyboard:
                {
                    MyKeys key = (MyKeys) Enum.Parse(typeof(MyKeys), controlName);
                    if (!this.IsKeyValid(key))
                    {
                        throw new Exception("Key \"" + key.ToString() + "\" is already assigned or is not valid.");
                    }
                    this.FindNotAssignedGameControl(controlType, device).SetControl(MyGuiInputDeviceEnum.Keyboard, key);
                    return;
                }
                case MyGuiInputDeviceEnum.Mouse:
                {
                    MyMouseButtonsEnum button = this.ParseMyMouseButtonsEnum(controlName);
                    if (!this.IsMouseButtonValid(button))
                    {
                        throw new Exception("Mouse button \"" + button.ToString() + "\" is already assigned or is not valid.");
                    }
                    this.FindNotAssignedGameControl(controlType, device).SetControl(button);
                    break;
                }
                case MyGuiInputDeviceEnum.KeyboardSecond:
                {
                    MyKeys key = (MyKeys) Enum.Parse(typeof(MyKeys), controlName);
                    if (!this.IsKeyValid(key))
                    {
                        throw new Exception("Key \"" + key.ToString() + "\" is already assigned or is not valid.");
                    }
                    this.FindNotAssignedGameControl(controlType, device).SetControl(MyGuiInputDeviceEnum.KeyboardSecond, key);
                    return;
                }
                default:
                    return;
            }
        }

        private void LoadGameControls(SerializableDictionary<string, object> controlsButtons)
        {
            if (controlsButtons.Dictionary.Count == 0)
            {
                throw new Exception("ControlsButtons config parameter is empty.");
            }
            foreach (KeyValuePair<string, object> pair in controlsButtons.Dictionary)
            {
                MyStringId? nullable = this.TryParseMyGameControlEnums(pair.Key);
                if (nullable != null)
                {
                    this.m_gameControlsList[nullable.Value].SetNoControl();
                    SerializableDictionary<string, string> dictionary = (SerializableDictionary<string, string>) pair.Value;
                    this.LoadGameControl(dictionary["Keyboard"], nullable.Value, this.ParseMyGuiInputDeviceEnum("Keyboard"));
                    this.LoadGameControl(dictionary["Keyboard2"], nullable.Value, this.ParseMyGuiInputDeviceEnum("KeyboardSecond"));
                    this.LoadGameControl(dictionary["Mouse"], nullable.Value, this.ParseMyGuiInputDeviceEnum("Mouse"));
                }
            }
        }

        public int MouseScrollWheelValue() => 
            this.m_actualMouseState.ScrollWheelValue;

        public void NegateEscapePress()
        {
            this.m_keyboardState.NegateEscapePress();
        }

        public MyGuiControlTypeEnum ParseMyGuiControlTypeEnum(string s)
        {
            for (int i = 0; i < MyEnumsToStrings.ControlTypeEnum.Length; i++)
            {
                if (MyEnumsToStrings.ControlTypeEnum[i] == s)
                {
                    return (MyGuiControlTypeEnum) ((byte) i);
                }
            }
            throw new ArgumentException("Value \"" + s + "\" is not from MyGuiInputTypeEnum.", "s");
        }

        public MyGuiInputDeviceEnum ParseMyGuiInputDeviceEnum(string s)
        {
            for (int i = 0; i < MyEnumsToStrings.GuiInputDeviceEnum.Length; i++)
            {
                if (MyEnumsToStrings.GuiInputDeviceEnum[i] == s)
                {
                    return (MyGuiInputDeviceEnum) ((byte) i);
                }
            }
            throw new ArgumentException("Value \"" + s + "\" is not from GuiInputDeviceEnum.", "s");
        }

        public MyJoystickAxesEnum ParseMyJoystickAxesEnum(string s)
        {
            for (int i = 0; i < MyEnumsToStrings.JoystickAxesEnum.Length; i++)
            {
                if (MyEnumsToStrings.JoystickAxesEnum[i] == s)
                {
                    return (MyJoystickAxesEnum) ((byte) i);
                }
            }
            throw new ArgumentException("Value \"" + s + "\" is not from JoystickAxesEnum.", "s");
        }

        public MyJoystickButtonsEnum ParseMyJoystickButtonsEnum(string s)
        {
            for (int i = 0; i < MyEnumsToStrings.JoystickButtonsEnum.Length; i++)
            {
                if (MyEnumsToStrings.JoystickButtonsEnum[i] == s)
                {
                    return (MyJoystickButtonsEnum) ((byte) i);
                }
            }
            throw new ArgumentException("Value \"" + s + "\" is not from JoystickButtonsEnum.", "s");
        }

        public MyMouseButtonsEnum ParseMyMouseButtonsEnum(string s)
        {
            for (int i = 0; i < MyEnumsToStrings.MouseButtonsEnum.Length; i++)
            {
                if (MyEnumsToStrings.MouseButtonsEnum[i] == s)
                {
                    return (MyMouseButtonsEnum) ((byte) i);
                }
            }
            throw new ArgumentException("Value \"" + s + "\" is not from MouseButtonsEnum.", "s");
        }

        public int[] POVDirection() => 
            (!this.m_joystickConnected ? null : this.m_actualJoystickState.PointOfViewControllers);

        public int PreviousMouseScrollWheelValue() => 
            this.m_previousMouseState.ScrollWheelValue;

        public void RevertChanges()
        {
            this.JoystickInstanceName = this.m_joystickInstanceNameSnapshot;
            this.CloneControls(this.m_gameControlsSnapshot, this.m_gameControlsList);
        }

        public void RevertToDefaultControls()
        {
            this.m_mouseXIsInverted = this.IsMouseXInvertedDefault;
            this.m_mouseYIsInverted = this.IsMouseYInvertedDefault;
            this.m_mouseSensitivity = this.MouseSensitivityDefault;
            this.m_joystickSensitivity = this.JoystickSensitivityDefault;
            this.m_joystickDeadzone = this.JoystickDeadzoneDefault;
            this.m_joystickExponent = this.JoystickExponentDefault;
            this.CloneControls(this.m_defaultGameControlsList, this.m_gameControlsList);
        }

        public void SaveControls(SerializableDictionary<string, object> controlsGeneral, SerializableDictionary<string, object> controlsButtons)
        {
            controlsGeneral.Dictionary.Clear();
            controlsGeneral.Dictionary.Add("mouseXIsInverted", this.m_mouseXIsInverted.ToString());
            controlsGeneral.Dictionary.Add("mouseYIsInverted", this.m_mouseYIsInverted.ToString());
            controlsGeneral.Dictionary.Add("mouseSensitivity", this.m_mouseSensitivity.ToString(CultureInfo.InvariantCulture));
            controlsGeneral.Dictionary.Add("joystickInstanceName", this.m_joystickInstanceName);
            controlsGeneral.Dictionary.Add("joystickSensitivity", this.m_joystickSensitivity.ToString(CultureInfo.InvariantCulture));
            controlsGeneral.Dictionary.Add("joystickExponent", this.m_joystickExponent.ToString(CultureInfo.InvariantCulture));
            controlsGeneral.Dictionary.Add("joystickDeadzone", this.m_joystickDeadzone.ToString(CultureInfo.InvariantCulture));
            controlsButtons.Dictionary.Clear();
            foreach (MyControl control in this.m_gameControlsList.Values)
            {
                SerializableDictionary<string, string> dictionary = new SerializableDictionary<string, string>();
                MyStringId gameControlEnum = control.GetGameControlEnum();
                controlsButtons[gameControlEnum.ToString()] = dictionary;
                dictionary["Keyboard"] = control.GetKeyboardControl().ToString();
                dictionary["Keyboard2"] = control.GetSecondKeyboardControl().ToString();
                dictionary["Mouse"] = MyEnumsToStrings.MouseButtonsEnum[(int) control.GetMouseControl()];
            }
        }

        public void SetControlBlock(MyStringId controlEnum, bool block = false)
        {
            if (block)
            {
                this.m_gameControlsBlacklist.Add(controlEnum);
            }
            else
            {
                this.m_gameControlsBlacklist.Remove(controlEnum);
            }
        }

        public void SetJoystickConnected(bool value)
        {
            if (this.m_joystickConnected != value)
            {
                this.m_joystickConnected = value;
                if (this.JoystickConnected != null)
                {
                    this.JoystickConnected(value);
                }
            }
        }

        public void SetJoystickDeadzone(float newDeadzone)
        {
            this.m_joystickDeadzone = newDeadzone;
        }

        public void SetJoystickExponent(float newExponent)
        {
            this.m_joystickExponent = newExponent;
        }

        public void SetJoystickSensitivity(float newSensitivity)
        {
            this.m_joystickSensitivity = newSensitivity;
        }

        public void SetMousePosition(int x, int y)
        {
            MyWindowsMouse.SetPosition(x, y);
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            this.m_mouseSensitivity = sensitivity;
        }

        public void SetMouseXInversion(bool inverted)
        {
            this.m_mouseXIsInverted = inverted;
        }

        public void SetMouseYInversion(bool inverted)
        {
            this.m_mouseYIsInverted = inverted;
        }

        public void TakeSnapshot()
        {
            this.m_joystickInstanceNameSnapshot = this.JoystickInstanceName;
            this.CloneControls(this.m_gameControlsList, this.m_gameControlsSnapshot);
        }

        public string TokenEvaluate(string token, string context)
        {
            MyControl control;
            if (!this.m_gameControlsList.TryGetValue(MyStringId.GetOrCompute(token), out control))
            {
                return "";
            }
            string controlButtonName = control.GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
            string str2 = control.GetControlButtonName(MyGuiInputDeviceEnum.Mouse);
            return (string.IsNullOrEmpty(controlButtonName) ? str2 : (string.IsNullOrEmpty(str2) ? controlButtonName : (controlButtonName + "'/'" + str2)));
        }

        public MyStringId? TryParseMyGameControlEnums(string s)
        {
            MyStringId orCompute = MyStringId.GetOrCompute(s);
            if (this.m_gameControlsList.ContainsKey(orCompute))
            {
                return new MyStringId?(orCompute);
            }
            return null;
        }

        private void UninitDevicePluginHandlerCallBack()
        {
            MyMessageLoop.RemoveMessageHandler(WinApi.WM.DEVICECHANGE, new ActionRef<Message>(this.DeviceChangeCallback));
        }

        public void UnloadData()
        {
            this.UninitDevicePluginHandlerCallBack();
            MyDirectInput.Close();
        }

        public bool Update(bool gameFocused)
        {
            if ((!this.m_gameWasFocused & gameFocused) && !this.OverrideUpdate)
            {
                this.UpdateStates();
            }
            this.m_gameWasFocused = gameFocused;
            if (!gameFocused && !this.OverrideUpdate)
            {
                this.ClearStates();
                return false;
            }
            if (!this.OverrideUpdate)
            {
                this.UpdateStates();
            }
            this.m_bufferedInputSource.SwapBufferedTextInput(ref this.m_currentTextInput);
            return true;
        }

        internal unsafe void UpdateStates()
        {
            if (this.m_enabled)
            {
                int num2;
                int num3;
                this.m_previousMouseState = this.m_actualMouseState;
                this.m_keyboardState.UpdateStates();
                this.m_actualMouseStateRaw = MyDirectInput.GetMouseState();
                int num = this.m_actualMouseState.ScrollWheelValue + this.m_actualMouseStateRaw.ScrollWheelValue;
                this.m_actualMouseState = this.m_actualMouseStateRaw;
                this.m_actualMouseState.ScrollWheelValue = num;
                this.m_actualMouseStateRaw.ClearPosition();
                MyWindowsMouse.GetPosition(out num2, out num3);
                this.m_absoluteMousePosition = new Vector2((float) num2, (float) num3);
                MyOpenVR.ClearButtonStates();
                MyOpenVR.PollEvents();
                if (this.IsJoystickConnected())
                {
                    try
                    {
                        this.m_joystick.Acquire();
                        this.m_joystick.Poll();
                        this.m_previousJoystickState = this.m_actualJoystickState;
                        this.m_actualJoystickState = ((Joystick) this.m_joystick).GetCurrentState();
                        if (this.JoystickAsMouse)
                        {
                            float joystickAxisStateForGameplay = this.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum.Xpos);
                            float num6 = this.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum.Ypos);
                            float* singlePtr1 = (float*) ref this.m_absoluteMousePosition.X;
                            singlePtr1[0] += (joystickAxisStateForGameplay + -this.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum.Xneg)) * 4f;
                            float* singlePtr2 = (float*) ref this.m_absoluteMousePosition.Y;
                            singlePtr2[0] += (num6 + -this.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum.Yneg)) * 4f;
                            MyWindowsMouse.SetPosition((int) this.m_absoluteMousePosition.X, (int) this.m_absoluteMousePosition.Y);
                        }
                    }
                    catch
                    {
                        this.SetJoystickConnected(false);
                    }
                }
                if (this.IsJoystickLastUsed)
                {
                    if (this.IsAnyMousePressed() || this.IsAnyKeyPress())
                    {
                        this.IsJoystickLastUsed = false;
                    }
                }
                else if (this.IsAnyJoystickButtonPressed() || this.IsAnyJoystickAxisPressed())
                {
                    this.IsJoystickLastUsed = true;
                }
                this.m_hasher.Keys.Clear();
                this.GetPressedKeys(this.m_hasher.Keys);
                if (!this.ENABLE_DEVELOPER_KEYS && this.m_hasher.TestHash("B12885A220B56226022423E34A12A182", "salt!@#"))
                {
                    this.ENABLE_DEVELOPER_KEYS = true;
                    MyLog.Default.WriteLine("DEVELOPER KEYS ENABLED");
                }
            }
        }

        internal void UpdateStatesFromPlayback(MyKeyboardState currentKeyboard, MyKeyboardState previousKeyboard, MyMouseState currentMouse, MyMouseState previousMouse, JoystickState currentJoystick, JoystickState previousJoystick, int x, int y, List<char> keyboardSnapshotText)
        {
            this.m_keyboardState.UpdateStatesFromSnapshot(currentKeyboard, previousKeyboard);
            this.m_previousMouseState = previousMouse;
            this.m_actualMouseState = currentMouse;
            this.m_actualJoystickState = currentJoystick;
            this.m_previousJoystickState = previousJoystick;
            this.m_absoluteMousePosition = new Vector2((float) x, (float) y);
            if (this.m_gameWasFocused)
            {
                MyWindowsMouse.SetPosition(x, y);
            }
            if (keyboardSnapshotText != null)
            {
                foreach (char ch in keyboardSnapshotText)
                {
                    this.m_bufferedInputSource.AddChar(ch);
                }
            }
        }

        int VRage.ModAPI.IMyInput.DeltaMouseScrollWheelValue() => 
            this.DeltaMouseScrollWheelValue();

        List<string> VRage.ModAPI.IMyInput.EnumerateJoystickNames() => 
            this.EnumerateJoystickNames();

        IMyControl VRage.ModAPI.IMyInput.GetControl(MyKeys key) => 
            this.GetControl(key);

        IMyControl VRage.ModAPI.IMyInput.GetControl(MyMouseButtonsEnum button) => 
            this.GetControl(button);

        IMyControl VRage.ModAPI.IMyInput.GetGameControl(MyStringId controlEnum) => 
            this.GetGameControl(controlEnum);

        float VRage.ModAPI.IMyInput.GetGameControlAnalogState(MyStringId controlEnum) => 
            this.GetGameControlAnalogState(controlEnum);

        float VRage.ModAPI.IMyInput.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis) => 
            this.GetJoystickAxisStateForGameplay(axis);

        string VRage.ModAPI.IMyInput.GetKeyName(MyKeys key) => 
            this.GetKeyName(key);

        void VRage.ModAPI.IMyInput.GetListOfPressedKeys(List<MyKeys> keys)
        {
            this.GetListOfPressedKeys(keys);
        }

        void VRage.ModAPI.IMyInput.GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result)
        {
            this.GetListOfPressedMouseButtons(result);
        }

        Vector2 VRage.ModAPI.IMyInput.GetMouseAreaSize() => 
            this.GetMouseAreaSize();

        Vector2 VRage.ModAPI.IMyInput.GetMousePosition() => 
            this.GetMousePosition();

        float VRage.ModAPI.IMyInput.GetMouseSensitivity() => 
            this.GetMouseSensitivity();

        int VRage.ModAPI.IMyInput.GetMouseX() => 
            this.GetMouseX();

        int VRage.ModAPI.IMyInput.GetMouseXForGamePlay() => 
            this.GetMouseXForGamePlay();

        bool VRage.ModAPI.IMyInput.GetMouseXInversion() => 
            this.GetMouseXInversion();

        int VRage.ModAPI.IMyInput.GetMouseY() => 
            this.GetMouseY();

        int VRage.ModAPI.IMyInput.GetMouseYForGamePlay() => 
            this.GetMouseYForGamePlay();

        bool VRage.ModAPI.IMyInput.GetMouseYInversion() => 
            this.GetMouseYInversion();

        string VRage.ModAPI.IMyInput.GetName(MyJoystickAxesEnum joystickAxis) => 
            this.GetName(joystickAxis);

        string VRage.ModAPI.IMyInput.GetName(MyJoystickButtonsEnum joystickButton) => 
            this.GetName(joystickButton);

        string VRage.ModAPI.IMyInput.GetName(MyMouseButtonsEnum mouseButton) => 
            this.GetName(mouseButton);

        void VRage.ModAPI.IMyInput.GetPressedKeys(List<MyKeys> keys)
        {
            this.GetPressedKeys(keys);
        }

        string VRage.ModAPI.IMyInput.GetUnassignedName() => 
            this.GetUnassignedName();

        bool VRage.ModAPI.IMyInput.IsAnyAltKeyPressed() => 
            this.IsAnyAltKeyPressed();

        bool VRage.ModAPI.IMyInput.IsAnyCtrlKeyPressed() => 
            this.IsAnyCtrlKeyPressed();

        bool VRage.ModAPI.IMyInput.IsAnyKeyPress() => 
            this.IsAnyKeyPress();

        bool VRage.ModAPI.IMyInput.IsAnyMouseOrJoystickPressed() => 
            this.IsAnyMouseOrJoystickPressed();

        bool VRage.ModAPI.IMyInput.IsAnyMousePressed() => 
            this.IsAnyMousePressed();

        bool VRage.ModAPI.IMyInput.IsAnyNewMouseOrJoystickPressed() => 
            this.IsAnyNewMouseOrJoystickPressed();

        bool VRage.ModAPI.IMyInput.IsAnyNewMousePressed() => 
            this.IsAnyNewMousePressed();

        bool VRage.ModAPI.IMyInput.IsAnyShiftKeyPressed() => 
            this.IsAnyShiftKeyPressed();

        bool VRage.ModAPI.IMyInput.IsButtonPressed(MySharedButtonsEnum button) => 
            this.IsButtonPressed(button);

        bool VRage.ModAPI.IMyInput.IsButtonReleased(MySharedButtonsEnum button) => 
            this.IsButtonReleased(button);

        bool VRage.ModAPI.IMyInput.IsGameControlPressed(MyStringId controlEnum) => 
            this.IsGameControlPressed(controlEnum);

        bool VRage.ModAPI.IMyInput.IsGameControlReleased(MyStringId controlEnum) => 
            this.IsGameControlReleased(controlEnum);

        bool VRage.ModAPI.IMyInput.IsJoystickAxisNewPressed(MyJoystickAxesEnum axis) => 
            this.IsJoystickAxisNewPressed(axis);

        bool VRage.ModAPI.IMyInput.IsJoystickAxisPressed(MyJoystickAxesEnum axis) => 
            this.IsJoystickAxisPressed(axis);

        bool VRage.ModAPI.IMyInput.IsJoystickAxisValid(MyJoystickAxesEnum axis) => 
            this.IsJoystickAxisValid(axis);

        bool VRage.ModAPI.IMyInput.IsJoystickButtonNewPressed(MyJoystickButtonsEnum button) => 
            this.IsJoystickButtonNewPressed(button);

        bool VRage.ModAPI.IMyInput.IsJoystickButtonPressed(MyJoystickButtonsEnum button) => 
            this.IsJoystickButtonPressed(button);

        bool VRage.ModAPI.IMyInput.IsJoystickButtonValid(MyJoystickButtonsEnum button) => 
            this.IsJoystickButtonValid(button);

        bool VRage.ModAPI.IMyInput.IsJoystickConnected() => 
            this.IsJoystickConnected();

        bool VRage.ModAPI.IMyInput.IsKeyDigit(MyKeys key) => 
            this.IsKeyDigit(key);

        bool VRage.ModAPI.IMyInput.IsKeyPress(MyKeys key) => 
            this.IsKeyPress(key);

        bool VRage.ModAPI.IMyInput.IsKeyValid(MyKeys key) => 
            this.IsKeyValid(key);

        bool VRage.ModAPI.IMyInput.IsLeftMousePressed() => 
            this.IsLeftMousePressed();

        bool VRage.ModAPI.IMyInput.IsLeftMouseReleased() => 
            this.IsLeftMouseReleased();

        bool VRage.ModAPI.IMyInput.IsMiddleMousePressed() => 
            this.IsMiddleMousePressed();

        bool VRage.ModAPI.IMyInput.IsMouseButtonValid(MyMouseButtonsEnum button) => 
            this.IsMouseButtonValid(button);

        bool VRage.ModAPI.IMyInput.IsMousePressed(MyMouseButtonsEnum button) => 
            this.IsMousePressed(button);

        bool VRage.ModAPI.IMyInput.IsMouseReleased(MyMouseButtonsEnum button) => 
            this.IsMouseReleased(button);

        bool VRage.ModAPI.IMyInput.IsNewButtonPressed(MySharedButtonsEnum button) => 
            this.IsNewButtonPressed(button);

        bool VRage.ModAPI.IMyInput.IsNewButtonReleased(MySharedButtonsEnum button) => 
            this.IsNewButtonReleased(button);

        bool VRage.ModAPI.IMyInput.IsNewGameControlPressed(MyStringId controlEnum) => 
            this.IsNewGameControlPressed(controlEnum);

        bool VRage.ModAPI.IMyInput.IsNewGameControlReleased(MyStringId controlEnum) => 
            this.IsNewGameControlReleased(controlEnum);

        bool VRage.ModAPI.IMyInput.IsNewJoystickAxisReleased(MyJoystickAxesEnum axis) => 
            this.IsNewJoystickAxisReleased(axis);

        bool VRage.ModAPI.IMyInput.IsNewJoystickButtonReleased(MyJoystickButtonsEnum button) => 
            this.IsNewJoystickButtonReleased(button);

        bool VRage.ModAPI.IMyInput.IsNewKeyPressed(MyKeys key) => 
            this.IsNewKeyPressed(key);

        bool VRage.ModAPI.IMyInput.IsNewKeyReleased(MyKeys key) => 
            this.IsNewKeyReleased(key);

        bool VRage.ModAPI.IMyInput.IsNewLeftMousePressed() => 
            this.IsNewLeftMousePressed();

        bool VRage.ModAPI.IMyInput.IsNewLeftMouseReleased() => 
            this.IsNewLeftMouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewMiddleMousePressed() => 
            this.IsNewMiddleMousePressed();

        bool VRage.ModAPI.IMyInput.IsNewMiddleMouseReleased() => 
            this.IsNewMiddleMouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewMousePressed(MyMouseButtonsEnum button) => 
            this.IsNewMousePressed(button);

        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonPressed() => 
            this.IsNewPrimaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonReleased() => 
            this.IsNewPrimaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsNewRightMousePressed() => 
            this.IsNewRightMousePressed();

        bool VRage.ModAPI.IMyInput.IsNewRightMouseReleased() => 
            this.IsNewRightMouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonPressed() => 
            this.IsNewSecondaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonReleased() => 
            this.IsNewSecondaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsNewXButton1MousePressed() => 
            this.IsNewXButton1MousePressed();

        bool VRage.ModAPI.IMyInput.IsNewXButton1MouseReleased() => 
            this.IsNewXButton1MouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewXButton2MousePressed() => 
            this.IsNewXButton2MousePressed();

        bool VRage.ModAPI.IMyInput.IsNewXButton2MouseReleased() => 
            this.IsNewXButton2MouseReleased();

        bool VRage.ModAPI.IMyInput.IsPrimaryButtonPressed() => 
            this.IsPrimaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsPrimaryButtonReleased() => 
            this.IsPrimaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsRightMousePressed() => 
            this.IsRightMousePressed();

        bool VRage.ModAPI.IMyInput.IsSecondaryButtonPressed() => 
            this.IsSecondaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsSecondaryButtonReleased() => 
            this.IsSecondaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsXButton1MousePressed() => 
            this.IsXButton1MousePressed();

        bool VRage.ModAPI.IMyInput.IsXButton2MousePressed() => 
            this.IsXButton2MousePressed();

        int VRage.ModAPI.IMyInput.MouseScrollWheelValue() => 
            this.MouseScrollWheelValue();

        int VRage.ModAPI.IMyInput.PreviousMouseScrollWheelValue() => 
            this.PreviousMouseScrollWheelValue();

        bool VRage.ModAPI.IMyInput.WasKeyPress(MyKeys key) => 
            this.WasKeyPress(key);

        bool VRage.ModAPI.IMyInput.WasMiddleMousePressed() => 
            this.WasMiddleMousePressed();

        bool VRage.ModAPI.IMyInput.WasMiddleMouseReleased() => 
            this.WasMiddleMouseReleased();

        bool VRage.ModAPI.IMyInput.WasRightMousePressed() => 
            this.WasRightMousePressed();

        bool VRage.ModAPI.IMyInput.WasRightMouseReleased() => 
            this.WasRightMouseReleased();

        bool VRage.ModAPI.IMyInput.WasXButton1MousePressed() => 
            this.WasXButton1MousePressed();

        bool VRage.ModAPI.IMyInput.WasXButton1MouseReleased() => 
            this.WasXButton1MouseReleased();

        bool VRage.ModAPI.IMyInput.WasXButton2MousePressed() => 
            this.WasXButton2MousePressed();

        bool VRage.ModAPI.IMyInput.WasXButton2MouseReleased() => 
            this.WasXButton2MouseReleased();

        public bool WasGamepadKeyDownPressed()
        {
            int num;
            int num2;
            return (this.GetGamepadKeyDirections(out num, out num2) && ((num2 >= 0x34bc) && (num2 <= 0x57e4)));
        }

        public bool WasGamepadKeyLeftPressed()
        {
            int num;
            int num2;
            return (this.GetGamepadKeyDirections(out num, out num2) && ((num2 >= 0x57e4) && (num2 <= 0x7b0c)));
        }

        public bool WasGamepadKeyRightPressed()
        {
            int num;
            int num2;
            return (this.GetGamepadKeyDirections(out num, out num2) && ((num2 >= 0x1194) && (num2 <= 0x34bc)));
        }

        public bool WasGamepadKeyUpPressed()
        {
            int num;
            int num2;
            if (!this.GetGamepadKeyDirections(out num, out num2))
            {
                return false;
            }
            if ((num2 < 0) || (num2 > 0x1194))
            {
                return ((num2 >= 0x7b0c) && (num2 <= 0x8ca0));
            }
            return true;
        }

        public bool WasJoystickAxisPressed(MyJoystickAxesEnum axis)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (axis != MyJoystickAxesEnum.None)) && (this.m_previousJoystickState != null))
            {
                flag = this.GetPreviousJoystickAxisStateForGameplay(axis) > 0.5f;
            }
            if (flag || (axis != MyJoystickAxesEnum.None))
            {
                return (this.IsJoystickAxisSupported(axis) ? flag : false);
            }
            return true;
        }

        public bool WasJoystickAxisReleased(MyJoystickAxesEnum axis)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (axis != MyJoystickAxesEnum.None)) && (this.m_previousJoystickState != null))
            {
                flag = this.GetPreviousJoystickAxisStateForGameplay(axis) <= 0.5f;
            }
            if (flag || (axis != MyJoystickAxesEnum.None))
            {
                return (this.IsJoystickAxisSupported(axis) ? flag : false);
            }
            return true;
        }

        public bool WasJoystickButtonPressed(MyJoystickButtonsEnum button)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (button != MyJoystickButtonsEnum.None)) && (this.m_previousJoystickState != null))
            {
                switch (button)
                {
                    case MyJoystickButtonsEnum.JDLeft:
                        flag = this.WasGamepadKeyLeftPressed();
                        break;

                    case MyJoystickButtonsEnum.JDRight:
                        flag = this.WasGamepadKeyRightPressed();
                        break;

                    case MyJoystickButtonsEnum.JDUp:
                        flag = this.WasGamepadKeyUpPressed();
                        break;

                    case MyJoystickButtonsEnum.JDDown:
                        flag = this.WasGamepadKeyDownPressed();
                        break;

                    default:
                        flag = this.m_previousJoystickState.Buttons[((int) button) - 5];
                        break;
                }
            }
            if (flag || (button != MyJoystickButtonsEnum.None))
            {
                return flag;
            }
            return true;
        }

        public bool WasJoystickButtonReleased(MyJoystickButtonsEnum button)
        {
            bool flag = false;
            if ((this.m_joystickConnected && (button != MyJoystickButtonsEnum.None)) && (this.m_previousJoystickState != null))
            {
                switch (button)
                {
                    case MyJoystickButtonsEnum.JDLeft:
                        flag = !this.WasGamepadKeyLeftPressed();
                        break;

                    case MyJoystickButtonsEnum.JDRight:
                        flag = !this.WasGamepadKeyRightPressed();
                        break;

                    case MyJoystickButtonsEnum.JDUp:
                        flag = !this.WasGamepadKeyUpPressed();
                        break;

                    case MyJoystickButtonsEnum.JDDown:
                        flag = !this.WasGamepadKeyDownPressed();
                        break;

                    default:
                        flag = this.m_previousJoystickState.IsReleased(((int) button) - 5);
                        break;
                }
            }
            if (flag || (button != MyJoystickButtonsEnum.None))
            {
                return flag;
            }
            return true;
        }

        public bool WasKeyPress(MyKeys key) => 
            this.m_keyboardState.IsPreviousKeyDown(key);

        public bool WasLeftMousePressed() => 
            this.m_previousMouseState.LeftButton;

        public bool WasLeftMouseReleased() => 
            !this.m_previousMouseState.LeftButton;

        public bool WasMiddleMousePressed() => 
            this.m_previousMouseState.MiddleButton;

        public bool WasMiddleMouseReleased() => 
            !this.m_previousMouseState.MiddleButton;

        public bool WasMousePressed(MyMouseButtonsEnum button)
        {
            switch (button)
            {
                case MyMouseButtonsEnum.Left:
                    return this.WasLeftMousePressed();

                case MyMouseButtonsEnum.Middle:
                    return this.WasMiddleMousePressed();

                case MyMouseButtonsEnum.Right:
                    return this.WasRightMousePressed();

                case MyMouseButtonsEnum.XButton1:
                    return this.WasXButton1MousePressed();

                case MyMouseButtonsEnum.XButton2:
                    return this.WasXButton2MousePressed();
            }
            return false;
        }

        public bool WasMouseReleased(MyMouseButtonsEnum button)
        {
            switch (button)
            {
                case MyMouseButtonsEnum.Left:
                    return this.WasLeftMouseReleased();

                case MyMouseButtonsEnum.Middle:
                    return this.WasMiddleMouseReleased();

                case MyMouseButtonsEnum.Right:
                    return this.WasRightMouseReleased();

                case MyMouseButtonsEnum.XButton1:
                    return this.WasXButton1MouseReleased();

                case MyMouseButtonsEnum.XButton2:
                    return this.WasXButton2MouseReleased();
            }
            return false;
        }

        public bool WasRightMousePressed() => 
            this.m_previousMouseState.RightButton;

        public bool WasRightMouseReleased() => 
            !this.m_previousMouseState.RightButton;

        public bool WasXButton1MousePressed() => 
            this.m_previousMouseState.XButton1;

        public bool WasXButton1MouseReleased() => 
            !this.m_previousMouseState.XButton1;

        public bool WasXButton2MousePressed() => 
            this.m_previousMouseState.XButton2;

        public bool WasXButton2MouseReleased() => 
            !this.m_previousMouseState.XButton2;

        bool VRage.ModAPI.IMyInput.IsCapsLock =>
            this.IsCapsLock;

        string VRage.ModAPI.IMyInput.JoystickInstanceName =>
            this.JoystickInstanceName;

        ListReader<char> VRage.ModAPI.IMyInput.TextInput =>
            this.TextInput;

        bool VRage.ModAPI.IMyInput.JoystickAsMouse =>
            this.JoystickAsMouse;

        bool VRage.ModAPI.IMyInput.IsJoystickLastUsed =>
            this.IsJoystickLastUsed;

        public bool IsCapsLock =>
            ((((ushort) GetKeyState(20)) & 0xffff) != 0);

        public bool IsNumLock =>
            ((((ushort) GetKeyState(0x90)) & 0xffff) != 0);

        public bool IsScrollLock =>
            ((((ushort) GetKeyState(0x91)) & 0xffff) != 0);

        public MyMouseState ActualMouseState =>
            this.m_actualMouseState;

        public JoystickState ActualJoystickState =>
            this.m_actualJoystickState;

        public string JoystickInstanceName
        {
            get => 
                this.m_joystickInstanceName;
            set
            {
                if (this.m_joystickInstanceName != value)
                {
                    this.m_joystickInstanceName = value;
                    this.InitializeJoystickIfPossible();
                }
            }
        }

        public bool IsMouseXInvertedDefault =>
            false;

        public bool IsMouseYInvertedDefault =>
            false;

        public float MouseSensitivityDefault =>
            1.655f;

        public string JoystickInstanceNameDefault =>
            null;

        public float JoystickSensitivityDefault =>
            2f;

        public float JoystickExponentDefault =>
            2f;

        public float JoystickDeadzoneDefault =>
            0.2f;

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set
            {
                if (this.m_enabled != value)
                {
                    if (value)
                    {
                        this.mouse = MyDirectInput.GetMouseState();
                    }
                    this.ClearStates();
                    this.m_enabled = value;
                }
            }
        }

        public IntPtr WindowHandle =>
            this.m_windowHandle;

        public ListReader<char> TextInput =>
            new ListReader<char>(this.m_currentTextInput);

        public bool JoystickAsMouse { get; set; }

        public bool IsJoystickLastUsed { get; set; }

        public bool ENABLE_DEVELOPER_KEYS { get; private set; }

        public bool Trichording { get; set; }
    }
}

