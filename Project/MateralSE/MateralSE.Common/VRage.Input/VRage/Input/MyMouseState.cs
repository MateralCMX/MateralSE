namespace VRage.Input
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), Obfuscation(Feature="cw symbol renaming", Exclude=true)]
    public struct MyMouseState
    {
        public int X;
        public int Y;
        public int ScrollWheelValue;
        public bool LeftButton;
        public bool RightButton;
        public bool MiddleButton;
        public bool XButton1;
        public bool XButton2;
        public MyMouseState(int x, int y, int scrollWheel, bool leftButton, bool middleButton, bool rightButton, bool xButton1, bool xButton2)
        {
            this.X = x;
            this.Y = y;
            this.ScrollWheelValue = scrollWheel;
            this.LeftButton = leftButton;
            this.MiddleButton = middleButton;
            this.RightButton = rightButton;
            this.XButton1 = xButton1;
            this.XButton2 = xButton2;
        }

        public void ClearPosition()
        {
            this.X = 0;
            this.Y = 0;
        }
    }
}

