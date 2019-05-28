namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using VRage.Input;

    public class MyKeyThrottler
    {
        private static int m_WINDOWS_CharacterInitialDelay = -1;
        private static int m_WINDOWS_CharacterRepeatDelay = -1;
        private static int m_WINDOWS_CharacterInitialDelayMs = 0;
        private static int m_WINDOWS_CharacterRepeatDelayMs = 0;
        private Dictionary<MyKeys, MyKeyThrottleState> m_keyTimeControllers = new Dictionary<MyKeys, MyKeyThrottleState>();

        private static void ComputeCharacterInitialDelay(int code, out int ms)
        {
            switch (code)
            {
                case 0:
                    ms = 250;
                    return;

                case 1:
                    ms = 500;
                    return;

                case 2:
                    ms = 750;
                    return;

                case 3:
                    ms = 0x3e8;
                    return;
            }
            ms = 500;
        }

        private static void ComputeCharacterRepeatDelay(int code, out int ms)
        {
            if ((code < 0) || (code > 0x1f))
            {
                ms = 0x19;
            }
            else
            {
                float num = 500f;
                float num3 = ((float) code) / 31f;
                ms = (int) (((1f - num3) * num) + (num3 * 25f));
            }
        }

        private MyKeyThrottleState GetKeyController(MyKeys key)
        {
            MyKeyThrottleState state;
            if (!this.m_keyTimeControllers.TryGetValue(key, out state))
            {
                state = new MyKeyThrottleState();
                this.m_keyTimeControllers[key] = state;
            }
            return state;
        }

        public ThrottledKeyStatus GetKeyStatus(MyKeys key)
        {
            if (!MyInput.Static.IsKeyPress(key))
            {
                return ThrottledKeyStatus.UNPRESSED;
            }
            MyKeyThrottleState keyController = this.GetKeyController(key);
            if (keyController != null)
            {
                if (MyInput.Static.IsNewKeyPressed(key))
                {
                    keyController.RequiredDelay = WINDOWS_CharacterInitialDelayMs;
                    keyController.LastKeyPressTime = MyGuiManager.TotalTimeInMilliseconds;
                    return ThrottledKeyStatus.PRESSED_AND_READY;
                }
                if ((MyGuiManager.TotalTimeInMilliseconds - keyController.LastKeyPressTime) <= keyController.RequiredDelay)
                {
                    return ThrottledKeyStatus.PRESSED_AND_WAITING;
                }
                keyController.RequiredDelay = WINDOWS_CharacterRepeatDelayMs;
                keyController.LastKeyPressTime = MyGuiManager.TotalTimeInMilliseconds;
            }
            return ThrottledKeyStatus.PRESSED_AND_READY;
        }

        public bool IsNewPressAndThrottled(MyKeys key)
        {
            if (!MyInput.Static.IsNewKeyPressed(key))
            {
                return false;
            }
            MyKeyThrottleState keyController = this.GetKeyController(key);
            if (keyController != null)
            {
                if ((MyGuiManager.TotalTimeInMilliseconds - keyController.LastKeyPressTime) <= WINDOWS_CharacterRepeatDelayMs)
                {
                    return false;
                }
                keyController.LastKeyPressTime = MyGuiManager.TotalTimeInMilliseconds;
            }
            return true;
        }

        public static int WINDOWS_CharacterInitialDelayMs
        {
            get
            {
                if (m_WINDOWS_CharacterInitialDelay != SystemInformation.KeyboardDelay)
                {
                    m_WINDOWS_CharacterInitialDelay = SystemInformation.KeyboardDelay;
                    ComputeCharacterInitialDelay(m_WINDOWS_CharacterInitialDelay, out m_WINDOWS_CharacterInitialDelayMs);
                }
                return m_WINDOWS_CharacterInitialDelayMs;
            }
        }

        public static int WINDOWS_CharacterRepeatDelayMs
        {
            get
            {
                if (m_WINDOWS_CharacterRepeatDelay != SystemInformation.KeyboardSpeed)
                {
                    m_WINDOWS_CharacterRepeatDelay = SystemInformation.KeyboardSpeed;
                    ComputeCharacterRepeatDelay(m_WINDOWS_CharacterRepeatDelay, out m_WINDOWS_CharacterRepeatDelayMs);
                }
                return m_WINDOWS_CharacterRepeatDelayMs;
            }
        }

        private class MyKeyThrottleState
        {
            public int LastKeyPressTime = -60000;
            public int RequiredDelay;
        }
    }
}

