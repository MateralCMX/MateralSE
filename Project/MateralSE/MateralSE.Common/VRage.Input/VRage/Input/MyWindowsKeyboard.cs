namespace VRage.Input
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class MyWindowsKeyboard
    {
        private static unsafe void CopyBuffer(byte* windowsKeyData, ref MyKeyboardBuffer buffer)
        {
            for (int i = 0; i < 0x100; i++)
            {
                if ((windowsKeyData[i] & 0x80) != 0)
                {
                    buffer.SetBit((byte) i, true);
                }
            }
        }

        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int keyCode);
        public static MyKeyboardState GetCurrentState()
        {
            MyKeyboardBuffer buffer = new MyKeyboardBuffer();
            for (int i = 0; i < 0x100; i++)
            {
                if ((((ushort) GetAsyncKeyState(i)) >> 15) != 0)
                {
                    buffer.SetBit((byte) i, true);
                }
            }
            if (buffer.GetBit(0xa5))
            {
                buffer.SetBit(0xa2, false);
                buffer.SetBit(0x11, false);
            }
            return MyKeyboardState.FromBuffer(buffer);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        private static extern unsafe bool GetKeyboardState(byte* data);
    }
}

