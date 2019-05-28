namespace VRage.Input
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyKeyboardState
    {
        private MyKeyboardBuffer m_buffer;
        public void GetPressedKeys(List<MyKeys> keys)
        {
            keys.Clear();
            for (int i = 1; i < 0xff; i++)
            {
                if (this.m_buffer.GetBit((byte) i))
                {
                    keys.Add((MyKeys) ((byte) i));
                }
            }
        }

        public bool IsAnyKeyPressed() => 
            this.m_buffer.AnyBitSet();

        public void SetKey(MyKeys key, bool value)
        {
            this.m_buffer.SetBit((byte) key, value);
        }

        public static MyKeyboardState FromBuffer(MyKeyboardBuffer buffer) => 
            new MyKeyboardState { m_buffer = buffer };

        public bool IsKeyDown(MyKeys key) => 
            this.m_buffer.GetBit((byte) key);

        public bool IsKeyUp(MyKeys key) => 
            !this.IsKeyDown(key);

        public void AddKey(MyKeys key, bool value)
        {
            this.m_buffer.SetBit((byte) key, true);
        }
    }
}

