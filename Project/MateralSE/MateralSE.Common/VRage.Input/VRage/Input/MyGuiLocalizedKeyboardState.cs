namespace VRage.Input
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unsharper;
    using VRage.Win32;

    [UnsharperDisableReflection]
    public class MyGuiLocalizedKeyboardState
    {
        private static HashSet<byte> m_localKeys;
        internal const uint KLF_NOTELLSHELL = 0x80;
        private MyKeyboardState m_previousKeyboardState;
        private MyKeyboardState m_actualKeyboardState = MyWindowsKeyboard.GetCurrentState();

        public MyGuiLocalizedKeyboardState()
        {
            if (m_localKeys == null)
            {
                m_localKeys = new HashSet<byte>();
                this.AddLocalKey(MyKeys.LeftControl);
                this.AddLocalKey(MyKeys.LeftAlt);
                this.AddLocalKey(MyKeys.LeftShift);
                this.AddLocalKey(MyKeys.RightAlt);
                this.AddLocalKey(MyKeys.RightControl);
                this.AddLocalKey(MyKeys.RightShift);
                this.AddLocalKey(MyKeys.Delete);
                this.AddLocalKey(MyKeys.NumPad0);
                this.AddLocalKey(MyKeys.NumPad1);
                this.AddLocalKey(MyKeys.NumPad2);
                this.AddLocalKey(MyKeys.NumPad3);
                this.AddLocalKey(MyKeys.NumPad4);
                this.AddLocalKey(MyKeys.NumPad5);
                this.AddLocalKey(MyKeys.NumPad6);
                this.AddLocalKey(MyKeys.NumPad7);
                this.AddLocalKey(MyKeys.NumPad8);
                this.AddLocalKey(MyKeys.NumPad9);
                this.AddLocalKey(MyKeys.Decimal);
                this.AddLocalKey(MyKeys.LeftWindows);
                this.AddLocalKey(MyKeys.RightWindows);
                this.AddLocalKey(MyKeys.Apps);
                this.AddLocalKey(MyKeys.Pause);
                this.AddLocalKey(MyKeys.Divide);
            }
        }

        private void AddLocalKey(MyKeys key)
        {
            m_localKeys.Add((byte) key);
        }

        public void ClearStates()
        {
            this.m_previousKeyboardState = this.m_actualKeyboardState;
            this.m_actualKeyboardState = new MyKeyboardState();
        }

        public MyKeyboardState GetActualKeyboardState() => 
            this.m_actualKeyboardState;

        public void GetActualPressedKeys(List<MyKeys> keys)
        {
            this.m_actualKeyboardState.GetPressedKeys(keys);
            for (int i = 0; i < keys.Count; i++)
            {
                if (!this.IsKeyLocal(keys[i]))
                {
                    keys[i] = USEnglishToLocal(keys[i]);
                }
            }
        }

        public MyKeyboardState GetPreviousKeyboardState() => 
            this.m_previousKeyboardState;

        public bool IsAnyKeyPressed() => 
            this.m_actualKeyboardState.IsAnyKeyPressed();

        public bool IsKeyDown(MyKeys key) => 
            this.IsKeyDown(key, this.IsKeyLocal(key));

        public bool IsKeyDown(MyKeys key, bool isLocalKey)
        {
            if (!isLocalKey)
            {
                MyKeys keys1 = LocalToUSEnglish(key);
                key = keys1;
            }
            return this.m_actualKeyboardState.IsKeyDown(key);
        }

        private bool IsKeyLocal(MyKeys key) => 
            m_localKeys.Contains((byte) key);

        public bool IsKeyUp(MyKeys key) => 
            this.IsKeyUp(key, this.IsKeyLocal(key));

        public bool IsKeyUp(MyKeys key, bool isLocalKey)
        {
            if (!isLocalKey)
            {
                MyKeys keys1 = LocalToUSEnglish(key);
                key = keys1;
            }
            return this.m_actualKeyboardState.IsKeyUp(key);
        }

        public bool IsPreviousKeyDown(MyKeys key) => 
            this.IsPreviousKeyDown(key, this.IsKeyLocal(key));

        public bool IsPreviousKeyDown(MyKeys key, bool isLocalKey)
        {
            if (!isLocalKey)
            {
                MyKeys keys1 = LocalToUSEnglish(key);
                key = keys1;
            }
            return this.m_previousKeyboardState.IsKeyDown(key);
        }

        public bool IsPreviousKeyUp(MyKeys key) => 
            this.IsPreviousKeyUp(key, this.IsKeyLocal(key));

        public bool IsPreviousKeyUp(MyKeys key, bool isLocalKey)
        {
            if (!isLocalKey)
            {
                MyKeys keys1 = LocalToUSEnglish(key);
                key = keys1;
            }
            return this.m_previousKeyboardState.IsKeyUp(key);
        }

        public static MyKeys LocalToUSEnglish(MyKeys key) => 
            key;

        public void NegateEscapePress()
        {
            this.m_previousKeyboardState.SetKey(MyKeys.Escape, true);
            this.m_actualKeyboardState.SetKey(MyKeys.Escape, true);
        }

        public void SetKey(MyKeys key, bool value)
        {
            this.m_actualKeyboardState.SetKey(key, value);
        }

        public void UpdateStates()
        {
            this.m_previousKeyboardState = this.m_actualKeyboardState;
            this.m_actualKeyboardState = MyWindowsKeyboard.GetCurrentState();
        }

        public void UpdateStatesFromSnapshot(MyKeyboardState state)
        {
            this.m_previousKeyboardState = this.m_actualKeyboardState;
            this.m_actualKeyboardState = state;
        }

        public void UpdateStatesFromSnapshot(MyKeyboardState currentState, MyKeyboardState previousState)
        {
            this.m_previousKeyboardState = previousState;
            this.m_actualKeyboardState = currentState;
        }

        public static MyKeys USEnglishToLocal(MyKeys key) => 
            key;

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardLayout : IDisposable
        {
            public readonly IntPtr Handle;
            public static MyGuiLocalizedKeyboardState.KeyboardLayout US_English;
            public KeyboardLayout(IntPtr handle)
            {
                this = new MyGuiLocalizedKeyboardState.KeyboardLayout();
                this.Handle = handle;
            }

            public KeyboardLayout(string keyboardLayoutID) : this(WinApi.LoadKeyboardLayout(keyboardLayoutID, 0x80))
            {
            }

            public bool IsDisposed { get; private set; }
            public void Dispose()
            {
                if (!this.IsDisposed)
                {
                    WinApi.UnloadKeyboardLayout(this.Handle);
                    this.IsDisposed = true;
                }
            }

            public static MyGuiLocalizedKeyboardState.KeyboardLayout Active =>
                new MyGuiLocalizedKeyboardState.KeyboardLayout(WinApi.GetKeyboardLayout(IntPtr.Zero));
            static KeyboardLayout()
            {
                US_English = new MyGuiLocalizedKeyboardState.KeyboardLayout("00000409");
            }
        }
    }
}

